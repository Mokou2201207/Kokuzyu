using UnityEngine;

public class ARM_OutPoint : MonoBehaviour
{
    public ARM m_ARM;
    public PlayerAction m_PA;
    void Start()
    {
        if (!m_ARM)
        {
            m_ARM = transform.GetChild(0).GetComponent<ARM>();

            if (m_PA)
            {
                m_PA.m_ARM = m_ARM;
                m_ARM.PickUpUnit(m_PA);
            }
        }
    }
    public void OnFire()
    {
        
    }
}
