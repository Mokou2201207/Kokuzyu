using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    [Header("コンポーネントを自動でアタッチ")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;

    [Header("シネマシーンのカメラをアタッチ"), SerializeField]
    private CinemachineVirtualCamera virtualCamera;

    [Header("scriptををアタッチ"), SerializeField]
    private SutaminaParameterManager sutaminaParameterManagerScript;
    [SerializeField]　private CurseManager curseManager;

    [Header("移動速度")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [Header("呪いの際の移動速度")]
    [SerializeField] private float curseWalkSpeed = 2.5f;
    [SerializeField] private float curseRunSpeed = 5f;

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
        animator = gameObject.GetComponentInChildren<Animator>();
        curseManager = gameObject.GetComponent<CurseManager>();

        if (virtualCamera != null)
        {
            // Noiseの設定部分を取得
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Start()
    {
        // 自分自身じゃない場合はカメラを無効化する
        if (!isLocalPlayer)
        {
            if (virtualCamera != null) virtualCamera.gameObject.SetActive(false);

            // AudioListenerが付いているなら無効化（警告防止）
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;

            // MainCamera等のカメラタグがついている普通のカメラがあれば無効化
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.gameObject.SetActive(false);
        }
    }



    void Update()
    {
        if (!isLocalPlayer) return; // 自分が操作するキャラ以外は無視

        // 毎フレーム1度だけ接地判定を取得
        bool isGrounded = controller.isGrounded;

        if (isGrounded && V.y < 0)
        {
            V.y = -2f;
            
            // 地面に着地したらジャンプアニメーションを解除
            if (animator != null)
            {
                animator.SetBool("Jump", false);
            }
        }

        // ジャンプの入力判定 (Moveの前に持ってくることで、Moveによる接地判定のズレを防ぐ)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            V.y = Mathf.Sqrt(JumpHight * -2f * gravity);

            //ジャンプアニメーション
            if (animator != null)
            {
                animator.SetBool("Jump", true);
            }
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        Vector3 move = transform.right * x + transform.forward * z;

        // スピードの設定（空中でも移動できるようにする）
        if (move.magnitude > 0.1f)
        {
            if (Input.GetKey(KeyCode.LeftShift) && !sutaminaParameterManagerScript.isExhausted)
            {
                if (animator != null)
                {
                    animator.SetBool("Run", true);   // 走るをON
                    animator.SetBool("Walk", true);  // 他は絶対にOFF
                }

                sutaminaParameterManagerScript.isRun = true;
                //呪いによって速度変化
                float runspeed = curseManager.isCurseFull ? curseRunSpeed : runSpeed;
                speed = runspeed;
            }
            else
            {
                if (animator != null)
                {
                    animator.SetBool("Run", false);   // 走るをOFF
                    animator.SetBool("Walk", true);  // 他は絶対にOFF
                }

                sutaminaParameterManagerScript.isRun = false;
                //呪いによって速度変化
                float walkspeed = curseManager.isCurseFull ? curseWalkSpeed : walkSpeed;
                speed = walkspeed;
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("Run", false);   // 走るをOFF
                animator.SetBool("Walk", false);  // 歩くをOFF
            }

            sutaminaParameterManagerScript.isRun = false;
            speed = 0f;
        }

        // 移動かつ地面を踏んでいたら（足音とカメラの揺れの処理）
        if (move.magnitude > 0.1f && isGrounded)
        {
            //走ってなければ歩くAnimation
            if (animator != null && !sutaminaParameterManagerScript.isRun)
            {
                animator.SetBool("Walk", true);
            }

            // まだ音が鳴っていなければ再生を開始
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            audioSource.clip = moveSE;

            if (sutaminaParameterManagerScript.isRun)
            {
                audioSource.pitch = 1.3f;
                //揺れの大きさ
                noise.m_AmplitudeGain = 1.3f;
                //揺れの速さ
                noise.m_FrequencyGain = 0.3f;
            }
            else
            {
                audioSource.pitch = 1.0f;
                //揺れの大きさ
                noise.m_AmplitudeGain = 0.7f;
                //揺れの速さ
                noise.m_FrequencyGain = 0.3f;
            }
        }
        else
        {
            //歩くのを停止Animation
            if (animator != null)
            {
                animator.SetBool("Walk", false);
            }

            // 止まっている、または空中にいる時は音と揺れを止める
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            noise.m_AmplitudeGain = 0.3f;
            noise.m_FrequencyGain = 0.1f;
        }

        // 重力適用
        V.y += gravity * Time.deltaTime;

        // 横移動と重力・ジャンプをまとめて1回のMoveで実行する
        Vector3 finalVelocity = move * speed + Vector3.up * V.y;
        controller.Move(finalVelocity * Time.deltaTime);


    }

}

