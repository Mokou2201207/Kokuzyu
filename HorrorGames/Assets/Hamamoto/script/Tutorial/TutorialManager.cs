using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
/// <summary>
/// タイトル画面からソロ用の擬似オンラインチュートリアルを起動するクラス
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("チュートリアル用のステージシーン")]
    [Scene]
    [SerializeField] private string tutorialScene;

    [Header("ローディングのパネル")]
    [SerializeField] private Image loadingImage;

    // 元のマルチプレイ用の設定を一時的に保存しておく変数
    private static string originalRoomScene;
    private static string originalGameplayScene;
    private static string originalOnlineScene;
    private static bool isInitialized = false;

    private void Start()
    {
        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null) return;

        // 初回起動時にインスペクターに設定されている正規のロビー・本編シーンのパスを記憶する
        if (!isInitialized)
        {
            originalRoomScene = roomManager.RoomScene;
            originalGameplayScene = roomManager.GameplayScene;
            originalOnlineScene = roomManager.onlineScene;
            isInitialized = true;
        }
        else
        {
            // チュートリアルを終えてタイトルに戻ってきた際、設定を通常のマルチプレイ用に戻す
            roomManager.RoomScene = originalRoomScene;
            roomManager.GameplayScene = originalGameplayScene;
            roomManager.onlineScene = originalOnlineScene;
            Debug.Log("ロビーの設定を通常のマルチプレイ用に復元しました。");
        }

        //ローディングのパネルを非表示
        loadingImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// UIの「チュートリアルを始める」ボタンから呼び出すメソッド
    /// </summary>
    public void StartTutorial()
    {
        //画面をローディング表示
        loadingImage.gameObject.SetActive(true);

        var roomManager = NetworkManager.singleton as NetworkRoomManager;
        if (roomManager == null) return;

        // 行き先のシーン設定を、一時的にチュートリアル用ステージに書き換える
        roomManager.GameplayScene = tutorialScene;
        roomManager.onlineScene = tutorialScene;

        // 自分1人だけのサーバーとして起動
        roomManager.StartHost();

        Debug.Log($"【システム】ソロチュートリアルを擬似オンライン（ステージ: {tutorialScene}）で起動しました！");
    }
}
