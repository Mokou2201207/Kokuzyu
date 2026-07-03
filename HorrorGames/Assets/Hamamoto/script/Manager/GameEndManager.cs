using UnityEngine;
using Mirror;

public class GameEndManager : NetworkBehaviour
{
    /// <summary>
    /// 【全員共通】誰からでも呼び出せるタイトル強制送還処理
    /// </summary>
    public void TriggerReturnToTitle()
    {
        if (isServer)
        {
            // 自分がホスト（サーバー）なら、直接全員を巻き込んで終了する
            RpcDisconnectAll();
        }
        else
        {
            // 自分がゲスト（クライアント）なら、サーバーに終了と頼む
            CmdRequestReturnToTitle();
        }
    }

    /// <summary>
    /// クライアントからサーバーへ終了を要請する
    /// </summary>
    [Command(requiresAuthority = false)]
    private void CmdRequestReturnToTitle()
    {
        RpcDisconnectAll();
    }

    /// <summary>
    /// サーバーから全員の接続を安全に切断する
    /// </summary>
    [Server] // サーバー上でのみ実行されることを保証
    private void RpcDisconnectAll()
    {
        Debug.Log("全員をタイトル画面へ強制送還します。");

        if (NetworkManager.singleton != null)
        {
            // ホスト（サーバー）を停止。
            // これを呼ぶだけで、ゲスト側は自動的に切断され、
            // NetworkManagerに登録されている「Offline Scene」へ全員が一斉に戻ります。
            NetworkManager.singleton.StopHost();
        }
    }
}