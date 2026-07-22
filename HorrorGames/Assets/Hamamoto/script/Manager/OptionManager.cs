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
    [Header("コンポーネントを自動アタッチ")]
    [SerializeField]private AudioSource optionAudioSource;

    [Header("オプションのパネル"), SerializeField]
    private Image optionPanal;

    [Header("オプションを開くSE"),SerializeField]
    private AudioClip optionAudioClip;

    //オプションを開いているか
    public static bool isOpenOption = false;

    private void Start()
    {
        //最初はオプションのパネルを非表示
        optionPanal.gameObject.SetActive(false);
    }

    private void Update()
    {
        // オンライン対応：複数プレイヤーがいる場合でも、自分（ローカルプレイヤー）のAudioSourceを取得する
        if (optionAudioSource == null)
        {
            if (NetworkClient.localPlayer != null)
            {
                optionAudioSource = NetworkClient.localPlayer.GetComponent<AudioSource>();
            }
        }

        //オプションを開く処理
        if (Input.GetKeyDown(KeyCode.Escape) && !isOpenOption)
        {
            isOpenOption = true;
            
            // 再生前に一度Stopすることで、連打した際の音の重なりを防ぐ
            if (optionAudioSource != null)
            {
                optionAudioSource.Stop(); 
                optionAudioSource.PlayOneShot(optionAudioClip);
            }
            optionPanal.gameObject.SetActive(true);

            // オプション操作のためにマウスカーソルを表示・ロック解除
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isOpenOption)
        {
            isOpenOption = false;
            optionPanal.gameObject.SetActive(false);
            
            // オプションを閉じたら音を消す
            if (optionAudioSource != null)
            {
                optionAudioSource.Stop();
            }

            // オプションを閉じたらマウスカーソルを隠して視点操作に戻す
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // (Unityエディタ上でESCキーを押すと強制的にロック解除される仕様を回避するため、数フレーム後にも再度ロックする)
            StartCoroutine(LockCursorDelay());
        }
    }

    private IEnumerator LockCursorDelay()
    {
        // 1フレーム後と0.1秒後に念押しでカーソルをロック・非表示にする（エディタのESCキー対策）
        yield return null;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(0.1f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}