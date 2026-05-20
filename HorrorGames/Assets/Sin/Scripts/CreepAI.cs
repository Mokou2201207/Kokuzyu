using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class CreepAI : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("追いかける対象（Player）")]
    public Transform player;
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

    private NavMeshAgent agent;
    private float wanderTimer;
    private float stateTimer;
    private bool isChasing = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderTimer = wanderWaitTime;

        // Playerという名前のオブジェクトを自動的に探して設定する
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Playerという名前のオブジェクトが見つかりませんでした。");
            }
        }

        // Animatorが設定されていない場合、子オブジェクトから取得する
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // RigidbodyとNavMeshAgentが物理演算で干渉しないように、RigidbodyをKinematicに設定
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 状態のタイマーを更新
        stateTimer += Time.deltaTime;

        if (isChasing)
        {
            // 追いかける時間が経過したら徘徊に切り替え
            if (stateTimer >= chaseDuration)
            {
                isChasing = false;
                stateTimer = 0f;
                wanderTimer = wanderWaitTime; // すぐに徘徊の目的地を決めるようにリセット
                
                if (animator != null)
                {
                    animator.SetBool("Change", false);
                }
            }
            else
            {
                // プレイヤーをターゲットにして追いかける
                agent.SetDestination(player.position);

                if (animator != null)
                {
                    animator.SetBool("Change", true);
                }
            }
        }
        else
        {
            // 徘徊する時間が経過したら追いかける状態に切り替え
            if (stateTimer >= wanderDuration)
            {
                isChasing = true;
                stateTimer = 0f;
            }
            else
            {
                // 周辺をウロウロする（徘徊）
                Wander();

                if (animator != null)
                {
                    animator.SetBool("Change", false);
                }
            }
        }
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        // 指定した時間が経過したか、目的地付近に到着した場合に次のランダムな目的地を設定
        if (wanderTimer >= wanderWaitTime || (agent.pathPending == false && agent.remainingDistance <= agent.stoppingDistance))
        {
            Vector3 newPos = GetRandomNavMeshPosition(transform.position, wanderRadius);
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
}
