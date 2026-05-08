using System.Collections.Generic;
using UnityEngine;


public class RepopSystem : MonoBehaviour
{
    public GameObject m_PopUnit;
    public List<GameObject> m_Unit;
    public float m_Times = 2.0f;
    public float m_MaxTimes = 2.0f;
    public int m_MaxCount = 20;
    void Start()
    {
        
    }

    void Update()
    {
        // null—v‘f‚ð‚·‚×‚Äíœ
        m_Unit.RemoveAll(item => item == null);

        if (m_Times <= 0.0f)
        {
            if (m_Unit.Count < m_MaxCount)
            {
                GameObject Unit = Instantiate(
                    m_PopUnit,
                    new Vector3(Random.Range(10.0f, -10.0f), 0, Random.Range(10.0f, -10.0f)),
                    Quaternion.identity);
                m_Unit.Add(Unit);
            }
            m_Times = m_MaxTimes;
        }
        else
        {
            m_Times -= Time.deltaTime;
        }
    }
}
