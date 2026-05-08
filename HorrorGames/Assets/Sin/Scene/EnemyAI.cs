using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform target;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        transform.LookAt(target);

        if (target != null)
        {
            // ターゲットの位置を目的地として設定
            agent.destination = target.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            Debug.Log("プレイヤー発見！");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            Debug.Log("プレイヤーが範囲外へ");
        }
    }
}