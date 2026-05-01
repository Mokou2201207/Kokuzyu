using Unity.VisualScripting;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    [Header("アニメーターリンク")]
    public Animator m_Animator;
    [Header("銃装備アウトポイント")]
    public Transform m_OutPoint;
    [Header("銃装備")]
    public ARM m_ARM;
    void Start()
    {
        if (!m_Animator)
            m_Animator = GetComponent<Animator>();

        //保険
        // マウスカーソルを非表示にする
        Cursor.visible = false;
        // マウスカーソルを画面中央に固定する
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //発砲
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
        //移動
        if (m_Animator)
        {
            m_Animator.SetFloat("X", Input.GetAxis("Horizontal"));
            m_Animator.SetFloat("Y", Input.GetAxis("Vertical"));
        }
        //回転
        transform.Rotate(new Vector3(0,Input.GetAxis("Mouse X"),0));
    }
    /// <summary>
    /// 発砲
    /// </summary>
    public void Fire()
    {
        // 「発砲」という名前のレイヤー番号を取得
        int layerIndex = m_Animator.GetLayerIndex("発砲");
        // 現在のアニメーションから「Hit」ステートへ0.1秒かけてブレンド
        m_Animator.CrossFade("発砲", 0.0f, layerIndex, 0f);
    }
    /// <summary>
    /// 死亡
    /// </summary>
    public void Dead()
    {
        // 「被弾」という名前のレイヤー番号を取得
        int layerIndex = m_Animator.GetLayerIndex("死亡");
        // 現在のアニメーションから「Hit」ステートへ0.1秒かけてブレンド
        m_Animator.CrossFade("死亡", 0.1f, layerIndex, 0f);
    }
    /// <summary>
    /// 死亡によるユニット削除
    /// </summary>
    public void SetDestroy()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 発砲によるイベントが実行された
    /// </summary>
    public void OnFireEvent()
    {
        if (m_ARM)
        {
            m_ARM.OnFire();
        }
    }
}
