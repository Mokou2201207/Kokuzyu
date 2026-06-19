using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// オルゴール（デコイ）の処理
/// </summary>
public class DecoyTrap : NetworkBehaviour
{
    [Header("オルゴールのAudioSource")]
    [SerializeField] private AudioSource audioSource;

    [Header("効果時間（秒）")]
    [SerializeField] private float lifeTime = 60f;

    /// <summary>
    /// サーバー上で生成された瞬間に呼ばれる
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        //サーバ側で設定した時間によって消す（サーバから）
        Invoke(nameof(DestroySelf), lifeTime);
    }

    /// <summary>
    /// 開始
    /// </summary>
    private void Start()
    {
        //曲を再生
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// 敵がコライダー（トリガー）に入った瞬間
    /// </summary>
    /// <param name="other"></param>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enamy"))
        {
            DestroySelf();
        }
    }

    /// <summary>
    /// 自分自身のネットワークから消す
    /// </summary>
    [ServerCallback]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
