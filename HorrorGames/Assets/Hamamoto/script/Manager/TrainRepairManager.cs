using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Mirror;
/// <summary>
/// 条件を満たしたときの列車の処理
/// </summary>
[RequireComponent(typeof(NetworkIdentity))]
public class TrainRepairManager : NetworkBehaviour
{
    [Header("修理中のImage")]
    [SerializeField] private Image repairImage;
    [Header("チャージ用のImage")]
    [SerializeField] private Image repairGaugeImage;

    [Header("何秒間修理するか")]
    [SerializeField] private readonly float repairRequiredTime = 10f;

    [Header("修理中のSE"), SerializeField]
    private AudioClip repairAudioClip;

    [Header("エンディングムービー"), SerializeField]
    private PlayableDirector endingDirector;

    private AudioSource audioSource;

    private float repairTimer = 0f;
    // 素材が揃って、列車の前にいるか
    private bool canRepair = false;
    //全員の画面で自動で共有されるフラグ（修理中か）
    [SyncVar]
    private bool isSomeoneRepairing = false;
    //自分自身が修理の権利を取ったかどうかのフラグ
    private bool amIRepairing = false;
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
                //ほかの人がここで修理中なら自分はなにもできない
                if (isSomeoneRepairing && !amIRepairing)
                {
                    return;
                }

                //
                if (!amIRepairing)
                {
                    amIRepairing = true;
                    CmdSetRepairState(true);
                }
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
                StopMyRepair();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void StopMyRepair()
    {
        if (amIRepairing)
        {
            amIRepairing = false;
            CmdSetRepairState(false); // サーバーに「修理やめたからロック解除して！」と報告
        }
        ResetGauge(); // ゲージや音をゼロに戻す処理（元のまま）
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

        Debug.Log("修理完了！サーバに報告");

        CmdTriggerEnding();

    }

    /// <summary>
    /// characterを非表示にする処理
    /// </summary>
    private void DestroyCharacter()
    {
        //カメラはプレイヤーから外す
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.SetParent(null); // 親子関係を解除して外に出す
        }

        //Tagで対象のcharacterを非表示
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }

        GameObject[] enemis = GameObject.FindGameObjectsWithTag("Enamy");
        foreach (GameObject enemy in enemis)
        {
            enemy.SetActive(false);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSetRepairState(bool state)
    {
        isSomeoneRepairing = state;
    }

    /// <summary>
    /// クライアント側から修理を終わったことをサーバに伝える
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdTriggerEnding()
    {
        // サーバーが受け取ったら、全員にムービを出す
        RpcPlayEndingMovie();
    }

    /// <summary>
    /// サーバーが全員にムービーを流す命令
    /// </summary>
    [ClientRpc]
    private void RpcPlayEndingMovie()
    {
        //characterを非表示
        DestroyCharacter();

        //ムービーを再生
        if (endingDirector != null)
        {
            endingDirector.Play();
        }
    }


}
