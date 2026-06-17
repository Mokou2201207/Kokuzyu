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

    private void Start()
    {
        //アイテムが追加された時の処理
        InventoryManager.instance.OnItemAdded += UpdateItemUI;
        InventoryManager.instance.OnMissionObj += UpdateMissionUI;

        //最初は画像は黒く
        tireIcon.color = Color.black;
        keyIcon.color = Color.black;
        coalIcon.color = Color.black;
        driverIcon.color = Color.black;

        //非表示
        missionImage.gameObject.SetActive(false);
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

            //バッテリー
            case InteractableItem.ItemType.Battery:
                //バッテリーを補充する処理へ
                if (batteryParametarManager != null) batteryParametarManager.SupplementBattery();
                break;

                //十字架
            case InteractableItem.ItemType.TheCross:
                if (curseManager != null) curseManager.UseTheCross();
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
}
