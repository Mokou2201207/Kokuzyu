using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class CreepAI : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("追いかける対象（Player）")]
    public Transform player;
    [Tooltip("プレイヤーを検知して追いかける範囲（距離）")]
    public float chaseDistance = 15f;
    [Tooltip("ターゲットを追いかける時間（秒）")]
    public float chaseDuration = 10f;

    [Header("徘徊（ウロウロ）設定")]
    [Tooltip("ウロウロする時間（秒）")]
    public float wanderDuration = 10f;
    [Tooltip("ウロウロする範囲の半径")]
    public float wanderRadius = 10f;
    [Tooltip("次の目的地を決めるまでの時間（秒）")]
    public float wanderWaitTime = 3f;

    [Header("アニメーション設定")]
    [Tooltip("子オブジェクトにあるAnimatorを指定（指定がない場合は自動取得）")]
    public Animator animator;

    [Header("サウンド設定")]
    [Tooltip("オーディオソース（指定がない場合は自動取得または追加）")]
    public AudioSource audioSource;
    [Tooltip("鳴き声の音声クリップ")]
    public AudioClip roarClip;
    [Tooltip("足音の音声クリップ")]
    public AudioClip footstepClip;
    [Tooltip("鳴き声を再生する間隔（秒）")]
    public float roarInterval = 5f;
    [Tooltip("足音を再生する間隔（秒）")]
    public float footstepInterval = 0.5f;
    [Tooltip("音が最大音量で聞こえる距離")]
    public float soundMinDistance = 2f;
    [Tooltip("音が聞こえなくなる最大距離")]
    public float soundMaxDistance = 20f;

    private float roarTimer;
    private float footstepTimer;

    private NavMeshAgent agent;
    private float wanderTimer;
    private float stateTimer;
    private bool isChasingPhase = true; // 交互モードの時に追跡フェーズかどうか

    // 現在のアニメーション状態を記憶して、無駄な呼び出しを防ぐ
    private bool currentAnimState = false;

    // 徘徊の中心点と、現在徘徊中かどうかを判定するフラグ
    private Vector3 wanderCenter;
    private bool isWanderingNow = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderWaitTime;

        // Playerタグのオブジェクトを自動的に探して設定する
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Playerタグのオブジェクトが見つかりませんでした。");
            }
        }

        // Animatorが設定されていない場合、子オブジェクトから取得する
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("【エラー】子オブジェクトにAnimatorが見つかりません！");
            }
            else
            {
                Debug.Log("【確認】Animatorを自動取得しました：" + animator.gameObject.name);
            }
        }
        else
        {
            Debug.Log("【確認】インスペクターからAnimatorが設定されています：" + animator.gameObject.name);
        }

        // 最初のアニメーション状態を確実に反映する
        if (animator != null)
        {
            animator.SetBool("Change", currentAnimState);
        }

        // RigidbodyとNavMeshAgentが物理演算で干渉しないように、RigidbodyをKinematicに設定
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // オーディオソースの取得または追加
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        // Unityの標準の3D音響機能ではなく、スクリプトでPlayerとの距離を直接計算して
        // 音量を調整するため、3D設定（spatialBlend）はオフ(0)にしておきます。
        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f; 
        }
        
        // 最初からすぐ鳴く/足音が鳴るようにタイマーを初期化（カウントダウン方式）
        roarTimer = 0f;
        footstepTimer = 0f;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // --- 距離に応じた音量の自動調整（明示的なスクリプト制御） ---
        if (audioSource != null)
        {
            if (distanceToPlayer <= soundMinDistance)
            {
                audioSource.volume = 1f; // 近い時は最大音量
            }
            else if (distanceToPlayer >= soundMaxDistance)
            {
                audioSource.volume = 0f; // 遠い時は無音
            }
            else
            {
                // MinとMaxの間で、距離が離れるほど徐々に音量を下げる（割合計算）
                audioSource.volume = 1f - ((distanceToPlayer - soundMinDistance) / (soundMaxDistance - soundMinDistance));
            }
        }

        HandleAudio();

        if (distanceToPlayer <= chaseDistance)
        {
            // --- 範囲内の場合：常に追跡 ---
            agent.SetDestination(player.position);
            SetAnimation(true);

            // 範囲外に出たときに備えてリセット（範囲外に出た直後は「10秒追う」からスタート）
            stateTimer = 0f;
            isChasingPhase = true;
            isWanderingNow = false;
        }
        else
        {
            // --- 範囲外の場合：「10秒追跡、10秒徘徊」の繰り返しモード ---
            stateTimer += Time.deltaTime;

            // タイマーによるフェーズ（状態）の切り替え
            if (isChasingPhase)
            {
                if (stateTimer >= chaseDuration)
                {
                    isChasingPhase = false;
                    stateTimer = 0f;
                    wanderTimer = wanderWaitTime; // 徘徊開始時にすぐ目的地を決めるため
                }
            }
            else
            {
                if (stateTimer >= wanderDuration)
                {
                    isChasingPhase = true;
                    stateTimer = 0f;
                }
            }

            // 現在のフェーズに応じた実際の行動
            if (isChasingPhase)
            {
                // 範囲外での追跡行動
                agent.SetDestination(player.position);
                SetAnimation(true);
                isWanderingNow = false;
            }
            else
            {
                // 範囲外での徘徊行動
                if (!isWanderingNow)
                {
                    // 新たに徘徊を始めた瞬間、その場所を「徘徊の中心」に設定する
                    isWanderingNow = true;
                    wanderCenter = transform.position;
                }
                Wander(wanderCenter);
                SetAnimation(false);
            }
        }
    }

    void HandleAudio()
    {
        if (audioSource == null) return;

        // 足音の処理：Agentが動いている時のみ再生
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (footstepClip != null)
                {
                    audioSource.PlayOneShot(footstepClip);
                    // クリップの長さ ＋ インターバルの時間だけ待つ（確実に1回鳴り終わってから次を鳴らす）
                    footstepTimer = footstepClip.length + footstepInterval;
                }
                else
                {
                    footstepTimer = footstepInterval;
                }
            }
        }
        else
        {
            footstepTimer = 0f; // 動き出したらすぐに鳴るようにリセット
        }

        // 鳴き声の処理：追跡中（アニメーションがChange=trueの時）に定期的に再生
        if (currentAnimState)
        {
            roarTimer -= Time.deltaTime;
            if (roarTimer <= 0f)
            {
                if (roarClip != null)
                {
                    audioSource.PlayOneShot(roarClip);
                    // クリップの長さ ＋ インターバルの時間だけ待つ（確実に1回鳴り終わってから次を鳴らす）
                    roarTimer = roarClip.length + roarInterval;
                }
                else
                {
                    roarTimer = roarInterval;
                }
            }
        }
        else
        {
            roarTimer = 0f; // 追跡を開始したらすぐに鳴くようにリセット
        }
    }

    // アニメーションの切り替えを管理する専用メソッド
    void SetAnimation(bool chaseAnim)
    {
        if (animator == null) return;

        // 今のアニメーション状態から変化があった時だけSetBoolを実行する
        if (currentAnimState != chaseAnim)
        {
            Debug.Log($"【アニメーション切り替え】Change = {chaseAnim} に変更しました！");
            animator.SetBool("Change", chaseAnim);
            currentAnimState = chaseAnim;
        }
    }

    void Wander(Vector3 center)
    {
        wanderTimer += Time.deltaTime;

        // 指定した時間が経過したか、目的地付近に到着した場合に次のランダムな目的地を設定
        if (wanderTimer >= wanderWaitTime || (agent.pathPending == false && agent.remainingDistance <= agent.stoppingDistance))
        {
            // 現在地ではなく、常に center（記憶した中心点）を基準にしてランダムな目的地を決める
            Vector3 newPos = GetRandomNavMeshPosition(center, wanderRadius);
            agent.SetDestination(newPos);
            wanderTimer = 0f;
        }
    }

    // 指定した半径内のNavMesh上のランダムな座標を取得する
    Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        // ランダムな方向ベクトルを作成
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        NavMeshHit navHit;
        // 取得した座標に最も近いNavMesh上のポイントを探す
        if (NavMesh.SamplePosition(randomDirection, out navHit, distance, NavMesh.AllAreas))
        {
            return navHit.position;
        }

        // 見つからなかった場合は現在の位置を返す
        return origin;
    }

    // プレイヤーを捕まえた時に外部（EnemyAttackスクリプト等）から呼ばれる処理
    public void OnCaughtPlayer()
    {
        // 1. 移動を完全に止める（慣性で滑るのも防ぐ）
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero; // ピタッと止める
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 2. 再生中の歩く音や声をピタッと止める
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // 3. このスクリプト自体の機能をOFFにして、再び足音が鳴るのを防ぐ
        this.enabled = false;
    }
}
