using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>
/// 条件を満たしたときの列車の処理
/// </summary>
public class TrainRepairManager : MonoBehaviour
{
    [Header("修理中のImage")]
    [SerializeField] private Image repairImage;

    [Header("チャージ用のImage")]
    [SerializeField] private Image repairGaugeImage;

    [Header("何秒間修理するか")]
    [SerializeField] private float repairRequiredTime = 10f;

    [Header("修理中のSE")]
    [SerializeField] private AudioClip repairAudioClip;

    [Header("エンディングTimeline")]
    [SerializeField] private PlayableDirector endingTimeline;

    [Header("ロビーシーン名")]
    [SerializeField] private string lobbySceneName = "Lobby";

    // AudioSourceを格納
    private AudioSource audioSource;

    // 修理時間を計測するタイマー
    private float repairTimer = 0f;

    // 素材が揃って、列車の前にいるか
    private bool canRepair = false;

    // Start is called before the first frame update
    void Start()
    {
        // AudioSourceを取得
        audioSource = GetComponent<AudioSource>();

        // 修理SEを設定
        audioSource.clip = repairAudioClip;

        // SEをループ再生に設定
        audioSource.loop = true;

        // ゲージを最初は0にする
        repairGaugeImage.fillAmount = 0f;

        // 修理UIを非表示
        repairGaugeImage.gameObject.SetActive(false);
        repairImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 修理可能な場合のみ実行
        if (canRepair)
        {
            // Eキーを押している間
            if (Input.GetKey(KeyCode.E))
            {
                // 修理UIを表示
                repairGaugeImage.gameObject.SetActive(true);
                repairImage.gameObject.SetActive(true);

                // 修理時間を加算
                repairTimer += Time.deltaTime;

                // SEが鳴っていなければ再生
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }

                // 修理ゲージを更新
                repairGaugeImage.fillAmount = repairTimer / repairRequiredTime;

                // 規定時間修理したら修理完了
                if (repairTimer >= repairRequiredTime)
                {
                    RepairComplete();
                }
            }
            // Eキーを離したらリセット
            else if (Input.GetKeyUp(KeyCode.E))
            {
                ResetGauge();
            }
        }
    }

    /// <summary>
    /// UIManagerから修理可能状態を設定
    /// </summary>
    public void SetCanRepair(bool state)
    {
        // 修理可能状態を変更
        canRepair = state;

        // 修理可能でなくなったらゲージをリセット
        if (!state)
        {
            ResetGauge();
        }
    }

    /// <summary>
    /// 修理ゲージをリセットする処理
    /// </summary>
    private void ResetGauge()
    {
        // タイマーをリセット
        repairTimer = 0f;

        // ゲージを0に戻す
        repairGaugeImage.fillAmount = 0f;

        // 修理UIを非表示
        repairGaugeImage.gameObject.SetActive(false);
        repairImage.gameObject.SetActive(false);

        // 修理SEを停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// 修理完了時の処理
    /// </summary>
    private void RepairComplete()
    {
        // 修理を終了
        canRepair = false;

        // ゲージをMAXにする
        repairGaugeImage.fillAmount = 1f;

        // 修理UIを非表示
        repairGaugeImage.gameObject.SetActive(false);
        repairImage.gameObject.SetActive(false);

        // 修理SEを停止
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // コンソールに表示
        Debug.Log("修理完了！脱出成功！");

        // エンディングTimelineが設定されている場合
        if (endingTimeline != null)
        {
            // エンディングムービーを再生
            endingTimeline.Play();

            // Timeline終了後にロビーへ戻る
            StartCoroutine(ReturnToLobbyAfterTimeline());
        }
    }

    /// <summary>
    /// Timeline終了後にロビーへ戻る
    /// </summary>
    private IEnumerator ReturnToLobbyAfterTimeline()
    {
        // Timelineの再生時間だけ待機
        yield return new WaitForSeconds((float)endingTimeline.duration);

        // ロビーシーンへ移動
        SceneManager.LoadScene(lobbySceneName);
    }
}