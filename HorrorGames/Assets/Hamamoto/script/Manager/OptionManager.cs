using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// option設定処理
/// </summary>
public class OptionManager : MonoBehaviour
{
    [Header("コンポーネントを自動アタッチ"), SerializeField]
    private AudioSource optionAudioSource;

    [Header("オプションのパネル"), SerializeField]
    private Image optionPanal;

    [Header("オプションを開くSE"), SerializeField]
    private AudioClip optionAudioClip;

    [Header("タイトルに戻るボタン"), SerializeField]
    private Button returnToTitleButton;

    [Header("オプションを閉じるボタン"), SerializeField]
    private Button closeOptionButton;

    // オプションを開いているか
    public static bool isOpenOption = false;

    private void Start()
    {
        // 最初はオプションのパネルを非表示
        if (optionPanal != null)
        {
            optionPanal.gameObject.SetActive(false);
        }

        // ボタンのOnClickイベントをアタッチ
        if (returnToTitleButton != null)
        {
            returnToTitleButton.onClick.AddListener(OnReturnToTitleButtonClicked);
        }
        if (closeOptionButton != null)
        {
            closeOptionButton.onClick.AddListener(OnCloseOptionButtonClicked);
        }
    }

    private void Update()
    {
        // オンライン対応 ローカルプレイヤーのAudioSourceを取得する
        if (optionAudioSource == null)
        {
            if (NetworkClient.localPlayer != null)
            {
                optionAudioSource = NetworkClient.localPlayer.GetComponent<AudioSource>();
            }
        }

        // オプションを開く閉じる処理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpenOption)
            {
                OpenOption();
            }
            else
            {
                CloseOption();
            }
        }
    }

    /// <summary>
    /// オプション画面を開く
    /// </summary>
    public void OpenOption()
    {
        isOpenOption = true;

        // 再生前に一度Stopすることで音の重なりを防ぐ
        if (optionAudioSource != null)
        {
            optionAudioSource.Stop();
            optionAudioSource.PlayOneShot(optionAudioClip);
        }

        if (optionPanal != null)
        {
            optionPanal.gameObject.SetActive(true);
        }

        // オプション操作のためにマウスカーソルを表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// オプション画面を閉じる
    /// </summary>
    public void CloseOption()
    {
        isOpenOption = false;

        if (optionPanal != null)
        {
            optionPanal.gameObject.SetActive(false);
        }

        // オプションを閉じたら音を消す
        if (optionAudioSource != null)
        {
            optionAudioSource.Stop();
        }

        // オプションを閉じたらマウスカーソルを隠して視点操作に戻す
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 数フレーム後にも再度ロックする
        StartCoroutine(LockCursorDelay());
    }

    /// <summary>
    /// オプション閉じるボタン押下時のイベント
    /// </summary>
    public void OnCloseOptionButtonClicked()
    {
        CloseOption();
    }

    /// <summary>
    /// タイトルに戻るボタン押下時のイベント
    /// </summary>
    public void OnReturnToTitleButtonClicked()
    {
        isOpenOption = false;

        // タイトル画面に戻るのでカーソルを表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ホストの場合は StopHost で全員タイトルへ
        // クライアントの場合は StopClient で自分だけタイトルへ
        if (NetworkServer.active && NetworkClient.active)
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.StopHost();
            }
        }
        else if (NetworkClient.isConnected || NetworkClient.active)
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.StopClient();
            }
        }
        else
        {
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.StopHost();
            }
        }

        // 古いRelayManagerを削除
        if (RelayManager.Instance != null)
        {
            Destroy(RelayManager.Instance.gameObject);
        }
    }

    private IEnumerator LockCursorDelay()
    {
        // 1フレーム後と0.1秒後に念押しでカーソルをロックする
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(0.1f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}