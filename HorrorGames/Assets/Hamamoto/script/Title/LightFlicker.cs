using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private Light targetLight;

    [Header("点滅の激しさ（小さいほどチラつく）")]
    [SerializeField] private float flickerSpeed = 0.05f;

    [Header("ライトの強さの範囲")]
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.2f;

    [Header("Light_18のマテリアル連動")]
    [SerializeField] private Renderer lightObjectRenderer;

    private Material lightMaterial;

    private void Start()
    {
        targetLight = GetComponent<Light>();

        // Light_18のマテリアルを取得（インスタンス化して他に影響しないようにする）
        if (lightObjectRenderer != null)
        {
            lightMaterial = lightObjectRenderer.material;
            // Emissionを有効にする
            lightMaterial.EnableKeyword("_EMISSION");
        }

        StartCoroutine(FlickerRoutine());
    }

    private System.Collections.IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // ライトの有効・無効をランダムに切り替える
            bool isOn = Random.value > 0.1f;
            targetLight.enabled = isOn;

            // 明るさもランダムに変える
            float intensity = Random.Range(minIntensity, maxIntensity);
            targetLight.intensity = intensity;

            // Light_18のマテリアルカラーをライトの強さに連動させる
            if (lightMaterial != null)
            {
                if (!isOn)
                {
                    // ライトがOFFの時は真っ黒
                    lightMaterial.SetColor("_EmissionColor", Color.black);
                    lightMaterial.color = Color.black;
                }
                else
                {
                    // ライトの強さを0~1に正規化して白黒を決める
                    float t = Mathf.InverseLerp(minIntensity, maxIntensity, intensity);
                    Color emissionColor = Color.Lerp(Color.black, Color.white, t);
                    lightMaterial.SetColor("_EmissionColor", emissionColor);
                    lightMaterial.color = Color.Lerp(Color.gray, Color.white, t);
                }
            }

            // 次の点滅までの待機時間も少しランダムにする
            yield return new WaitForSeconds(Random.Range(0.01f, flickerSpeed));
        }
    }
}
