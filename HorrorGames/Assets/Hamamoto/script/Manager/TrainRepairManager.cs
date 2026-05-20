using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private readonly float repairRequiredTime = 5f;

    [Header("修理中のSE"),SerializeField]
    private AudioClip repairAudioClip;

    private AudioSource audioSource;

    private float repairTimer = 0f;
    // 素材が揃って、列車の前にいるか
    private bool canRepair = false; 
    // Start is called before the first frame update
    void Start()
    {
        //格納
        audioSource = GetComponent<AudioSource>();

        //ループに設定
        audioSource.clip = repairAudioClip;
        audioSource.loop = true;

        //ゲージを最初は０に
        repairGaugeImage.fillAmount = 0f; 
        //非表示
        repairGaugeImage.gameObject.SetActive(false);
        repairImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (canRepair)
        {
            // Eキーを「押している間」ずっと実行
            if (Input.GetKey(KeyCode.E))
            {
                // ゲージを表示して時間を進める
                repairGaugeImage.gameObject.SetActive(true);
                repairImage.gameObject.SetActive(true);
                repairTimer += Time.deltaTime;

                //音が鳴っていなければ再生を開始する
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                // ゲージのUIを満たす
                repairGaugeImage.fillAmount = repairTimer / repairRequiredTime;

                // 5秒達成したかチェック
                if (repairTimer >= repairRequiredTime)
                {
                    RepairComplete();
                }
            }
            // Eキーを「離した」らリセット
            else if (Input.GetKeyUp(KeyCode.E))
            {
                ResetGauge();
            }
        }
    }
    /// <summary>
    /// UIManagerから「修理可能状態」をオンオフしてもらう
    /// </summary>
    public void SetCanRepair(bool state)
    {
        canRepair = state;
        // 視線が外れたらリセット
        if (!state) ResetGauge(); 
    }

    /// <summary>
    /// リセットした際の処理
    /// </summary>
    private void ResetGauge()
    {
        //リセット
        repairTimer = 0f;
        repairGaugeImage.fillAmount = 0f;
        //非表示
        repairGaugeImage.gameObject.SetActive(false);
        repairImage.gameObject.SetActive(false);

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// 修理できた際の処理
    /// </summary>
    private void RepairComplete()
    {
        canRepair = false;
        // メーターをMAXで止める
        repairGaugeImage.fillAmount = 1f;
        //非表示
        repairGaugeImage.gameObject.SetActive(false);
        repairImage.gameObject.SetActive(false);

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Debug.Log("修理完了！脱出成功！");
    }
}
