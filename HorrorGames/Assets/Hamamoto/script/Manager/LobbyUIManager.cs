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
    [Header("4人分のスロット本体（PlayerSlot）")]
    [SerializeField] private GameObject[] playerSlots;

    [Header("それぞれの名前テキスト")]
    [SerializeField] private Text[] nameTexts;

    [Header("それぞれのボタンのテキスト（状態）")]
    [SerializeField] private Text[] buttonTexts;

    [Header("ボタン設定")]
    [SerializeField] private Button returnToTitleButton;

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
        //通信の管理者を取得
        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null) return;

        //現在部屋に入ってるプレイヤーを取得
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
            // もしこの枠の番号に、実際のプレイヤーが存在したら
            if (i < roomPlayers.Count)
            {
                playerSlots[i].SetActive(true); // 枠を表示

                var player = roomPlayers[i]; // そのプレイヤーの情報を取り出す

                // 名前を変更
                nameTexts[i].text = (i == 0) ? "ホスト (Player 1)" : $"ゲスト (Player {i + 1})";

                // その人が「準備完了」を押しているかどうかで文字を変える
                if (player.readyToBegin)
                {
                    buttonTexts[i].text = "READY!";
                    buttonTexts[i].color = Color.red; // ホラーっぽく赤色に
                }
                else
                {
                    buttonTexts[i].text = "準備中...";
                    buttonTexts[i].color = Color.white;
                    allReady = false; // 準備中がいる場合は全員完了ではない
                }
            }
            // プレイヤーが存在しない空き枠だったら
            else
            {
                playerSlots[i].SetActive(false); // 枠を非表示にする
            }
        }
    }

    /// <summary>
    /// UIの「準備完了」ボタンを押した際に呼ばれる処理
    /// </summary>
    public void ClickReadyButton()
    {
        // ネットワーク上にある「自分自身のプレイヤー」を探す
        var localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;

        // 自分自身のRoomPlayerコンポーネントを取得
        var roomPlayer = localPlayer.GetComponent<NetworkRoomPlayer>();
        if (roomPlayer == null) return;

        // 今の自分の状態の「逆」にする（準備中なら完了へ、完了なら準備中へキャンセル）
        bool changeState = !roomPlayer.readyToBegin;

        // サーバーに向けて通信を送る
        roomPlayer.CmdChangeReadyState(changeState);

        Debug.Log($"自分の準備状態を {changeState} に変更しました！");
    }

    /// <summary>
    /// UIの「タイトルに戻る」ボタンを押した際に呼ばれる処理
    /// ホストが押したら全員タイトルへ、ゲストが押したら自分だけタイトルへ戻る
    /// </summary>
    public void OnReturnToTitleButtonClicked()
    {
        // タイトル画面に戻るのでカーソルを表示・ロック解除
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // ホスト（サーバー兼クライアント）の場合は StopHost() で全員タイトルへ
        // クライアント（ゲスト）の場合は StopClient() で自分だけタイトルへ
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
