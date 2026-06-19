using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// スロットのKeyの選択処理
/// </summary>
public class PlayerInventoryInput : NetworkBehaviour
{
    [Header("落とすオルゴールのprefab")]
    [SerializeField] private GameObject musicBoxPrefab;

    public override void OnStartLocalPlayer()
    {
        InventoryManager.instance.OnUseMusicBox += RequestSpawnMusicBox;
    }

    private void OnDestroy()
    {
        // キャラクターが消える時は、エラーを防ぐために耳を塞ぐ（登録解除）
        if (isLocalPlayer && InventoryManager.instance != null)
        {
            InventoryManager.instance.OnUseMusicBox -= RequestSpawnMusicBox;
        }
    }


    private void Update()
    {
        //自分のキャラクター以外は処理しない
        if (!isLocalPlayer) return;

        //番号Keyでスロットを選択
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            InventoryManager.instance.SelectSlot(0);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            InventoryManager.instance.SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            InventoryManager.instance.SelectSlot(2);
        }

        //Eでアイテムを使用
        if (Input.GetKeyDown(KeyCode.F))
        {
            InventoryManager.instance.UseSelectedItem();
        }
    }

    /// <summary>
    /// InventoryManagerからオルゴールを使用した実行される
    /// </summary>
    private void RequestSpawnMusicBox()
    {
        // プレイヤーの少し前の座標を計算
        Vector3 dropPosition = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

        // サーバーに対してオルゴールを置くように命令
        CmdSpawnMusicBox(dropPosition, transform.rotation);
    }

    /// <summary>
    /// 【サーバー専用処理】実際に世界にオルゴールを出現させる
    /// </summary>
    [Command]
    private void CmdSpawnMusicBox(Vector3 spawnPos, Quaternion spawnRot)
    {
        if (musicBoxPrefab == null) return;

        // サーバー上で生成
        GameObject decoy = Instantiate(musicBoxPrefab, spawnPos, spawnRot);

        // 全員の画面に同期して出現させる
        NetworkServer.Spawn(decoy);
    }
}
