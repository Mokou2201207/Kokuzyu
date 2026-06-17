using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// 呪いゲージの処理
/// </summary>
public class CurseManager : NetworkBehaviour
{
    [Header("呪い用のスライダー"),SerializeField]
    private Slider curseSlider;

    [Header("呪いが持つ時間"), SerializeField]
    private float curseConsumptionTime = 120f;

    [Header("ゲージが赤くなるアニメーション")]
    [SerializeField] private Animator animator;

    [Header("AudioSouse")]
    [SerializeField]private AudioSource curseAudioSource;
    [Header("十字架を使った音")]
    [SerializeField] private AudioClip useTheCrossSE;

    //呪いゲージが満タンになったか
    public bool isCurseFull=false;

    /// <summary>
    /// 開始
    /// </summary>
    private void Start()
    {
        //最初はスライダーのメータをゼロに
        if (curseSlider!=null)
        {
            curseSlider.value = curseSlider.minValue;
        }
    }

    /// <summary>
    /// 自分だけ最初にアタッチさせる※混合させないため
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //外部からアタッチさせる
        if (UIManager.instance!=null)
        {
            UIManager.instance.curseManager = this;
            if(curseSlider==null) curseSlider = UIManager.instance.curseSliderUI;
            if (animator == null) animator = UIManager.instance.curseAnimatorUI;

            //メータを変更
            if (curseSlider != null)
            {
                curseSlider.value = curseSlider.minValue;
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (curseSlider == null) return;

        //フルなら処理をしない
        if (!isCurseFull)
        {
            //一秒で減らすべきの量を特定
            float cursePerSecound = curseSlider.maxValue / curseConsumptionTime;
            //特定した秒数で増やしていく
            curseSlider.value += cursePerSecound*Time.deltaTime;

            if (curseSlider.value>=curseSlider.maxValue)
            {
                //メータが超えないように固定
                curseSlider.value= curseSlider.maxValue;
                isCurseFull = true;

            }
        }

        //8割ならメータを赤く
        if (curseSlider.value >= curseSlider.maxValue * 0.8f)
        {
            animator.SetBool("MetarRed", true);
        }
        else
        {
            animator.SetBool("MetarRed", false);
        }
    }

    /// <summary>
    /// 十字架を使う処理
    /// </summary>
    public void UseTheCross()
    {
        if (curseSlider == null) return;
        //SE
        curseAudioSource.PlayOneShot(useTheCrossSE);

        //呪いのゲージを初期化
        curseSlider.value=curseSlider.minValue;
        isCurseFull = false;
    }
}
