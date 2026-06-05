using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// タイトルでロビーに入る処理
/// </summary>
public class TitleMenuManager : MonoBehaviour
{
    [Header("IPアドレス入力欄")]
    [SerializeField] private TMP_InputField ipInputField;

    /// <summary>
    /// 部屋を作るボタン
    /// </summary>
    public void ClickCreateRoom()
    {
        Debug.Log("ホストとして部屋を作ります");
        //サーバーを作る処理
        NetworkManager.singleton.StartHost();
    }

    /// <summary>
    /// 部屋に入るボタン
    /// </summary>
    public void ClickJoinRoom()
    {
        string ipAddress = "localhost";

        if (ipInputField != null && !string.IsNullOrEmpty(ipInputField.text))
        {
            ipAddress = ipInputField.text;
        }

        Debug.Log($"{ipAddress} の部屋に参加します！");

        // 接続先を設定して、クライアントとして参加する
        NetworkManager.singleton.networkAddress = ipAddress;
        NetworkManager.singleton.StartClient();
    }
}
