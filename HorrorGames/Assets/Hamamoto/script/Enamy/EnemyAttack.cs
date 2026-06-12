using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
/// <summary>
/// 敵に捕まったときの演出
/// </summary>
public class EnemyAttack : NetworkBehaviour
{
    [Header("ターゲット設定")]
    [SerializeField] private string playerTag = "Player";

    [Header("画面をフェイドアウトするパネル")]
    [SerializeField] private Image gameOverPanal;

    [Header("捕まった瞬間の衝撃音")]
    [SerializeField] private AudioClip boomSound;
    [Header("耳鳴りの音")]
    [SerializeField] private AudioClip ringSound;
    [Header("心拍音")]
    [SerializeField] private AudioClip heartbeatSound;
    [Header("敵の絶叫SE")]
    [SerializeField] private AudioClip jumpscareSound;

    [Tooltip("ジャンプスケア時にカメラをワープさせる位置")]
    [SerializeField] private Transform jumpscareCameraPos;
    [Tooltip("アニメーションのトリガー名")]
    [SerializeField] private string attackAnimationTrigger = "Attack";
    [Tooltip("ゲームオーバーシーンの名前")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    private AudioSource audioSource;
    private Animator animator;
    private bool isAttacking = false; // 連続でヒットしないようにするフラグ

    void Start()
    {
        //非表示
        gameOverPanal.gameObject.SetActive(false);
        //格納
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// プレイヤーに触れたら（サーバーでのみ判定）
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return; // サーバーだけが当たり判定を管理する

        if (other.CompareTag(playerTag) && !isAttacking)
        {
            isAttacking = true;
            
            // 捕まえた相手のNetworkIdentityを取得
            NetworkIdentity netId = other.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                // 回線落ちを防ぐため、数値を送信する
                RpcTriggerJumpscare(netId.netId);
            }

            // サーバー側でシーン遷移のカウントダウンを開始
            StartCoroutine(ServerGameOverSequence());
        }
    }

    /// <summary>
    /// クライアント側で演出を実行する
    /// </summary>
    [ClientRpc]
    private void RpcTriggerJumpscare(uint caughtNetId)
    {
        // ネットワーク上の全オブジェクトから、捕まったプレイヤーを探す
        if (NetworkClient.spawned.TryGetValue(caughtNetId, out NetworkIdentity caughtIdentity))
        {
            if (caughtIdentity.isLocalPlayer)
            {
                StartCoroutine(LocalJumpscareSequence(caughtIdentity.gameObject));
            }
            else
            {
                // 他の人が捕まった場合は悲鳴だけ鳴らす
                if (jumpscareSound != null) audioSource.PlayOneShot(jumpscareSound);
            }
        }
    }

    /// <summary>
    /// 演出（捕まった本人のみ実行される）
    /// </summary>
    private IEnumerator LocalJumpscareSequence(GameObject player)
    {
        CreepAI creepAI = GetComponentInParent<CreepAI>();
        if (creepAI != null)
        {
            creepAI.OnCaughtPlayer();
        }

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            // プレイヤーを消した時にカメラまで消えないよう、カメラをプレイヤーから切り離す
            mainCam.transform.SetParent(null);

            //  Cinemachineを使っている場合、CinemachineBrainが無効化されていないとカメラ操作を奪えないためOFFにする
            MonoBehaviour brain = (MonoBehaviour)mainCam.GetComponent("CinemachineBrain");
            if (brain != null)
            {
                brain.enabled = false;
                Debug.Log("CinemachineBrainを無効化しました");
            }

            // カメラを演出用の位置と向きへ瞬間移動させる
            if (jumpscareCameraPos != null)
            {
                mainCam.transform.position = jumpscareCameraPos.position;
                mainCam.transform.rotation = jumpscareCameraPos.rotation;
                Debug.Log("カメラをTPしました");
            }
            else
            {
                // もしjumpscareCameraPosが設定されていない場合でも、強制的に敵の顔の前にカメラを移動させる
                mainCam.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 1.5f;
                mainCam.transform.LookAt(transform.position + Vector3.up * 1.5f);
                Debug.Log("カメラを自動計算位置へTPしました");
            }
        }

        //表示
        if (gameOverPanal != null)
        {
            gameOverPanal.gameObject.SetActive(true);
        }

        // プレイヤーの見た目だけを非表示にする（回線落ちを防ぐためSetActive(false)は使わない）
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        Debug.Log("【演出】アニメーションとSEを再生します！");
        
        // サウンド演出
        if (boomSound != null) audioSource.PlayOneShot(boomSound);
        if (jumpscareSound != null) audioSource.PlayOneShot(jumpscareSound);

        if (animator != null)
        {
            // AnimatorのトリガーをONにして襲いかかるアニメーションを再生
            animator.SetTrigger(attackAnimationTrigger);
        }

        if (ringSound != null) audioSource.PlayOneShot(ringSound);
        if (heartbeatSound != null) audioSource.PlayOneShot(heartbeatSound);
        yield return new WaitForSeconds(4f);
    }

    /// <summary>
    /// サーバー側で４秒待ってからシーンをリスタートする
    /// </summary>
    private IEnumerator ServerGameOverSequence()
    {
        CreepAI creepAI = GetComponentInParent<CreepAI>();
        if (creepAI != null)
        {
            creepAI.OnCaughtPlayer();
        }

        // 演出の終了を待つ
        yield return new WaitForSeconds(4f);

        // UI（真っ暗な画面）を持ったまま次のシーンに行かないように、サーバー側で非表示に戻す指示を出す
        RpcHideGameOverPanel();

        if (NetworkManager.singleton != null)
        {
            // Lobby（Room）の仕様に合わせて、MainではなくRoomSceneに戻す
            NetworkRoomManager roomManager = NetworkManager.singleton as NetworkRoomManager;
            if (roomManager != null)
            {
                roomManager.ServerChangeScene(roomManager.RoomScene);
            }
        }
    }

    [ClientRpc]
    private void RpcHideGameOverPanel()
    {
        if (gameOverPanal != null)
        {
            gameOverPanal.gameObject.SetActive(false);
        }

        // ロビー画面に戻った際にUI（ボタンなど）をクリックできるように、マウスカーソルを表示・ロック解除する
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}
