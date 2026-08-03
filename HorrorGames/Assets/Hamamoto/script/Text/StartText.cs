using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 最初の開始テキストの効果音
/// </summary>
public class StartText : MonoBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
      audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 開始処理
    /// </summary>
    private void StartTextSE()
    {
        audioSource.Play();
    }

    /// <summary>
    /// 英語の処理
    /// </summary>
    private void EndTextSE()
    {
        audioSource.Play();
        StartCoroutine(DestroyCoroutine());
    }

    /// <summary>
    /// 二秒後Textを消す
    /// </summary>
    /// <returns></returns>
    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }
}
