using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST001 : MonoBehaviour
{
    [System.Serializable]
    public struct DATA001
    {
        [Header("名前")]
        public string name;
        [Header("耐久力")]
        public int Hp;
    }
    [Header("構造体リストデータ")]
    public List<DATA001> data;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
