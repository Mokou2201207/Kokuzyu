using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// バッテリーの消費などの処理
/// </summary>
public class BatteryParametarManager : MonoBehaviour
{
    [Header("バッテリースライダー"), SerializeField]
    private Slider batterySlider;

    [Header("バッテリが持つ時間"), SerializeField]
    private float batteryConsumptionTime = 180f;

    [Header("バッテリー切れ時の霧の濃さ")]
    [SerializeField] private float deadBatteryFogDensity = 0.5f;

    [Header("バッテリーを補充するSE")]
    [SerializeField] private AudioClip supplementBatterySE;
    [Header("充電が切れるSE")]
    [SerializeField] private AudioClip deadBatterySE;
    [Header("ゲージが赤くなるアニメーション")]
    [SerializeField] private Animator animator;

    [Header("コンポーネント自動")]
    [SerializeField] private Light flashlight;
    [SerializeField] private AudioSource audioSource;

    //バッテリがそこを付いたかどうか
    private bool isBatteryDead = false;

    private void Start()
    {
        //格納
        flashlight = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();

        //ゲーム開始時にスライダーをMAXにしておく
        if (batterySlider != null)
        {
            batterySlider.value = batterySlider.maxValue;
        }
    }

    private void Update()
    {
        if (batterySlider == null) return;
        //バッテリが底をついてなければ消費する
        if (!isBatteryDead)
        {
            //一秒で減らすべきの量を特定
            float decreasePerSecound = batterySlider.maxValue / batteryConsumptionTime;
            //特定した秒数で減らしていく
            batterySlider.value -= decreasePerSecound * Time.deltaTime;

            //ライトを付ける
            if (flashlight != null)
            {
                flashlight.enabled = true;
            }

            //霧を濃く
            RenderSettings.fogDensity =0.15f;

            //バッテリの底が付いたら
            if (batterySlider.value <= 0f)
            {
                //二重防止
                if (!isBatteryDead)
                {
                    //SE
                    audioSource.PlayOneShot(deadBatterySE);
                }

                // マイナスにならないように0に固定
                batterySlider.value = 0f;
                isBatteryDead = true;

                Debug.Log("バッテリーが切れました！");

                //ライトを消す
                if (flashlight != null)
                {
                    flashlight.enabled = false;
                }

                //霧を濃く
                RenderSettings.fogDensity = deadBatteryFogDensity;
            }
        }

        //ゲージが0.2以下になったら赤色に
        if (batterySlider.value<=0.2f)
        {
            animator.SetBool("MetarRed", true);
        }
        else
        {
            animator.SetBool("MetarRed", false);
        }
    }

    /// <summary>
    /// バッテリーを補充する処理
    /// </summary>
    public void SupplementBattery()
    {
        if (batterySlider == null) return;

        //SE
        audioSource.PlayOneShot(supplementBatterySE);

        //バッテリーをマックスに
        batterySlider.value = batterySlider.maxValue;
        isBatteryDead = false;
    }
}
