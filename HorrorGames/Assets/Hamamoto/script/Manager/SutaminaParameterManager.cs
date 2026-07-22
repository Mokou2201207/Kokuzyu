using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
/// <summary>
/// パラメータ処理
/// </summary>
public class SutaminaParameterManager : NetworkBehaviour
{
    [Header("スタミナのスライダ"), SerializeField]
    private Slider sutaminaSlider;
    [Header("スタミナが一秒間に減る量"), SerializeField]
    private float decreaseSpeed = 0.2f;
    [Header("スタミナが回復する間"), SerializeField]
    private float sutaminaHeelTime = 3f;
    [Header("呪いがかかっていてスタミナが一秒間に減る量"), SerializeField]
    private float curseDecreaseSpeed = 0.8f;
    [Header("呪いがかかっていてスタミナが回復する間"), SerializeField]
    private float curseSutaminaHeelTime = 8f;
    [Header("視界が暗くなるパネル"), SerializeField]
    private Image dizzinessBackground;

    [Header("メーターのアニメーションアタッチ"), SerializeField]
    private Animator metarAnimator;
    [Header("視界のアニメーションアタッチ"), SerializeField]
    private Animator shortnesAnimator;
    [Header("scriptをアタッチ"), SerializeField]
    private CurseManager curseManager;

    [Header("コンポーネントはキャンバスから"), SerializeField]
    private AudioSource audioSource;

    [Header("息切れSE")]
    [SerializeField] private AudioClip shortnessSE;

    // 回復までのカウントダウン用タイマー
    private float recoveryTimer = 0f;

    //今走っているかどうか
    public bool isRun = false;
    //ゲージが満タンかどうか
    [SerializeField] private bool isStaminaFull = true;
    //息切れして回復待ちかどうか
    public bool isExhausted = false;

    [Header("アイテムによるバフ")]
    public bool isSpeedBoosted = false;
    public bool isStaminaInfinite = false;

    public void UseStimulant()
    {
        StartCoroutine(StimulantCoroutine());
    }

    private IEnumerator StimulantCoroutine()
    {
        isSpeedBoosted = true;
        yield return new WaitForSeconds(15f);
        isSpeedBoosted = false;
    }

    public void UseSpeedEnergy()
    {
        StartCoroutine(SpeedEnergyCoroutine());
    }

    private IEnumerator SpeedEnergyCoroutine()
    {
        isStaminaInfinite = true;
        yield return new WaitForSeconds(20f);
        isStaminaInfinite = false;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (UIManager.instance != null)
        {
            UIManager.instance.sutaminaParameterManager = this;
            if (sutaminaSlider == null) sutaminaSlider = UIManager.instance.sutaminaSliderUI;
            if (dizzinessBackground == null) dizzinessBackground = UIManager.instance.dizzinessBackgroundUI;
            if (metarAnimator == null) metarAnimator = UIManager.instance.metarAnimatorUI;
            if (shortnesAnimator == null) shortnesAnimator = UIManager.instance.shortnesAnimatorUI;
            if (curseManager == null) curseManager = GetComponent<CurseManager>();

            if (sutaminaSlider != null)
            {
                sutaminaSlider.value = sutaminaSlider.maxValue;
            }
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (sutaminaSlider == null) return;

        //走っていて、息切れしていなかったら
        if (isRun && !isExhausted)
        {
            if (!isStaminaInfinite)
            {
                //減るスピードをフラグによって決める
                float currentDecrease = curseManager.isCurseFull ? curseDecreaseSpeed : decreaseSpeed;
                //減らす処理（共通）
                sutaminaSlider.value -= currentDecrease * Time.deltaTime;
            }
            // 走っている間は回復タイマーをリセットし続ける
            recoveryTimer = 0f;

            //ゲージが底ついたら
            if (sutaminaSlider.value <= 0.0f)
            {
                sutaminaSlider.value = 0.0f;
                isExhausted = true;
                isRun = false;
            }
        }
        else if (!isStaminaFull)
        {
            // タイマーをカウントアップ
            recoveryTimer += Time.deltaTime;

            //呪われているかどうかで回復量変化
            float normalWait = curseManager.isCurseFull ? curseSutaminaHeelTime : sutaminaHeelTime;

            //息切れ中なら５秒、通常なら３秒
            float waitTime = isExhausted ? 5f : normalWait;

            // タイマーが目標時間を超えたら回復開始
            if (recoveryTimer >= waitTime)
            {
                sutaminaSlider.value += 0.2f * Time.deltaTime;

                // 回復が始まったら息切れ状態は解除する
                isExhausted = false;
            }
        }

        //ゲージが0.3以下になったら赤色に
        if (sutaminaSlider.value <= 0.3f||curseManager.isCurseFull)
        {
            //メータとめまい導入
            metarAnimator.SetBool("MetarRed", true);
            shortnesAnimator.SetBool("shortnes", true);

            //息切れの音を入れる
            if (shortnessSE != null && !audioSource.isPlaying)
            {
                audioSource.clip = shortnessSE;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            ////メータとめまいを停止
            metarAnimator.SetBool("MetarRed", false);
            shortnesAnimator.SetBool("shortnes", false);

            //息切れをストップ
            if (audioSource.isPlaying && audioSource.clip == shortnessSE)
            {
                audioSource.Stop();
            }
        }

        //上限・下限の制限
        if (sutaminaSlider.value > 1.0f) sutaminaSlider.value = 1.0f;
        if (sutaminaSlider.value < 0.0f) sutaminaSlider.value = 0.0f;

        //値が一以上なら満タン
        isStaminaFull = (sutaminaSlider.value >= 1.0f);
    }
}
