using UnityEngine;
using Mirror;
/// <summary>
/// プレイヤーの体をカメラでは非表示にする処理
/// </summary>
public class PlayerVisibility : NetworkBehaviour
{
    [Header("見えなくする自分の3Dモデル")]
    [SerializeField] private GameObject[] visualModels;

    /// <summary>
    /// 自分が触るPlayerだけレイヤー変更
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        // レイヤーを取得
        int localPlayerLayer = LayerMask.NameToLayer("LocalPlayer");

        // モデルをLocalPlayerに変更
        foreach (var model in visualModels)
        {
            if (model != null)
            {
                model.layer = localPlayerLayer;
            }
        }

        Debug.Log("自分の体をLocalPlayerレイヤーに変更しました！");
    }
}