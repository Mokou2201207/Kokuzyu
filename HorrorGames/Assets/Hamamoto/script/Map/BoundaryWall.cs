using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 壁の処理　※範囲によって見えなくなります
/// </summary>
public class BoundaryWall : MonoBehaviour
{
    [Header("プレイヤーのTransform")]
    public Transform player;

    [Header("壁が見え始める距離")]
    public float visibleDistance = 5f;

    [Header("線が動くスピード")]
    public Vector2 scrollSpeed = new Vector2(0.5f, 0.5f);

    [Header("一番近づいた時の濃さ")]
    [Range(0f, 1f)] public float maxAlpha = 0.8f;

    private Material wallMaterial;
    private Collider wallCollider;

    /// <summary>
    /// 開始
    /// </summary>
    private void Start()
    {
        //レンダラーとコライダーを取得
        wallMaterial = GetComponent<Renderer>().material;
        wallCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// プレイヤーの距離によって壁の透明度を設定
    /// </summary>
    private void Update()
    {
        if (player == null)
        {
            // プレイヤーを遅延取得する
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return; // プレイヤーが見つかるまで処理をスキップ
            }
        }

        //textureの座標を少しずつずらす
        Vector2 currentOffset=wallMaterial.mainTextureOffset;
        wallMaterial.mainTextureOffset=currentOffset+scrollSpeed*Time.deltaTime;

        //プレイヤーと壁の距離を取得
        Vector3 closestPoint = wallCollider.ClosestPoint(player.position);
        float distance = Vector3.Distance(player.position, closestPoint);

        float alpha = Mathf.Clamp01(1f - (distance / visibleDistance)) * maxAlpha;

        //透明度をマテリアルに反映
        Color currentColor = wallMaterial.color;
        currentColor.a = alpha;
        wallMaterial.color = currentColor;
    }
}
