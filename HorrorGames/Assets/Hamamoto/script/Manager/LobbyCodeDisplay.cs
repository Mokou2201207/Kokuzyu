using UnityEngine;
using TMPro;

public class LobbyCodeDisplay : MonoBehaviour
{
    [Header("ロビー画面のコード表示用テキスト")]
    [SerializeField] private TextMeshProUGUI lobbyCodeText;

    private void Update()
    {
        //直接アクセス
        if (!string.IsNullOrEmpty(RelayManager.currentJoinCode))
        {
            lobbyCodeText.text = "Code: " + RelayManager.currentJoinCode;
        }
        else
        {
            lobbyCodeText.text = "Fetching Code...";
        }
    }
}