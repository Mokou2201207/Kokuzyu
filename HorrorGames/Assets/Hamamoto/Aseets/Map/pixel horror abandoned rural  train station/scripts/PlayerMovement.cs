using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class PlayerMovement : MonoBehaviour
{
    [Header("コンポーネントを自動でアタッチ")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioSource audioSource;

    [Header("シネマシーンのカメラをアタッチ"),SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [Header("SutaminaParameterManagerをアタッチ"), SerializeField]
    private SutaminaParameterManager sutaminaParameterManagerScript;

    [Header("移動速度")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    [Header("重力"), SerializeField]
    private float gravity = -40f;
    [Header("ジャンプ力"), SerializeField]
    private float JumpHight = 1;

    [Header("移動SE")]
    [SerializeField] private AudioClip moveSE;


    private CinemachineBasicMultiChannelPerlin noise;
    //現在のスピードを保管変数
    private float speed;
    Vector3 V;
    private void Awake()
    {
        //格納
        controller = gameObject.GetComponent<CharacterController>();
        audioSource = gameObject.GetComponent<AudioSource>();

        if (virtualCamera != null)
        {
            // Noiseの設定部分を取得
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }


    void Update()
    {
        if (controller.isGrounded && V.y < 0)
        {
            V.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        //移動かつ地面を踏んでいたら
        if (move.magnitude > 0.1f&& controller.isGrounded)
        {
            // まだ音が鳴っていなければ再生を開始
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            //shiftで移動速度変化
            if (Input.GetKey(KeyCode.LeftShift)&&!sutaminaParameterManagerScript.isExhausted)
            {
                //走っているフラグオン
                sutaminaParameterManagerScript.isRun = true;

                //移動音を再生
                audioSource.clip = moveSE;
                audioSource.pitch = 1.3f;

                //画面の揺れも変化
                //揺れの大きさ
                noise.m_AmplitudeGain = 1.3f;
                //揺れの速さ
                noise.m_FrequencyGain = 0.3f;

                speed = runSpeed;
            }
            else
            {
                //走っているフラグオフ
                sutaminaParameterManagerScript.isRun = false;

                //移動音を再生
                audioSource.clip = moveSE;
                audioSource.pitch = 1.0f;

                //画面の揺れも変化
                //揺れの大きさ
                noise.m_AmplitudeGain = 0.7f;
                //揺れの速さ
                noise.m_FrequencyGain = 0.3f;

                speed = walkSpeed;
            }
        }
        else
        {
            // 止まっている、または空中にいる時は音を止める
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            noise.m_AmplitudeGain = 0.3f; 
            noise.m_FrequencyGain = 0.1f;
            speed = 0f;
        }



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

