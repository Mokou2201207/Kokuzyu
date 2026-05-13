using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Playerをアタッチ")]
    public Transform target;

    [Header("コンポーネント（自動）")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (target != null)
        {
            //追跡処理
            agent.destination = target.position;
        }
    }

    /// <summary>
    /// トリガーに入ったら攻撃
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            anim.SetBool("Change", true);
        }
    }

    /// <summary>
    /// トリガーから外れたら攻撃停止
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            anim.SetBool("Change", false);

            agent.destination = transform.position;
        }
    }
}