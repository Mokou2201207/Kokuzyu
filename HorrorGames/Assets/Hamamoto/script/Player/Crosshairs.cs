using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
/// <summary>
/// クロスヘア処理
/// </summary>
public class Crosshairs : NetworkBehaviour
{
    [Header("UIManagerをアタッチ"),SerializeField]
    private UIManager uiManagerScript;

    [Header("クロスヘアのImage"), SerializeField]
    private Image crosshairImage;

    [Header("通常のクロスヘア"), SerializeField]
    private Sprite normalSprite;
    [Header("アイテムの場合のクロスヘア"), SerializeField]
    private Sprite targetSprite;


    [Header("Rayの半径"), SerializeField]
    private float sphereRadius = 0.5f;
    [Header("Rayの距離"),SerializeField]
    private float maxDistance = 50f;

    private void Start()
    {
        if (uiManagerScript == null)
        {
            uiManagerScript = FindObjectOfType<UIManager>();
        }

        if (crosshairImage == null)
        {
            GameObject crosshairObj = GameObject.Find("Crosshairs");
            if (crosshairObj == null) crosshairObj = GameObject.Find("Crosshair");
            
            if (crosshairObj != null)
            {
                crosshairImage = crosshairObj.GetComponent<Image>();
            }
            else
            {
                Image[] images = FindObjectsOfType<Image>(true);
                foreach (Image img in images)
                {
                    if (img.gameObject.name.ToLower().Contains("crosshair"))
                    {
                        crosshairImage = img;
                        break;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Camera.main == null || crosshairImage == null) return;

        Ray ray = Camera.main.ScreenPointToRay(crosshairImage.transform.position);
        RaycastHit hit;

        // SphereCastを実行
        if (Physics.SphereCast(ray, sphereRadius, out hit, maxDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
            //アイテムに当たってるとき
            if (hit.collider.TryGetComponent<InteractableItem>(out var item))
            {
                //スプライト変換
               crosshairImage.sprite=targetSprite;

                // EKeyでアイテムを入手
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // インベントリに空きがあるかチェック（素材以外）
                    if (InventoryManager.instance != null && !InventoryManager.instance.CanAddItem(item.itemType))
                    {
                        Debug.Log("インベントリがいっぱいです！拾えません。");
                        return;
                    }
                    //サーバーからこのアイテムを消し、全員のUIも更新
                    CmdPickupItem(item.gameObject, item.itemType);
                }

            }
            else if(hit.collider.TryGetComponent<MissionObj>(out var mission))
            {
                //スプライト変換
                crosshairImage.sprite = targetSprite;

                // EKeyでアイテムを入手
                if (Input.GetKeyDown(KeyCode.E)&&!uiManagerScript.isMissionOpen)
                {
                    mission.MissionInteract();
                }
            }
            else
            {
                //スプライト変換
                crosshairImage.sprite = normalSprite;
            }
        }
        //ものがまずなにも当たってない時
        else
        {
            crosshairImage.sprite = normalSprite;
        }
    }

    /// <summary>
    /// サーバーからアイテムを削除
    /// </summary>
    /// <param name="itemObject"></param>
    [Command]
    private void CmdPickupItem(GameObject itemObject,InteractableItem.ItemType type)
    {
        if (itemObject == null)
        {
            Debug.LogError("CmdPickupItem: itemObjectがnullです。アイテムにNetworkIdentityが付いているか確認してください。");
            return;
        }

        // NetworkIdentityがあるか確認
        if (itemObject.GetComponent<NetworkIdentity>() != null)
        {
            //全員の画面から削除
            NetworkServer.Destroy(itemObject);
        }
        else
        {
            Debug.LogWarning($"CmdPickupItem: {itemObject.name} にNetworkIdentityがありません。RpcでDestroyします。");
            RpcDestroyItem(itemObject);
        }

        // バッテリー、十字架、オルゴールは拾った人だけに反映、それ以外は全員に反映
        if (type == InteractableItem.ItemType.Battery || type == InteractableItem.ItemType.TheCross || type == InteractableItem.ItemType.MusicBox||type==InteractableItem.ItemType.Stimulant)
        {
            // 拾った本人だけにUI更新を送る
            TargetPickerOnlyUI(connectionToClient, type);
        }
        else
        {
            // 全員の画面からUIの更新をかける
            RpcUpdateAllUI(type);
        }
    }

    /// <summary>
    /// NetworkIdentityがないオブジェクトを全クライアントで削除する
    /// </summary>
    [ClientRpc]
    private void RpcDestroyItem(GameObject itemObject)
    {
        if (itemObject != null)
        {
            Destroy(itemObject);
        }
    }

    /// <summary>
    /// 拾った本人だけにUI更新を送る
    /// </summary>
    [TargetRpc]
    private void TargetPickerOnlyUI(NetworkConnection target, InteractableItem.ItemType type)
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AddItem(type);
        }
    }

    /// <summary>
    /// サーバーから命令を受け取り全プレイヤーから実行
    /// </summary>
    /// <param name="type"></param>
    [ClientRpc] 
    private void RpcUpdateAllUI(InteractableItem.ItemType type)
    {
        // 全員の画面にあるインベントリマネージャーにアイテムを追加する
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AddItem(type);
        }
    }
}

