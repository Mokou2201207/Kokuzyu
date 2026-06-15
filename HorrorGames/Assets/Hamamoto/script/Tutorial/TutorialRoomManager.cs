using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// チュートリアルの時だけロビーを経由せずにプレイヤーを出現させるカスタムRoomManager
/// </summary>
public class TutorialRoomManager : NetworkRoomManager
{
    [Header("Tutorial Settings")]
    [Scene]
    [Tooltip("チュートリアルシーンを指定してください")]
    [SerializeField] private string tutorialScenePath;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 現在のシーンがチュートリアルシーンの場合、ロビー(RoomScene)のチェックをスキップして直接プレイヤーを出現させる
        if (SceneManager.GetActiveScene().path == tutorialScenePath)
        {
            Transform startPos = GetStartPosition();
            GameObject gamePlayer = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            // プレイヤーをネットワーク上に生成し、クライアントに権限を渡す
            NetworkServer.AddPlayerForConnection(conn, gamePlayer);
        }
        else
        {
            // それ以外のシーン（通常のオンラインプレイ時）は元のNetworkRoomManagerの動作を行う
            base.OnServerAddPlayer(conn);
        }
    }
}
