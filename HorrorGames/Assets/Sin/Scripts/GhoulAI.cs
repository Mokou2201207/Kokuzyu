using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class GhoulAI : NetworkBehaviour
{
    public enum GhoulState
    {
        Crying,     // 泣いている（待機）
        Chasing,    // プレイヤーを見つけて追いかける
        Attacking   // プレイヤーに接近して攻撃する
    }

    [Header("ターゲット・索敵設定")]
    [Tooltip("プレイヤーを検知して追いかけ始める距離")]
    public float detectionRange = 10f;
    [Tooltip("攻撃を行う距離")]
    public float attackRange = 2f;
    [Tooltip("追跡を諦める距離")]
    public float loseSightRange = 20f;

    [Header("アニメーション")]
    [Tooltip("Animatorコンポーネント（指定がない場合は子から自動取得）")]
    public Animator animator;

    [Header("サウンド設定")]
    public AudioSource audioSource;
    [Tooltip("泣き声の音声クリップ（ループ再生を推奨）")]
    public AudioClip cryingClip;
    [Tooltip("プレイヤーを見つけて襲い掛かる時の声")]
    public AudioClip roarClip;
    [Tooltip("攻撃時の音声（オプション）")]
    public AudioClip attackClip;

    [Header("攻撃設定")]
    [Tooltip("攻撃の間隔（秒）")]
    public float attackCooldown = 2f;
    [Tooltip("攻撃モーション中に立ち止まる時間（秒）")]
    public float attackActionDuration = 1.5f;

    // サーバーとクライアント間で現在の状態を同期する
    [SyncVar(hook = nameof(OnStateChanged))]
    private GhoulState currentState = GhoulState.Crying;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private float targetSearchTimer = 0f;
    // 攻撃のクールダウンタイマー
    private float currentAttackCooldown = 0f;
    // 攻撃アニメーション中の硬直タイマー
    private float attackActionTimer = 0f;
    private bool hasRoared = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // NavMeshAgentと競合しないようにする
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1f; // 3Dサウンド（距離に応じて音量変化）
            audioSource.maxDistance = 20f;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("GhoulAI: Animatorが見つかりません。");
            }
        }

        // 初期状態（泣いている状態）の反映
        ApplyStateEffects(currentState);
    }

    void Update()
    {
        // 状態遷移や移動ロジックはサーバーのみで処理する
        if (!isServer) return;

        UpdateTarget();

        float distanceToPlayer = targetPlayer != null ? Vector3.Distance(transform.position, targetPlayer.position) : Mathf.Infinity;

        // 全体の攻撃クールダウンを減らす（攻撃硬直中以外でカウント）
        if (currentState != GhoulState.Attacking && currentAttackCooldown > 0f)
        {
            currentAttackCooldown -= Time.deltaTime;
        }

        // --- 状態遷移のロジック ---
        switch (currentState)
        {
            case GhoulState.Crying:
                // プレイヤーが近づいたら追跡開始
                if (targetPlayer != null && distanceToPlayer <= detectionRange)
                {
                    ChangeState(GhoulState.Chasing);
                }
                break;

            case GhoulState.Chasing:
                // 遠くへ離れたら諦めて泣きに戻る
                if (targetPlayer == null || distanceToPlayer > loseSightRange)
                {
                    ChangeState(GhoulState.Crying);
                }
                // 十分に近づき、かつクールダウンが明けていれば攻撃開始
                else if (distanceToPlayer <= attackRange && currentAttackCooldown <= 0f)
                {
                    ChangeState(GhoulState.Attacking);
                }
                break;

            case GhoulState.Attacking:
                // 攻撃モーション中の硬直時間を減らす
                attackActionTimer -= Time.deltaTime;
                if (attackActionTimer <= 0f)
                {
                    // 攻撃の硬直が終わったらクールダウンを開始し、追跡状態に戻る
                    currentAttackCooldown = attackCooldown;
                    ChangeState(GhoulState.Chasing);
                }
                break;
        }

        // --- 現在の状態に応じた行動 ---
        ExecuteStateAction();
    }

    // 定期的に最も近いプレイヤーを探す
    void UpdateTarget()
    {
        targetSearchTimer -= Time.deltaTime;
        if (targetSearchTimer <= 0f)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            float closestDistance = Mathf.Infinity;
            Transform closestPlayer = null;

            foreach (GameObject p in players)
            {
                // 自分自身（または自分の親・子オブジェクト）がPlayerタグを持っている場合は除外する
                if (p != null && p.transform.root != this.transform.root)
                {
                    float dist = Vector3.Distance(transform.position, p.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestPlayer = p.transform;
                    }
                }
            }
            targetPlayer = closestPlayer;
            targetSearchTimer = 1f; // 1秒ごとに索敵
        }
    }

    void ChangeState(GhoulState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"【GhoulAI】状態が遷移しました: {currentState} -> {newState} / ターゲット: {(targetPlayer != null ? targetPlayer.name : "なし")}");

        currentState = newState;

        // サーバー側で攻撃状態に遷移した瞬間の処理
        if (newState == GhoulState.Attacking)
        {
            // 硬直タイマーをセット
            attackActionTimer = attackActionDuration;

            // クライアント全員に攻撃アニメーションと音声をトリガーさせる
            RpcTriggerAttack();
        }

        // サーバー側でも自身の演出を反映
        ApplyStateEffects(newState);
    }

    // SyncVarのフック：クライアント側で状態が変わった時に呼ばれる
    void OnStateChanged(GhoulState oldState, GhoulState newState)
    {
        ApplyStateEffects(newState);
    }

    // アニメーションやオーディオなど、見た目と音の演出を適用する
    void ApplyStateEffects(GhoulState state)
    {
        if (animator == null) return;

        switch (state)
        {
            case GhoulState.Crying:
                hasRoared = false;
                animator.SetFloat("Speed", 0f); // 待機アニメーション（Idle）にする
                PlayAudioLoop(cryingClip);
                if (isServer && agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                break;

            case GhoulState.Chasing:
                animator.SetFloat("Speed", 1f); // 走りアニメーション（Run）にする
                if (!hasRoared)
                {
                    PlayAudioOneShot(roarClip);
                    hasRoared = true;
                }
                if (isServer && agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
                break;

            case GhoulState.Attacking:
                animator.SetFloat("Speed", 0f); // 攻撃時は立ち止まる
                if (isServer && agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }
                break;
        }
    }

    // サーバーのみが実行する移動・攻撃の実処理
    void ExecuteStateAction()
    {
        switch (currentState)
        {
            case GhoulState.Chasing:
                if (targetPlayer != null && agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(targetPlayer.position);
                }
                break;

            case GhoulState.Attacking:
                if (targetPlayer != null)
                {
                    // 攻撃中は移動しないが、プレイヤーの方を振り向く
                    Vector3 direction = (targetPlayer.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                    }
                }
                break;
        }
    }

    [ClientRpc]
    void RpcTriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackClip != null)
        {
            PlayAudioOneShot(attackClip);
        }
    }

    void PlayAudioLoop(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = true;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void PlayAudioOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.loop = false;
        audioSource.Stop(); // 泣き声などのループを止める
        audioSource.PlayOneShot(clip);
    }

    // 既存システム連携用：プレイヤーを捕まえた（ゲームオーバー時など）に呼ばれる処理
    public void OnCaughtPlayer()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        this.enabled = false;
    }
}
