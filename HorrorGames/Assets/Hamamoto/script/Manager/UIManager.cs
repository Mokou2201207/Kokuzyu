using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI更新などの処理
/// </summary>
public class UIManager : MonoBehaviour
{
    //インスタンス化
    public static UIManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    [Header("タイヤアイコン")]
    [SerializeField] private Image tireIcon;
    [Header("石炭アイコン")]
    [SerializeField] private Image coalIcon;
    [Header("鍵アイコン")]
    [SerializeField] private Image keyIcon;
    [Header("ドライバーアイコン")]
    [SerializeField] private Image driverIcon;

    [Header("ミッションテキストとImageとアニメーターをアタッチ")]
    [SerializeField] private Image missionImage;
    [SerializeField] private Text missionText;
    [SerializeField] private Animator animator;

    [Header("TrainRepairManagerをアタッチ")]
    [SerializeField] private TrainRepairManager trainRepairManager;
    [Header("BatteryParametarManagerをアタッチ")]
    public BatteryParametarManager batteryParametarManager;
    [Header("CurseManagerをアタッチ")]
    public CurseManager curseManager;

    [Header("Curse UI Reference")]
    public Slider curseSliderUI;
    public Animator curseAnimatorUI;

    [Header("Battery UI Reference")]
    public Slider batterySliderUI;
    public Animator batteryAnimatorUI;

    [Header("Sutamina UI Reference")]
    public SutaminaParameterManager sutaminaParameterManager;
    public Slider sutaminaSliderUI;
    public Image dizzinessBackgroundUI;
    public Animator metarAnimatorUI;
    public Animator shortnesAnimatorUI;

    //ミッション用のテキストを表示されているかどうか
    public bool isMissionOpen = false;

    [Header("インベントリ枠のImage（3つ）")]
    [SerializeField] private Image[] inventorySlotImages = new Image[3];

    [Header("現在装備中のアイテム表示"),SerializeField]
    private Image equippedItemIconUI;

    [Header("アイテムタイプごとのスプライト設定")]
    [SerializeField] private ItemSpriteEntry[] itemSprites;

    private void Start()
    {
        //アイテムが追加された時の処理
        InventoryManager.instance.OnItemAdded += UpdateItemUI;
        InventoryManager.instance.OnMissionObj += UpdateMissionUI;

        // インベントリ枠が変化した時のUI更新
        InventoryManager.instance.OnInventoryChanged += RefreshInventorySlotUI;

        //もしアイテムが選ばれた時
        InventoryManager.instance.OnSlotSelected += UpdateEquippedItemUI;

        //最初は何も持っていないので非表示にしておく
        if (equippedItemIconUI != null) equippedItemIconUI.gameObject.SetActive(false);

        //最初は画像は黒く
        tireIcon.color = Color.black;
        keyIcon.color = Color.black;
        coalIcon.color = Color.black;
        driverIcon.color = Color.black;

        //非表示
        missionImage.gameObject.SetActive(false);

        // インベントリ枠のImageを最初は非表示
        foreach (var slotImage in inventorySlotImages)
        {
            if (slotImage != null) slotImage.gameObject.SetActive(false);
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

            //効果を発動
            case InteractableItem.ItemType.Battery:
            case InteractableItem.ItemType.TheCross:
            case InteractableItem.ItemType.MusicBox:

                break;
        }

    }


    /// <summary>
    /// 選択したミッションによってUIを変化
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    private void UpdateMissionUI(MissionObj.ObjType type, int count)
    {
        switch (type)
        {
            //列車
            case MissionObj.ObjType.train:
                //特定のアイテムを入手してるかどうか
                if (InventoryManager.instance.GetItemCount(InteractableItem.ItemType.Tire) >= 1 &&
                    InventoryManager.instance.GetItemCount(InteractableItem.ItemType.Coal) >= 1 &&
                    InventoryManager.instance.GetItemCount(InteractableItem.ItemType.Key) >= 1 &&
                    InventoryManager.instance.GetItemCount(InteractableItem.ItemType.Driver) >= 1)
                {
                    if (trainRepairManager != null)
                    {
                        trainRepairManager.SetCanRepair(true);
                    }
                }
                else
                {
                    missionText.text = "素材が足りません";
                    StartCoroutine(MissionImageOpenCoroutine());
                }

                break;
        }
    }

    /// <summary>
    /// ミッションImage用のコルーチン
    /// </summary>
    /// <returns></returns>
    private IEnumerator MissionImageOpenCoroutine()
    {
        //表示
        missionImage.gameObject.SetActive(true);
        isMissionOpen = true;

        yield return new WaitForSeconds(3f);

        //非表示
        missionImage.gameObject.SetActive(false);
        isMissionOpen = false;
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

/// <summary>
/// アイテムタイプとスプライトの対応付け用の構造体
/// </summary>
[System.Serializable]
public struct ItemSpriteEntry
{
    public InteractableItem.ItemType itemType;
    public Sprite sprite;
}
