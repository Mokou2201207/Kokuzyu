using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerZiken : NetworkBehaviour
{
    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            CmdMoveSphere(x, z);
        }
    }

    [Command]
    void CmdMoveSphere(float x,float z)
    {
        Vector3 v=new Vector3(x,0,z)*5f;
        GetComponent<Rigidbody>().AddForce(v);
    }
}
