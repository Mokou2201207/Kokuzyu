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

    [Header("通常のクロスヘア"), SerializeField]
    private Sprite normalSprite;
    [Header("アイテムの場合のクロスヘア"), SerializeField]
    private Sprite targetSprite;


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
            //アイテムに当たってるとき
            if (hit.collider.TryGetComponent<InteractableItem>(out var item))
            {
                //スプライト変換
               crosshairImage.sprite=targetSprite;

                // EKeyでアイテムを入手
                if (Input.GetKeyDown(KeyCode.E))
                {
                    item.ItemInteract();
                }

            }
            else if(hit.collider.TryGetComponent<MissionObj>(out var mission))
            {
                //スプライト変換
                crosshairImage.sprite = targetSprite;

                // EKeyでアイテムを入手
                if (Input.GetKeyDown(KeyCode.E))
                {
                    mission.MissionInteract();
                }
            }
            else
            {
                //スプライト変換
                crosshairImage.sprite = normalSprite;
            }
        }
        //ものがまずなにも当たってない時
        else
        {
            crosshairImage.sprite = normalSprite;
        }
    }
   
}
