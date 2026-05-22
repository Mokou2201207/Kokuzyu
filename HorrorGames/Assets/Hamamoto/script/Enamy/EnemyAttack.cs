using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移に必要

public class EnemyAttack : MonoBehaviour
{
    [Header("ターゲット設定")]
    [SerializeField] private string playerTag = "Player";

    [Header("ジャンプスケア演出設定")]
    [Tooltip("①捕まった瞬間の衝撃音（ドーン！）")]
    [SerializeField] private AudioClip boomSound;
    [Tooltip("②耳鳴りの音（キーン…）")]
    [SerializeField] private AudioClip ringSound;
    [Tooltip("③心拍音（ドクン、ドクン）")]
    [SerializeField] private AudioClip heartbeatSound;
    [Tooltip("④敵の絶叫SE（ギャアア！）※あれば")]
    [SerializeField] private AudioClip jumpscareSound;

    [Tooltip("ジャンプスケア時にカメラをワープさせる位置・向き（空のオブジェクトを配置してください）")]
    [SerializeField] private Transform jumpscareCameraPos;
    [Tooltip("演出にかかる時間（秒）")]
    [SerializeField] private float jumpscareDuration = 2.5f;
    [Tooltip("アニメーションのトリガー名")]
    [SerializeField] private string attackAnimationTrigger = "Attack";
    [Tooltip("ゲームオーバーシーンの名前")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    private AudioSource audioSource;
    private Animator animator;
    private bool isAttacking = false; // 連続でヒットしないようにするフラグ

    void Start()
    {
        // AudioSourceがアタッチされていなければ追加する
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 親オブジェクトや自分自身からAnimatorを取得する
        animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !isAttacking)
        {
            isAttacking = true;
            StartCoroutine(JumpscareSequence(other.gameObject));
        }
    }

    // ジャンプスケアの演出シーケンス（TP方式）
    private IEnumerator JumpscareSequence(GameObject player)
    {
        Debug.Log("【演出開始】プレイヤーを消去し、カメラをワープさせます");

        // --- ここから追加：敵のAIを止めて足音と声を消す ---
        CreepAI creepAI = GetComponentInParent<CreepAI>();
        if (creepAI != null)
        {
            creepAI.OnCaughtPlayer();
        }
        // ----------------------------------------------------

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            // 1. プレイヤーを消した時にカメラまで消えないよう、カメラをプレイヤーから切り離す（親子関係を解除）
            mainCam.transform.SetParent(null);

            // 2. Cinemachineを使っている場合、CinemachineBrainが無効化されていないとカメラ操作を奪えないためOFFにする
            MonoBehaviour brain = (MonoBehaviour)mainCam.GetComponent("CinemachineBrain");
            if (brain != null)
            {
                brain.enabled = false;
                Debug.Log("CinemachineBrainを無効化しました");
            }

            // 3. カメラを演出用の位置と向きへ瞬間移動（TP）させる
            if (jumpscareCameraPos != null)
            {
                mainCam.transform.position = jumpscareCameraPos.position;
                mainCam.transform.rotation = jumpscareCameraPos.rotation;
                Debug.Log("カメラをTPしました");
            }
        }

        // 4. プレイヤー本体を非表示（消去）にする
        player.SetActive(false);

        Debug.Log("【演出】アニメーションとSEを再生します！");
        
        // --- サウンド演出 ---
        // ① 触れた瞬間に「ドーン！」という衝撃音と、敵の「絶叫」を同時に鳴らす
        if (boomSound != null) audioSource.PlayOneShot(boomSound);
        if (jumpscareSound != null) audioSource.PlayOneShot(jumpscareSound);

        if (animator != null)
        {
            // AnimatorのトリガーをONにして襲いかかるアニメーションを再生
            animator.SetTrigger(attackAnimationTrigger);
        }

        // ほんの一瞬（0.2秒）だけ待つと、「衝撃を受けた直後に耳鳴りが始まる」というリアルな恐怖演出になります
        yield return new WaitForSeconds(0.2f);

        // ② その直後に「キーン」という耳鳴りと、「ドクン、ドクン」という心拍音を重ねて再生
        if (ringSound != null) audioSource.PlayOneShot(ringSound);
        if (heartbeatSound != null) audioSource.PlayOneShot(heartbeatSound);

        // 演出が終わるまで待機（最初に0.2秒待ったので、その分を引いておく）
        yield return new WaitForSeconds(Mathf.Max(0f, jumpscareDuration - 0.2f));

        Debug.Log("【ゲームオーバー】暗転してリザルト画面へ移行");
        
        // シーン遷移処理（Build SettingsにGameOverシーンが登録されている必要があります）
        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            Debug.Log("死亡シーンへ");
            //SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
