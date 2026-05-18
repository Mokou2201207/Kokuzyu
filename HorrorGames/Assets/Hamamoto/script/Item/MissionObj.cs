using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionObj : MonoBehaviour
{
    [Header("オブジェクトの名前")]
    public string objName;

    [Header("オブジェクトのタイプ")]
    public ObjType objType;

    /// <summary>
    /// アイテムの名前
    /// </summary>
    public enum ObjType
    {
        //列車
        train
    }

    /// <summary>
    /// クロスヘアの処理から実行される処理
    /// </summary>
    public void MissionInteract()
    {
        // InventoryManagerからオブジェクトを触ったことを流す
        InventoryManager.instance.IntractMissionObj(objType);

        Debug.Log($"{objName} に触れた");
    }
}
