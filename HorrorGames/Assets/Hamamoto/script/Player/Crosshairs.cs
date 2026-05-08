using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// クロスヘア処理
/// </summary>
public class Crosshairs : MonoBehaviour
{
    [Header("クロスヘアのImage"), SerializeField]
    private Image crosshairImage;

    [Header("Rayの半径"), SerializeField]
    private float sphereRadius = 0.5f;
    [Header("Rayの距離"),SerializeField]
    private float maxDistance = 50f;

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(crosshairImage.transform.position);
        RaycastHit hit;

        // SphereCastを実行
        if (Physics.SphereCast(ray, sphereRadius, out hit, maxDistance))
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
            if (hit.collider.CompareTag("Item"))
            {
                Debug.Log("アイテムをとらえています");
            }
        }
    }
   
}
