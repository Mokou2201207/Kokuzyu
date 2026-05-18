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
        Tire,　　　　//タイヤ
        Coal,       // 石炭
        Driver,    // ドライバー
        Key　　　　//鍵
    }

    /// <summary>
    /// クロスヘアの処理から実行される処理
    /// </summary>
    public void Interact()
    {
        // InventoryManagerからアイテムをとったことを流す
        InventoryManager.instance.AddItem(itemType);

        Debug.Log($"{itemName} を拾った！");
        Destroy(gameObject);
    }
}
