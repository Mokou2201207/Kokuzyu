using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;


public class EnemyAttack : MonoBehaviour
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
    /// プレイヤーに触れたら
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isAttacking)
        {
            isAttacking = true;
            StartCoroutine(JumpscareSequence(other.gameObject));
        }
    }

    /// <summary>
    /// ジャンプスケアの演出
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator JumpscareSequence(GameObject player)
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

            // カメラを演出用の位置と向きへ瞬間移動（TP）させる
            if (jumpscareCameraPos != null)
            {
                mainCam.transform.position = jumpscareCameraPos.position;
                mainCam.transform.rotation = jumpscareCameraPos.rotation;
                Debug.Log("カメラをTPしました");
            }
        }

        //表示
        gameOverPanal.gameObject.SetActive(true);

        // プレイヤー本体を非表示にする
        player.SetActive(false);

        Debug.Log("【演出】アニメーションとSEを再生します！");
        
        // --- サウンド演出 ---
        if (boomSound != null) audioSource.PlayOneShot(boomSound);
        if (jumpscareSound != null) audioSource.PlayOneShot(jumpscareSound);

        if (animator != null)
        {
            // AnimatorのトリガーをONにして襲いかかるアニメーションを再生
            animator.SetTrigger(attackAnimationTrigger);
        }


        if (ringSound != null) audioSource.PlayOneShot(ringSound);
        if (heartbeatSound != null) audioSource.PlayOneShot(heartbeatSound);

        // 演出が終わるまで待機
        yield return new WaitForSeconds(9f);

        if (NetworkManager.singleton != null && NetworkManager.singleton.isNetworkActive)
        {
            if (NetworkServer.active)
            {
                NetworkManager.singleton.ServerChangeScene("Main");
            }
        }
        else
        {
            SceneManager.LoadScene("Main");
        }

    }

}
