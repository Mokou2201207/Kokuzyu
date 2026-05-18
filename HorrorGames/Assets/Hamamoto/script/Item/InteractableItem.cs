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
    }

    /// <summary>
    /// クロスヘアの処理から実行される処理
    /// </summary>
    public void ItemInteract()
    {
        // InventoryManagerからアイテムをとったことを流す
        InventoryManager.instance.AddItem(itemType);

        Debug.Log($"{itemName} を拾った！");
        Destroy(gameObject);
    }
}
