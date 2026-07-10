using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelayUIManager : MonoBehaviour
{
    [Header("UIパーツの紐付け")]
    public TMP_InputField joinCodeInputField;
    public TextMeshProUGUI codeDisplayText;

    private void Start()
    {
        // タイトルに戻ってきた時にカーソルが消えたままにならないよう、表示＆ロック解除する
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// 「部屋を作る」ボタンが押された時に呼ばれる
    /// </summary>
    public void OnHostButton()
    {
        //RelayManagerのホスト作成処理を呼ぶ
        RelayManager.Instance.CreateRelayHost();
    }

    /// <summary>
    /// 「部屋に入る」ボタンが押された時に呼ばれる
    /// </summary>
    public void OnJoinButton()
    {
        // 入力された文字から見えない文字(空白)を消す
        string rawText = joinCodeInputField.text;
        string cleanCode = rawText.Replace("\u200B", "").Trim().ToUpper();

        // 空っぽじゃなければ、そのコードを使って参加処理を呼ぶ
        if (!string.IsNullOrEmpty(cleanCode))
        {
            RelayManager.Instance.JoinRelayClient(cleanCode);
        }
        else
        {
            Debug.LogWarning("コードが入力されていません！");
        }
    }
}
