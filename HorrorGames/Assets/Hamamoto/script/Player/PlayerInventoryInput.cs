using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// スロットのKeyの選択処理
/// </summary>
public class PlayerInventoryInput : NetworkBehaviour
{
    private void Update()
    {
        //自分のキャラクター以外は処理しない
        if (!isLocalPlayer) return;

        //番号Keyでスロットを選択
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            InventoryManager.instance.SelectSlot(0);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            InventoryManager.instance.SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            InventoryManager.instance.SelectSlot(2);
        }

        //Eでアイテムを使用
        if (Input.GetKeyDown(KeyCode.F))
        {
            InventoryManager.instance.UseSelectedItem();
        }
    }
}
