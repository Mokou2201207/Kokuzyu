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

    // 「部屋を作る」ボタンが押された時に呼ばれる
    public void OnHostButton()
    {
        // RelayManagerのホスト作成処理を呼ぶ
        RelayManager.Instance.CreateRelayHost();
    }

    // 「部屋に入る」ボタンが押された時に呼ばれる
    public void OnJoinButton()
    {
        // 入力欄の文字（コード）を読み取る
        string code = joinCodeInputField.text;

        // 空っぽじゃなければ、そのコードを使って参加処理を呼ぶ
        if (!string.IsNullOrEmpty(code))
        {
            RelayManager.Instance.JoinRelayClient(code);
        }
        else
        {
            Debug.LogWarning("コードが入力されていません！");
        }
    }
}
