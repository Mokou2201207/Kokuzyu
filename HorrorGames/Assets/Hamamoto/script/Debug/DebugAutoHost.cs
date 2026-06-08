using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
/// <summary>
/// テスト用のプログラム（Mainsceneからゲーム開始用）
/// </summary>
public class DebugAutoHost : MonoBehaviour
{
    [Header("テストしたいシーンをアタッチ")]
    [Scene]
    [SerializeField] private string targetGameplayScene;
    private void Start()
    {
        //マネージャが無ければ実装しない
        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (NetworkManager.singleton == null) return;

        //インスペクターで設置していれば上書き
        if (!string.IsNullOrEmpty(targetGameplayScene))
        {
            roomManager.GameplayScene= targetGameplayScene;
            Debug.Log($"【チーム開発用】ロビーからの行き先を {targetGameplayScene} に上書きしました");
        }

        //タイトルやロビーから入ったときは処理しないようにする
        string currentScene = SceneManager.GetActiveScene().path;
        if (currentScene == roomManager.offlineScene || currentScene == roomManager.RoomScene)
        {
            return;
        }

        // すでにサーバーが動いているかチェック
        if (!NetworkServer.active)
        {
            Debug.Log("【デバッグ機能】Mainシーンから直接再生されたため、自動でホストとして起動します！");
            NetworkManager.singleton.StartHost();
        }
    }
}
