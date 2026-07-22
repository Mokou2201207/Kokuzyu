using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// ロビーの処理
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    [Header("4人分のスロット本体"), SerializeField]
    private GameObject[] playerSlots;

    [Header("それぞれの名前テキスト"), SerializeField]
    private Text[] nameTexts;

    [Header("それぞれのボタンのテキスト"), SerializeField]
    private Text[] buttonTexts;

    [Header("タイトルに戻るボタン"), SerializeField]
    private Button returnToTitleButton;

    // ロード画面が開始されたかを判定するフラグ
    private bool isLoadingStarted = false;

    private void Start()
    {
        // タイトルに戻るボタンのOnClickイベント登録
        if (returnToTitleButton != null)
        {
            returnToTitleButton.onClick.AddListener(OnReturnToTitleButtonClicked);
        }
    }

    private void Update()
    {
        // 通信の管理者を取得
        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null) return;

        // 現在部屋に入ってるプレイヤーを取得
        var roomPlayers = new List<NetworkRoomPlayer>(roomManager.roomSlots);
        
        // 全員が準備完了したかを判定するためのフラグ
        bool allReady = true;

        // 誰もいない場合はまだ準備完了ではない
        if (roomPlayers.Count == 0)
        {
            allReady = false;
        }

        // 4つの枠を順番にチェックしていく
        for (int i = 0; i < playerSlots.Length; i++)
        {
            // もしこの枠の番号に実際のプレイヤーが存在したら
            if (i < roomPlayers.Count)
            {
                // 枠を表示
                playerSlots[i].SetActive(true);

                // そのプレイヤーの情報を取り出す
                var player = roomPlayers[i];

                // 名前を変更
                nameTexts[i].text = (i == 0) ? "ホスト Player 1" : $"ゲスト Player {i + 1}";

                // その人が準備完了を押しているかどうかで文字を変える
                if (player.readyToBegin)
                {
                    buttonTexts[i].text = "READY";
                    // ホラーっぽく赤色に設定
                    buttonTexts[i].color = Color.red;
                }
                else
                {
                    buttonTexts[i].text = "準備中...";
                    buttonTexts[i].color = Color.white;
                    // 準備中がいる場合は全員完了ではない
                    allReady = false;
                }
            }
            // プレイヤーが存在しない空き枠だったら
            else
            {
                // 枠を非表示にする
                playerSlots[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// UIの準備完了ボタンを押した際に呼ばれる処理
    /// </summary>
    public void ClickReadyButton()
    {
        // ネットワーク上にある自分自身のプレイヤーを探す
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;

        // 自分自身のRoomPlayerコンポーネントを取得
        var roomPlayer = localPlayer.GetComponent<NetworkRoomPlayer>();
        if (roomPlayer == null) return;

        // 今の自分の状態の逆にする
        bool changeState = !roomPlayer.readyToBegin;

        // サーバーに向けて通信を送る
        roomPlayer.CmdChangeReadyState(changeState);

        Debug.Log($"自分の準備状態を変更しました {changeState}");
    }

    /// <summary>
    /// UIのタイトルに戻るボタンを押した際に呼ばれる処理
    /// ホストが押したら全員タイトルへ ゲストが押したら自分だけタイトルへ戻る
    /// </summary>
    public void OnReturnToTitleButtonClicked()
    {
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
}
