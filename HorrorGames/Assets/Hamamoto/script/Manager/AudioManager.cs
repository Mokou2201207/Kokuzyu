using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 音の処理
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("部品のSE")]
    [SerializeField] private AudioClip partsSE;
    [Header("ホラーアクションのSE")]
    [SerializeField] private AudioClip horrorPushSE;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // アイテムが追加されたイベントに音を鳴らす関数を登録
        InventoryManager.instance.OnItemAdded += PlayItemGetSound;
        InventoryManager.instance.OnMissionObj += PlayMissionSound;
    }

    /// <summary>
    /// アイテム用のサウンド
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>
    private void PlayItemGetSound(InteractableItem.ItemType type, int count)
    {
        AudioClip clipToPlay = null;

        // Switch文でアイテムごとに鳴らす音を分ける
        switch (type)
        {
            case InteractableItem.ItemType.Tire:
            case InteractableItem.ItemType.Coal:
            case InteractableItem.ItemType.Driver:
            case InteractableItem.ItemType.Key:
                //部品用のSEを追加
                clipToPlay = partsSE;
                break;
        }

        if (clipToPlay != null)
        {
            // 音を鳴らす
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    /// <summary>
    /// ミッション用のサウンド
    /// </summary>
    /// <param name="type"></param>
    /// <param name="count"></param>

    private void PlayMissionSound(MissionObj.ObjType type, int count)
    {
        AudioClip clipToPlay = null;

        // Switch文でアイテムごとに鳴らす音を分ける
        switch (type)
        {
            //列車
            case MissionObj.ObjType.train:
                clipToPlay = horrorPushSE;
                break;
        }

        if (clipToPlay != null)
        {
            // 音を鳴らす
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}
