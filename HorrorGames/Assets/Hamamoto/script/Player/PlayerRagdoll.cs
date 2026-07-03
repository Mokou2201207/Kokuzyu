using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 死亡した際の体のボーンをすべて力を抜く処理
/// </summary>
public class PlayerRagdoll : NetworkBehaviour
{
    [Header("コンポーネントの紐づけ")]
    [SerializeField]private Animator animator;

    //骨についているすべてのRigidbodyを格納
    [SerializeField]private Rigidbody[] ragdollRigidbodies;

    [Header("【デバッグ用】インスペクターからチェックを入れると力が抜けます")]
    [SerializeField] private bool testRagdoll = false;

    private void Awake()
    {
        //骨組みのRigidbodyを自動で取得
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();

        //ラグドール無効化
        SetRagdollEnabled(false);
    }

    private void Update()
    {
        // インスペクターでチェックを入れたら強制的にラグドールを発動
        if (testRagdoll)
        {
            testRagdoll = false;
            SetRagdollEnabled(true);
            Debug.Log("デバッグ用ラグドールを発動しました！");
        }
    }

    /// <summary>
    /// ラグドールをオン、オフに切り替える処理
    /// </summary>
    /// <param name="isEnabled"></param>
    private void SetRagdollEnabled(bool isEnabled)
    {
        //ラグドール中はアニメーション無効化
        if (animator != null)
        {
            animator.enabled = !isEnabled;
        }

        //全ての骨のRigidbodyの物理演算を切り替える
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !isEnabled;
        }

        // --- 追加：ラグドール時に他のコンポーネントが邪魔しないように無効化 ---
        // キャラクターコントローラーを無効化（立ったままになるのを防ぐ）
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = !isEnabled;
        }

        // プレイヤーの移動処理なども止める
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = !isEnabled;
        }
    }

    /// <summary>
    /// 敵に捕まって死亡した時に呼ばれる処理
    /// </summary>
    [ClientRpc]
    public void RpcDie()
    {
        Debug.Log("【PlayerRagdoll】RpcDieが呼ばれました！全身の力を抜きます！");
        // ラグドールをオンにして、全身の力を抜く！
        SetRagdollEnabled(true);
    }
}
