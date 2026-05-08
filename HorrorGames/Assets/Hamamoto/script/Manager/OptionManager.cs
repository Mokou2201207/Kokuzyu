using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// option設定処理
/// </summary>
public class OptionManager : MonoBehaviour
{
    [Header("コンポーネントを自動アタッチ")]
    [SerializeField]private AudioSource optionAudioSource;

    [Header("オプションのパネル"), SerializeField]
    private Image optionPanal;

    [Header("オプションを開くSE"),SerializeField]
    private AudioClip optionAudioClip;

    //オプションを開いているか
    private bool isOpenOption=false;

    private void Start()
    {
        //最初はオプションのパネルを非表示
        optionPanal.gameObject.SetActive(false);

        //Tagで検索
        if (optionAudioSource==null)
        {
            GameObject AudioObj = GameObject.FindGameObjectWithTag("Player");
            if (AudioObj!=null)
            {
                optionAudioSource = AudioObj.GetComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        //オプションを開く処理
        if (Input.GetKeyDown(KeyCode.Escape)&&!isOpenOption)
        {
            isOpenOption = true;
            //再生
            optionAudioSource.PlayOneShot(optionAudioClip);
            optionPanal.gameObject.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape)&&isOpenOption)
        {
            isOpenOption = false;
            optionPanal.gameObject.SetActive(false);
        }
    }
}
