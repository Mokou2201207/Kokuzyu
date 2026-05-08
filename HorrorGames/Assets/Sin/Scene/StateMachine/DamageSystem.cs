using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public Parameta m_Parameta;
    public int DMG = 1;

    public void Update()
    {
        if (!m_Parameta)
            return;
        if (m_Parameta.m_IsPlayer)
            return;

        if (m_Parameta.m_ArmsOnFlag)
        {
            GetComponent<BoxCollider>().enabled = true;
        }
        else
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Parameta>())
        {
            Parameta P = other.GetComponent<Parameta>();
            if (P != m_Parameta)
            {
                //ダメージを与える
                if (P.TakeDamage(DMG))
                {
                    //死亡している場合は、ターゲットから除外
                    if (m_Parameta)
                        if (m_Parameta.m_BTA)
                            if (m_Parameta.m_BTA.m_Player)
                                m_Parameta.m_BTA.m_Player = null;
                }
            }
        }
    }
}
