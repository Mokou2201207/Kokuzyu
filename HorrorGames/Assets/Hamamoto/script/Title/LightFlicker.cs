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

    private void Start()
    {
        targetLight = GetComponent<Light>();
        StartCoroutine(FlickerRoutine());
    }

    private System.Collections.IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // ライトの有効・無効をランダムに切り替える
            targetLight.enabled = Random.value > 0.1f; 

            // 明るさもランダムに変える
            targetLight.intensity = Random.Range(minIntensity, maxIntensity);

            // 次の点滅までの待機時間も少しランダムにする
            yield return new WaitForSeconds(Random.Range(0.01f, flickerSpeed));
        }
    }
}
