using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
/// <summary>
/// ノイズのような仕組み
/// </summary>
public class TitleGlitchEffect : MonoBehaviour
{
    private Text titleText; 
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    [Header("バグる激しさ（移動距離）")]
    public float glitchIntensity = 5f;

    [Header("バグる確率")]
    public float glitchChance = 0.1f;

    void Start()
    {
        //格納
        titleText = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;

        // バグらせるループ処理をスタート
        StartCoroutine(GlitchRoutine());
    }

    /// <summary>
    /// ノイズのような表現をさせる処理
    /// </summary>
    /// <returns></returns>
    IEnumerator GlitchRoutine()
    {
        while (true)
        {
            // 設定した確率でバグが発生
            if (Random.value < glitchChance)
            {
                //位置をランダムにズラす
                float offsetX = Random.Range(-glitchIntensity, glitchIntensity);
                float offsetY = Random.Range(-glitchIntensity, glitchIntensity);
                rectTransform.anchoredPosition = originalPosition + new Vector2(offsetX, offsetY);

                //一瞬だけ文字を半透明にしてチカチカさせる
                titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, Random.Range(0.2f, 0.8f));

                // バグっている時間
                yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
            }
            else
            {
                // バグっていない時は元の位置・元の色に戻す
                rectTransform.anchoredPosition = originalPosition;
                titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, 1f);

                // 正常な状態をキープする時間
                yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
            }
        }
    }
}