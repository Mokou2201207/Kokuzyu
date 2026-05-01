using UnityEngine;

public class ARM : MonoBehaviour
{
    [Header("弾オブジェクト")]
    public GameObject m_Tama;
    [Header("銃口位置")]
    public Transform m_Jyuko;
    [Header("銃口ブレ")]
    public Vector2 m_Bure;
    [Header("所有者")]
    public PlayerAction m_PA;

    public void OnFire()
    {
        if (!m_PA)
            return;

        GameObject Dummy = Instantiate(m_Tama, m_Jyuko.position, m_Jyuko.rotation);

        if (Dummy.GetComponent<DamageSystem>() && m_PA.GetComponent<Parameta>())
        {
            Dummy.GetComponent<DamageSystem>().m_Parameta = m_PA.GetComponent<Parameta>();
            Dummy.transform.Rotate(new Vector3(
                Random.Range(m_Bure.y, -m_Bure.y),
                Random.Range(m_Bure.x, -m_Bure.x),
                0));
            Dummy.GetComponent<Rigidbody>().AddForce(Dummy.transform.forward * 1000.0f);
            Destroy(Dummy, 3.0f);
        }
    }
    public void PickUpUnit(PlayerAction PA)
    {
        m_PA = PA;
    }
    public void DropUnit()
    {
        m_PA = null;
    }
}
