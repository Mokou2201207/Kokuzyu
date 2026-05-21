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

    [Header("コンポーネント自動")]
    [SerializeField] private Light flashlight;

    //バッテリがそこを付いたかどうか
    private bool isBatteryDead = false;

    private void Start()
    {
        //格納
        flashlight = GetComponent<Light>();

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

            //バッテリの底が付いたら
            if (batterySlider.value <= 0f)
            {
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
    }
}
