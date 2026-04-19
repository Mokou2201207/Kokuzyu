using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [Header("プレイヤー"), SerializeField]
    private CharacterController controller;
    [Header("移動速度"), SerializeField]
    private float speed = 12f;
    [Header("重力"), SerializeField]
    private float gravity = -40f;
    [Header("ジャンプ力"), SerializeField]
    private float JumpHight = 1;
    Vector3 V;
    private void Awake()
    {
        //格納
        controller = gameObject.GetComponent<CharacterController>();
    }


    void Update()
    {
        if (controller.isGrounded && V.y < 0)
        {
            V.y = -2f;
        }
        //入力
        float X = Input.GetAxis("Horizontal");
        float Z = Input.GetAxis("Vertical");
        //移動
        Vector3 M = transform.right * X + transform.forward * Z;
        controller.Move(M * speed * Time.deltaTime);
        //ジャンプ
        if (Input.GetButtonDown("Jump"))
        {
            V.y = Mathf.Sqrt(JumpHight * -2f * gravity);
        }
        V.y += gravity * Time.deltaTime;
        controller.Move(V * Time.deltaTime);


    }

}

