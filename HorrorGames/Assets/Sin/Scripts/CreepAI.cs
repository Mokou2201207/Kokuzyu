using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class CreepAI : MonoBehaviour
{
    [Header("ターゲット設定")]
    [Tooltip("追いかける対象（Player）")]
    public Transform player;
    [Tooltip("プレイヤーを検知して追いかけ始める距離")]
    public float chaseDistance = 15f;

    [Header("徘徊（ウロウロ）設定")]
    [Tooltip("ウロウロする範囲の半径")]
    public float wanderRadius = 10f;
    [Tooltip("次の目的地を決めるまでの時間（秒）")]
    public float wanderWaitTime = 3f;

    private NavMeshAgent agent;
    private float wanderTimer;

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

        // プレイヤーとCreep2の距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= chaseDistance)
        {
            // プレイヤーが近い場合：プレイヤーをターゲットにして追いかける
            agent.SetDestination(player.position);
            
            // 追いかけている時はタイマーをリセットし、見失ったときにすぐ次の徘徊ポイントを探すようにする
            wanderTimer = wanderWaitTime; 
        }
        else
        {
            // プレイヤーが遠い場合：周辺をウロウロする（徘徊）
            Wander();
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
