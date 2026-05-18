using System;
using System.Collections.Generic;
using UnityEngine;
using static InteractableItem;
using static MissionObj;
/// <summary>
/// 自分が持ってるインベントリの処理
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // どこからでもアクセスできるようにする
    public static InventoryManager instance;

    // アイテムの種類と個数をセットで保存する
    private Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();
    private Dictionary<ObjType, int> objCounts = new Dictionary<ObjType, int>();

    //アイテムを追加したら実行されるイベント
    public event Action<ItemType, int> OnItemAdded;
    public event Action<ObjType, int> OnMissionObj;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// アイテムを追加と特定のアイテムの実行
    /// </summary>
    /// <param name="type"></param>
    public void AddItem(ItemType type)
    {
        // すでに持っているなら＋1、持っていないなら1として登録
        if (itemCounts.ContainsKey(type))
        {
            itemCounts[type]++;
        }
        else
        {
            itemCounts.Add(type, 1);
        }
        OnItemAdded?.Invoke(type, itemCounts[type]);

        // 確認用デバッグログ
        Debug.Log($"{type} の現在の個数: {itemCounts[type]}");
    }

    /// <summary>
    /// ミッションのオブジェクトの実行
    /// </summary>
    /// <param name="type"></param>
    public void IntractMissionObj(ObjType type)
    {
        int currentCount = objCounts.ContainsKey(type) ? objCounts[type] : 0;
        OnMissionObj?.Invoke(type, currentCount);
    }

    /// <summary>
    /// 特定のアイテムは何個持ってるか
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetItemCount(ItemType type)
    {
        return itemCounts.ContainsKey(type) ? itemCounts[type] : 0;
    }
}