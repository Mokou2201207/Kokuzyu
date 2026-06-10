using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("アイテムの名前")]
    public string itemName;

    [Header("アイテムのタイプ")]
    public ItemType itemType;

    /// <summary>
    /// アイテムの名前
    /// </summary>
    public enum ItemType
    {
        //タイヤ
        Tire,
        // 石炭
        Coal,
        // ドライバー
        Driver,
        //鍵
        Key,
        //バッテリー
        Battery,
    }

    /// <summary>
    /// アイテムが拾われた時に呼ばれる
    /// </summary>
    public void OnPickedUp()
    {
        // 自分のインベントリマネージャーにアイテムを追加
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.AddItem(itemType);
        }
        Debug.Log($"{itemName} をローカルで取得しました！");
    }
}
