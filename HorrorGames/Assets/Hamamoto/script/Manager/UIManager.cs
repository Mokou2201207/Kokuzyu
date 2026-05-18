using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI更新などの処理
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("タイヤアイコン")]
    [SerializeField] private Image tireIcon;
    [Header("石炭アイコン")]
    [SerializeField] private Image coalIcon;
    [Header("鍵アイコン")]
    [SerializeField] private Image keyIcon;
    [Header("ドライバーアイコン")]
    [SerializeField] private Image driverIcon;

    private void Start()
    {
        //アイテムが追加された時の処理
        InventoryManager.instance.OnItemAdded += UpdateItemUI;

        //最初は画像は黒く
        tireIcon.color = Color.black;
        keyIcon.color = Color.black;
        coalIcon.color = Color.black;
        driverIcon.color = Color.black;
    }

    /// <summary>
    /// 取得したアイテムによってUIを変化
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    private void UpdateItemUI(InteractableItem.ItemType type, int count)
    {
        switch (type)
        {
            //タイヤ
            case InteractableItem.ItemType.Tire:
                //色を元の色に戻す
                tireIcon.color = Color.white;
                break;

            //石炭
            case InteractableItem.ItemType.Coal:
                //色を元の色に戻す
                coalIcon.color = Color.white;
                break;

            //鍵
            case InteractableItem.ItemType.Key:
                //色を元の色に戻す
                keyIcon.color = Color.white;
                break;

            //ドライバー
            case InteractableItem.ItemType.Driver:
                //色を元の色に戻す
                driverIcon.color = Color.white;
                break;

        }

    }
}
