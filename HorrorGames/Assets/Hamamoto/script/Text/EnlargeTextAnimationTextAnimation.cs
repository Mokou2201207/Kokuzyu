using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Textにcursorを合わせたら拡大する仕組み
/// </summary>
public class EnlargeTextAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("アニメーション設定")]
    [SerializeField] private float hoverScale = 1.15f; // どのくらい大きくするか
    [SerializeField] private float duration = 0.2f;    // アニメーションの時間

    [Header("AudioSouseをアタッチ自動")]
    [SerializeField] private AudioSource audioSource;

    [Header("ボタンに触れたときのSE"), SerializeField]
    private AudioClip titleButtunActionSE;

    private bool inButtunAction=false;
    private Vector3 defaultScale;
    private Button button;

    private void Start()
    {
        // ビルド時にFPSが上がりすぎてカメラが暴れるのを防ぐため、フレームレートを固定
        Application.targetFrameRate = 60;

        //オブジェクトを探しアタッチ
        GameObject audio = GameObject.Find("TitleCanvas");
        if (audio != null)
        {
            audioSource = audio.GetComponent<AudioSource>();
        }

        defaultScale = transform.localScale;
        inButtunAction=false ;
        button = GetComponent<Button>();
    }

    /// <summary>
    /// カーソルにTextが当たってる間拡大
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!inButtunAction)
        {
            audioSource.PlayOneShot(titleButtunActionSE);
        }
        transform.DOScale(defaultScale * hoverScale, duration).SetEase(Ease.OutBack);
        inButtunAction = true;
    }


    /// <summary>
    /// カーソルからTextを外した場合縮小
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(defaultScale, duration).SetEase(Ease.Linear);
        inButtunAction=false;
    }



    /// <summary>
    /// ゲーム終了
    /// </summary>
    public void OnQuitClick()
    {
        Debug.Log("ゲーム終了");
        Application.Quit();
    }


}