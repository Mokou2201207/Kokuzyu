using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
/// <summary>
/// チュートリアルのタスクなどの処理
/// </summary>
public class TutorialStepManager : MonoBehaviour
{
    //チュートリアルの各ステップを構造体へまとめる
    [System.Serializable]
    public struct TutorialStep
    {
        [Header("ステップの説明名前")]
        [TextArea(2, 5)]
        public string stepName;

        [Header("このステップで流す動画")]
        public VideoClip tutorialVideo;

        [Header("クリアしたら消す壁")]
        public GameObject wallObject;
    }

    [Header("チュートリアルのステップ一覧"), SerializeField]
    private TutorialStep[] tutorialSteps;

    [Header("動画をアタッチ"), SerializeField]
    private VideoPlayer videoPlayer;
    [Header("説明文をアタッチ"), SerializeField]
    private Text tutorialStepText;
    [Header("タスクスライダーをアタッチ"), SerializeField]
    private Slider taskSlider;

    [Header("移動のタスク時間"), SerializeField]
    private float MoveTaskTime = 5f;
    [Header("ジャンプのタスク回数"), SerializeField]
    private float JumpTaskTime = 5f;

    //現在のステップ番号
    private int currentStepIndex = 0;
    //行動中のチュートリアル用のタイマー
    private float movementTimer = 0f;
    //移動のカウント
    private int jumpCount = 0;
    

    void Start()
    {
        //最初のチュートリアルを開始
        SetUpStep(currentStepIndex);
    }

    void Update()
    {
        HandleCurrentStepTask();
    }

    /// <summary>
    /// 指定されたステップの動画や環境をセットアップする
    /// </summary>
    /// <param name="index"></param>
    private void SetUpStep(int index)
    {
        //チュートリアル終了の条件
        if (index>=tutorialSteps.Length)
        {
            Debug.Log("チュートリアル完了です！");
            return;
        }

        //Textを更新していく
        if (tutorialStepText!=null)
        {
            tutorialStepText.text = tutorialSteps[index].stepName;
        }

        //動画を切り替えて再生
        if (videoPlayer != null && tutorialSteps[index].tutorialVideo!=null)
        {
            videoPlayer.clip = tutorialSteps[index].tutorialVideo;
            videoPlayer.Play();
        }
    }

    /// <summary>
    /// 現在のタスクがクリアされたか監視する処理
    /// </summary>
    private void HandleCurrentStepTask()
    {
        // すべて終わっていたら何もしない
        if (currentStepIndex >= tutorialSteps.Length) return;

        // インデックスに応じて判定を切り替える
        switch (currentStepIndex)
        {
            //移動のチュートリアル
            case 0:
                // プレイヤーが移動キーを押している間、タイマーを進める
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                {
                    movementTimer += Time.deltaTime;

                    //ゲージを増やす
                    if (taskSlider != null)
                    {
                        taskSlider.value = movementTimer / MoveTaskTime;
                    }

                    if (movementTimer >= MoveTaskTime)
                    {
                        //クリアしたらスライダーをリセット
                        ClearCurrentStep();
                        taskSlider.value = 0; 
                        movementTimer = 0f;
                    }
                }
                break;

            //走る移動のチュートリアル
            case 1:
                // プレイヤーが移動キー&シフトキーを押している間、タイマーを進める
                if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && Input.GetKey(KeyCode.LeftShift))
                {
                    movementTimer += Time.deltaTime;

                    //ゲージを増やす
                    if (taskSlider != null)
                    {
                        taskSlider.value = movementTimer / MoveTaskTime;
                    }

                    if (movementTimer >= MoveTaskTime)
                    {
                        //クリアしたらスライダーをリセット
                        ClearCurrentStep();
                        taskSlider.value = 0;
                        movementTimer = 0f;
                    }
                }
                break;

            case 2:
                // プレイヤーがジャンプキーを押した際タイマーを進める
                if (Input.GetKeyDown(KeyCode.Space))
                {
                   jumpCount++;

                    //ゲージを増やす
                    if (taskSlider != null)
                    {
                        taskSlider.value = (float)jumpCount / JumpTaskTime;
                    }

                    if (jumpCount >= JumpTaskTime)
                    {
                        //クリアしたらスライダーをリセット
                        ClearCurrentStep();
                        taskSlider.value = 0;
                        jumpCount = 0;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// タスク達成時の処理（壁を消して次へ）
    /// </summary>
    public void ClearCurrentStep()
    {
        Debug.Log($"{tutorialSteps[currentStepIndex].stepName} クリア！");

        // 対応する壁を非表示にする
        if (tutorialSteps[currentStepIndex].wallObject != null)
        {
            tutorialSteps[currentStepIndex].wallObject.SetActive(false);
        }

        // 次のステップへ進む
        currentStepIndex++;
        SetUpStep(currentStepIndex);
    }

}
