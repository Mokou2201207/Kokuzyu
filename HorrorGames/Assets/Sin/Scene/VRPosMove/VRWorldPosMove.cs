using UnityEngine;

public class VRWorldPosMove : MonoBehaviour
{
    public Transform m_Master;
    public Transform m_MasterWorld;
    public Transform m_SubWorld;
    void Start()
    {
        
    }

    void Update()
    {
        MasterWorldRotation();
        SubWorldMove();
    }
    public void MasterWorldRotation()
    {
        m_MasterWorld.Rotate(new Vector3(0, -Input.GetAxis("Horizontal"), 0));
    }
    public void SubWorldMove()
    {

        // ƒ[ƒ‹ƒhÀ•WŠî€‚ÅˆÚ“®
        Vector3 move = new Vector3(0, 0, -Input.GetAxis("Vertical"));
        m_SubWorld.Translate(move * 2.0f * Time.deltaTime, Space.World);
    }
}
