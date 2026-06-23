using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Networking.Transport.Relay;
using Utp;
/// <summary>
/// Unity Relayを使って部屋の作成と参加を行うマネージャー
/// </summary>
public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

    [Header("最大接続人数")]
    public int maxConnections = 4;

    // UIで画面に表示するためにコードを保存しておく変数
    [HideInInspector]
    public static string currentJoinCode = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // もしロビーから戻ってきた時などに2個目ができたら消す
        }
    }
    private async void Start()
    {
        // ゲーム開始時にUnity Servicesを初期化し、匿名でログインする
        await InitializeAndSignIn();
    }

    /// <summary>
    /// Unity Servicesの初期化と匿名ログイン
    /// </summary>
    /// <returns></returns>
    private async Task InitializeAndSignIn()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("$\"【Relay】匿名ログイン成功。 プレイヤーID: {AuthenticationService.Instance.PlayerId}\"");
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError("$\"【Relayエラー】ログイン失敗: {e.Message}\"");
        }
    }

    /// <summary>
    /// ホストとして部屋を作る
    /// </summary>
    /// <returns></returns>
    public void CreateRelayHost()
    {
        UtpTransport transport = NetworkManager.singleton.GetComponent<UtpTransport>();
        transport.useRelay = true;//Relayを使うモードにする

        Debug.Log("【Relay】部屋を作成中...");

        // UtpTransportに内蔵されている自動作成機能を使う
        transport.AllocateRelayServer(maxConnections, null,
            (string joinCode) =>
            {
                // 成功した時の処理
                Debug.Log($"【Relay】部屋作成成功. 参加コード: {joinCode}");
                currentJoinCode = joinCode;
                NetworkManager.singleton.StartHost(); // ホストとして起動
            },
            () =>
            {
                // 失敗した時の処理
                Debug.LogError("【Relayエラー】部屋の作成に失敗しました。");
            }
        );
    }

    /// <summary>
    /// ゲストとして参加コード（6桁）を入力して部屋に入る
    /// </summary>
    public void JoinRelayClient(string joinCode)
    {
        UtpTransport transport = NetworkManager.singleton.GetComponent<UtpTransport>();
        transport.useRelay = true; // Relayを使うモードにする

        Debug.Log($"【Relay】コード {joinCode} で部屋に参加中...");

        // UtpTransportに内蔵されている自動参加機能を使う！
        transport.ConfigureClientWithJoinCode(joinCode,
            () =>
            {
                // 成功した時の処理
                Debug.Log("【Relay】接続準備完了.ゲームに参加します。");
                NetworkManager.singleton.StartClient(); // クライアントとして起動
            },
            () =>
            {
                // 失敗した時の処理
                Debug.LogError("【Relayエラー】部屋の参加に失敗しました。コードが違うか、満員です。");
            }
        );
    }
}

