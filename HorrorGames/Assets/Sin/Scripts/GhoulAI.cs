using UnityEngine;
using UnityEngine.AI;
using Mirror;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class GhoulAI : NetworkBehaviour
{
    public enum GhoulState
    {
        Idle,       // 待機（見つける前）
        Chasing     // 追跡中
    }

    [Header("索敵設定")]
    [Tooltip("プレイヤーを検知して追いかけ始める距離")]
    public float detectionRange = 30f;
    [Tooltip("追跡を諦める距離")]
    public float loseSightRange = 20f;

    [Header("コンポーネント")]
    public Animator animator;

    // 現在の状態
    [SyncVar(hook = nameof(OnStateChanged))]
    private GhoulState currentState = GhoulState.Idle;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private float targetSearchTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // NavMeshAgentと競合しないよう物理演算を無効化
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // 初期状態の反映
        ApplyStateEffects(currentState);
    }

    void Update()
    {

        // ロジック処理はサーバー（ホスト）のみ実行
        //       if (!isServer) return;

        UpdateTarget();

        float distanceToPlayer = targetPlayer != null ? Vector3.Distance(transform.position, targetPlayer.position) : Mathf.Infinity;

        // --- 状態遷移ロジック ---
        switch (currentState)
        {
            case GhoulState.Idle:
                // プレイヤーが範囲内に入ったら追跡開始
                if (targetPlayer != null && distanceToPlayer <= detectionRange)
                {
                    ChangeState(GhoulState.Chasing);
                    Debug.Log("反応確認");
                }
                break;

            case GhoulState.Chasing:
                // --- 現在の状態に応じた行動 ---
                if (distanceToPlayer > loseSightRange)
                {
                    if (targetPlayer)
                        targetPlayer = null;
                    if (targetPlayer != null)
                    {
                        // 常にプレイヤーを追いかける
                        if (agent != null && agent.isOnNavMesh)
                        {
                            agent.SetDestination(targetPlayer.position);
                        }
                    }
                    else
                    {
                        // 
                        if (agent != null && agent.isOnNavMesh)
                        {
                            agent.SetDestination(transform.position);
                        }
                        ChangeState(GhoulState.Idle);
                    }
                }
                break;
        }



    }

    // 定期的に最も近いプレイヤーを探す
    void UpdateTarget()
    {
        targetSearchTimer -= Time.deltaTime;

        if (targetSearchTimer <= 0f)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log(players.Length);
            float closestDistance = Mathf.Infinity;
            Transform closestPlayer = null;

            foreach (GameObject p in players)
            {
                // 自分自身や親・子オブジェクトはターゲットにしない
                if (p != null /*&& p.transform.root != this.transform.root*/)
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

        // サーバー側で自身の見た目も更新
        ApplyStateEffects(newState);
    }

    // クライアント側で状態が同期された時に呼ばれる（見た目の更新）
    void OnStateChanged(GhoulState oldState, GhoulState newState)
    {
        ApplyStateEffects(newState);
    }

    // アニメーションや移動のオンオフなどの見た目を適用する
    void ApplyStateEffects(GhoulState state)
    {
        Debug.Log(state);
        switch (state)
        {
            case GhoulState.Idle:
                Debug.Log("55555555555555555555555555");
                if (animator != null) animator.SetFloat("Speed", 0f);
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero; // ピタッと止める
                }
                break;

            case GhoulState.Chasing:
                if (animator != null) animator.SetFloat("Speed", 1f);
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
                break;
        }
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

        this.enabled = false;
    }
}
