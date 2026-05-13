using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [Header("プレイヤー"), SerializeField]
    private CharacterController controller;

    [Header("移動速度")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    [Header("重力"), SerializeField]
    private float gravity = -40f;
    [Header("ジャンプ力"), SerializeField]
    private float JumpHight = 1;

    //現在のスピードを保管変数
    private float speed;
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

        //shiftで移動速度変化
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed=runSpeed;
        }
        else
        {
            speed = walkSpeed;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        // 移動実行
        controller.Move(move * speed * Time.deltaTime);

        //ジャンプ
        if (Input.GetButtonDown("Jump"))
        {
            V.y = Mathf.Sqrt(JumpHight * -2f * gravity);
        }
        V.y += gravity * Time.deltaTime;
        controller.Move(V * Time.deltaTime);
    }

}

