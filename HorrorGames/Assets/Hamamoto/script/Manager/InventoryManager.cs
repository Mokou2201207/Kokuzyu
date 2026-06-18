using System;
using System.Collections.Generic;
using UnityEngine;
using static InteractableItem;
using static MissionObj;

/// <summary>
/// イベントやアイテムの情報の保持
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // どこでもアクセスできるようにする
    public static InventoryManager instance;

    // アイテムの種類とカウントで保存
    private Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();
    private Dictionary<ObjType, int> objCounts = new Dictionary<ObjType, int>();

    // アイテムを３つだけ保存できる枠
    public List<ItemType> inventorySlots = new List<ItemType>();
    private const int MaxInventorySize = 3;

    //今何番を選択しているかの変数
    public int currentSelectedSlot = -1;

    //アイテム追加実行イベント
    public event Action<ItemType, int> OnItemAdded;
    public event Action<ObjType, int> OnMissionObj;

    // インベントリ枠が変化した時のイベント
    public event Action OnInventoryChanged;

    //選択スロットが変わったときのイベント
    public event Action<int> OnSlotSelected;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 特定の素材かどうかを判定
    /// </summary>
    public bool IsMaterial(ItemType type)
    {
        return type == ItemType.Tire || type == ItemType.Coal || type == ItemType.Driver || type == ItemType.Key;
    }

    /// <summary>
    /// インベントリに空きがあるかどうか
    /// </summary>
    public bool CanAddItem(ItemType type)
    {
        if (IsMaterial(type)) return true; // 素材は無制限
        return inventorySlots.Count < MaxInventorySize;
    }

    /// <summary>
    /// アイテム追加時のアイテムの実行
    /// 同じアイテムでも重ねない（1つ=1枠消費）
    /// </summary>
    /// <param name="type"></param>
    public void AddItem(ItemType type)
    {
        // 素材以外のアイテムなら、インベントリの枠に追加できるか確認
        if (!IsMaterial(type))
        {
            if (inventorySlots.Count < MaxInventorySize)
            {
                inventorySlots.Add(type);
            }
            else
            {
                Debug.LogWarning("インベントリの枠が一杯です！");
                return; // 一杯の場合は追加しない
            }
        }

        // 素材はDictionaryでカウント管理
        if (IsMaterial(type))
        {
            if (itemCounts.ContainsKey(type))
            {
                itemCounts[type]++;
            }
            else
            {
                itemCounts.Add(type, 1);
            }
        }

        // イベント発火
        int count = IsMaterial(type) ? itemCounts[type] : GetItemCount(type);
        OnItemAdded?.Invoke(type, count);

        // インベントリ枠UIの更新イベント発火（素材以外の場合）
        if (!IsMaterial(type)) OnInventoryChanged?.Invoke();

        // 確認用デバッグログ
        Debug.Log($"{type} の現在の個数: {count}（インベントリ残り枠: {MaxInventorySize - inventorySlots.Count}）");
    }

    /// <summary>
    /// アイテムを使用・消費してインベントリ枠から減らす処理
    /// </summary>
    public void UseItem(ItemType type)
    {
        if (!IsMaterial(type) && inventorySlots.Contains(type))
        {
            inventorySlots.Remove(type);
            OnInventoryChanged?.Invoke();
        }
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
    /// そのアイテムは何個持っているか
    /// 素材はDictionary、それ以外はインベントリ枠内の個数を返す
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetItemCount(ItemType type)
    {
        if (IsMaterial(type))
        {
            return itemCounts.ContainsKey(type) ? itemCounts[type] : 0;
        }
        // 素材以外はスロット内に何個あるかカウント（重ねないので1つずつ）
        int count = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot == type) count++;
        }
        return count;
    }

    /// <summary>
    /// スロットの選択の処理
    /// </summary>
    /// <param name="slotIndex"></param>
    public void SelectSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < MaxInventorySize)
        {
            //同じKeyを選択したならしまう
            if (currentSelectedSlot == slotIndex)
            {
                currentSelectedSlot = -1;
            }
            //じゃなければ構える
            else
            {
                currentSelectedSlot = slotIndex;
            }

            // 変更をUIに通知
            OnSlotSelected?.Invoke(currentSelectedSlot);
        }
    }

    /// <summary>
    /// 選択アイテムを使用する処理
    /// </summary>
    public void UseSelectedItem()
    {
        //初期値ならなにもしない
        if (currentSelectedSlot == -1)
        {
            Debug.Log("アイテムが選択されていません！数字キーで構えてください。");
            return;
        }

        // 選択中のスロットにアイテムがあるか確認
        if (currentSelectedSlot < inventorySlots.Count)
        {
            ItemType itemToUse = inventorySlots[currentSelectedSlot];

            // アイテムの効果を発動
            ExecuteItemEffect(itemToUse);

            // アイテムを消費して枠から消す
            inventorySlots.RemoveAt(currentSelectedSlot);

            //使った後は初期値に戻す
            currentSelectedSlot = -1;
            OnSlotSelected?.Invoke(currentSelectedSlot);

            //UIを更新
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.Log("選択中のスロットは空です。");
        }
    }

    /// <summary>
    /// アイテムの効果を実際に発動する処理
    /// </summary>
    /// <param name="type"></param>
    private void ExecuteItemEffect(ItemType type)
    {
        switch (type)
        {
            //バッテリー
            case ItemType.Battery:
                if (UIManager.instance.batteryParametarManager != null)
                {
                    UIManager.instance.batteryParametarManager.SupplementBattery();
                }
                break;
                //十字架
            case ItemType.TheCross:
                if (UIManager.instance.curseManager != null)
                {
                    UIManager.instance.curseManager.UseTheCross();
                }
                break;
            case ItemType.MusicBox:
                //例
                Debug.Log("オルゴールを設置しました！");
                break;
        }
    }
}