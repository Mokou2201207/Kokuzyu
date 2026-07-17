using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
/// <summary>
/// （チュートリアル用）UI更新処理
/// </summary>
public class TutorialUIManager : MonoBehaviour
{
    /// <summary>
    /// チュートリアルの一ページ分のデータ
    /// </summary>
    [System.Serializable]
    public struct TutorialPageData
    {
        [Header("このページで表示する画像")]
        public Sprite pageImage;

        [Header("ページのタイトル")]
        public string topText;

        [Header("このページの説明文")]
        [TextArea(2, 5)]
        public string pageText;
    }
    [Header("チュートリアルのページ一覧")]
    [SerializeField] private TutorialPageData[] tutorialPages;

    [Header("チュートリアルパネル内のUIパーツ")]
    [SerializeField] private Image tutorialDisplayImage; // 画像を表示するImage
    [SerializeField] private Text tutorialTopText;       // タイトルを表示するText
    [SerializeField] private Text tutorialDisplayText;   // 説明文を表示するText
    [SerializeField] private Text tutorialPageNumberText; // ページ数を表示するText (例: "1 / 5")

    // 今開いているチュートリアルのページ番号
    private int currentTutorialPageIndex = 0;

    [Header("チュートリアルパネル"), SerializeField]
    private GameObject tutorialPanel;

    [Header("ローディングパネル"), SerializeField]
    private GameObject lodingPanel;

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

    // チュートリアルパネルが開いているかどうか
    private bool isTutorialOpen = false;

    // チュートリアルパネルが一度でも表示されたかどうか
    private bool hasTutorialPanelAppeared = false;

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

        //チュートリアルのパネルを非表示
        tutorialPanel.SetActive(false);
        //ローディングのパネルを非表示
        lodingPanel.SetActive(false);
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

                    OpenTutorial();
                    break;
            }
        }

        // チュートリアルパネルが開いている時のページ送り操作
        if (isTutorialOpen)
        {
            // 右クリック 次のページへ
            if (Input.GetMouseButtonDown(1))
            {
                NextTutorialPage();
            }
            // 左クリック 前のページへ
            else if (Input.GetMouseButtonDown(0))
            {
                PreviousTutorialPage();
            }
        }

        // ESCキー Titleシーンに戻る
        if (Input.GetKeyDown(KeyCode.Escape) && hasTutorialPanelAppeared)
        {
            StartCoroutine(LodingCoroutine());
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
            // 画像をセットして、表示をオンにする
            equippedItemIconUI.sprite = sprite;
            equippedItemIconUI.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// チュートリアル画面を開いて、ゲームを止める
    /// </summary>
    public void OpenTutorial()
    {
        // 既に開いている場合は何もしない
        if (isTutorialOpen) return;

        // ページを最初に戻す
        currentTutorialPageIndex = 0;

        // 最初のページの内容を表示
        UpdateTutorialPageDisplay();

        tutorialPanel.SetActive(true);  // パネルを表示
        isTutorialOpen = true;
        hasTutorialPanelAppeared = true; // パネルが表示されたことを記録

        Time.timeScale = 0f;            // ゲームの時間をストップ
    }

    /// <summary>
    /// チュートリアル画面を閉じて、ゲームを再開する
    /// </summary>
    public void CloseTutorial()
    {
        tutorialPanel.SetActive(false); // パネルを非表示
        isTutorialOpen = false;
        Time.timeScale = 1f;            // ゲームの時間を元に戻す
    }

    /// <summary>
    /// Titleシーンに戻る処理
    /// （tutorialPanelが表示され、最後のページまで読んだ後にESCキーで呼ばれる）
    /// </summary>
    private void ReturnToTitle()
    {
        // ゲームの時間を元に戻す
        Time.timeScale = 1f;

        Debug.Log("チュートリアル終了 → Titleシーンへ戻ります");

        // Mirrorのホストを停止して、Offline Sceneへ戻る
        if (NetworkManager.singleton != null)
        {
            NetworkManager.singleton.StopHost();
        }
    }

    /// <summary>
    /// 次のページへ進む（右クリック）
    /// </summary>
    private void NextTutorialPage()
    {
        // 最後のページなら、それ以上進まない
        if (currentTutorialPageIndex >= tutorialPages.Length - 1)
        {
            return;
        }

        // 次のページへ
        currentTutorialPageIndex++;
        UpdateTutorialPageDisplay();
    }

    /// <summary>
    /// 前のページへ戻る（左クリック）
    /// </summary>
    private void PreviousTutorialPage()
    {
        // 最初のページなら何もしない
        if (currentTutorialPageIndex <= 0) return;

        // 前のページへ
        currentTutorialPageIndex--;
        UpdateTutorialPageDisplay();
    }

    /// <summary>
    /// 現在のページ番号に合わせてUI表示を更新する
    /// </summary>
    private void UpdateTutorialPageDisplay()
    {
        // ページデータが無い場合は何もしない
        if (tutorialPages == null || tutorialPages.Length == 0) return;

        // 現在のページデータを取得
        TutorialPageData page = tutorialPages[currentTutorialPageIndex];

        // 画像を更新
        if (tutorialDisplayImage != null)
        {
            tutorialDisplayImage.sprite = page.pageImage;
            // 画像がnullの場合は非表示にする
            tutorialDisplayImage.gameObject.SetActive(page.pageImage != null);
        }

        // タイトルを更新
        if (tutorialTopText != null)
        {
            tutorialTopText.text = page.topText;
        }

        // 説明文を更新
        if (tutorialDisplayText != null)
        {
            tutorialDisplayText.text = page.pageText;
        }

        // ページ番号を更新
        if (tutorialPageNumberText != null)
        {
            tutorialPageNumberText.text = $"{currentTutorialPageIndex + 1} / {tutorialPages.Length}";
        }
    }

    /// <summary>
    /// ローディングを入れる処理※ローディング入れてからシーン変え
    /// </summary>
    /// <returns></returns>
    private IEnumerator LodingCoroutine()
    {
        lodingPanel.SetActive(true);

        // 1秒待つ (タイムスケールが0でも動作するようにRealtimeを使用)
        yield return new WaitForSecondsRealtime(1f);

        ReturnToTitle();
    }
}
