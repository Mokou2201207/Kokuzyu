using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// （チュートリアル用）UI更新処理
/// </summary>
public class TutorialUIManager : MonoBehaviour
{
    [Header("インベントリ枠のImage")]
    [SerializeField] private Image[] inventorySlotImages = new Image[3];

    [Header("インベントリーのチュートリアルの際のText（数字）")]
    [SerializeField] private Text[] inventorySlotTexts;

    [Header("現在装備中のアイテム表示"), SerializeField]
    private Image equippedItemIconUI;

    [Header("アイテムタイプごとのスプライト設定")]
    [SerializeField] private ItemSpriteEntry[] itemSprites;

    [Header("TutorialStepManagerをアタッチ"), SerializeField]
    private TutorialStepManager tutorialStepManager;

    /// <summary>
    /// 開始処理
    /// </summary>
    private void Start()
    {
        //アイテムが追加された時の処理
        InventoryManager.instance.OnItemAdded += UpdateItemUI;

        // インベントリ枠が変化した時のUI更新
        InventoryManager.instance.OnInventoryChanged += RefreshInventorySlotUI;

        //もしアイテムが選ばれた時
        InventoryManager.instance.OnSlotSelected += UpdateEquippedItemUI;

        //最初は何も持っていないので非表示にしておく
        if (equippedItemIconUI != null) equippedItemIconUI.gameObject.SetActive(false);

        // インベントリ枠のImageを最初は非表示
        foreach (var slotImage in inventorySlotImages)
        {
            if (slotImage != null) slotImage.gameObject.SetActive(false);
        }

        //インベントリ枠のTextを非表示
        foreach (var slotText in inventorySlotTexts)
        {
            if (slotText != null) slotText.gameObject.SetActive(false);
        }
    }

    //更新処理
    private void Update()
    {
        if (tutorialStepManager != null)
        {
            switch (tutorialStepManager.currentStepIndex)
            {
                case 3:
                    //インベントリ枠のTextを表示
                    foreach (var slotText in inventorySlotTexts)
                    {
                        if (slotText != null) slotText.gameObject.SetActive(true);
                    }
                    break;

                case 4:
                    //インベントリ枠のTextを非表示
                    foreach (var slotText in inventorySlotTexts)
                    {
                        if (slotText != null) slotText.gameObject.SetActive(false);
                    }
                    break;
            }

        }
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
            case InteractableItem.ItemType.MusicBox:
                break;
        }
    }

    /// <summary>
    /// インベントリ枠のUIをリフレッシュ
    /// </summary>
    private void RefreshInventorySlotUI()
    {
        var slots = InventoryManager.instance.inventorySlots;

        for (int i = 0; i < inventorySlotImages.Length; i++)
        {
            if (inventorySlotImages[i] == null) continue;

            if (i < slots.Count)
            {
                // 枠にアイテムがある場合→表示してスプライトを設定
                inventorySlotImages[i].gameObject.SetActive(true);
                Sprite sprite = GetSpriteForItemType(slots[i]);
                if (sprite != null)
                {
                    inventorySlotImages[i].sprite = sprite;
                }
            }
            else
            {
                // 枠にアイテムがない場合→非表示
                inventorySlotImages[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ItemTypeに対応するスプライトを取得
    /// </summary>
    private Sprite GetSpriteForItemType(InteractableItem.ItemType type)
    {
        foreach (var entry in itemSprites)
        {
            if (entry.itemType == type) return entry.sprite;
        }
        return null;
    }

    /// <summary>
    /// 構えているアイテムを専用のUI枠に大きく表示する
    /// </summary>
    /// <param name="selectedIndex"></param>
    private void UpdateEquippedItemUI(int selectedIndex)
    {
        // 選択解除された時、またはインベントリの枠外の時は非表示にする
        if (selectedIndex == -1 || selectedIndex >= InventoryManager.instance.inventorySlots.Count)
        {
            if (equippedItemIconUI != null) equippedItemIconUI.gameObject.SetActive(false);
            return;
        }

        // 選択したスロットに入っているアイテムの種類を取得
        InteractableItem.ItemType currentItem = InventoryManager.instance.inventorySlots[selectedIndex];

        // アイテムに対応する画像を取得
        Sprite sprite = GetSpriteForItemType(currentItem);

        if (sprite != null && equippedItemIconUI != null)
        {
            // 画像をセットして、表示をオンにする！
            equippedItemIconUI.sprite = sprite;
            equippedItemIconUI.gameObject.SetActive(true);
        }
    }
}


