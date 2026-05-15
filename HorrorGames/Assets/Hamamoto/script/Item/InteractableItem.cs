using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("アイテムの名前")]
    public string itemName;

    [Header("アイテムのタイプ")]
    public ItemType itemType;

    public enum ItemType
    {
        Tire,

    }

    public void Interact()
    {
        // InventoryManagerからアイテムをとったことを流す
        InventoryManager.instance.AddItem(itemType);

        Debug.Log($"{itemName} を拾った！");
        Destroy(gameObject);
    }
}
