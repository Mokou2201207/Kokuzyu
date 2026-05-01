using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace StateMachineAI
{
    /// <summary>

    public class BTA_Dead : State<BattleTesterAI>
    {
        //待機時間
        public float m_DeadTime;

        //コンストラクタ
        public BTA_Dead(BattleTesterAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //死亡消滅カウンター
            m_DeadTime = 3.0f;
            //ナビゲーション停止
            owner.m_NavMeshAgent.enabled = false;
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            Brain();
        }
        public override void Exit()
        {
        }
        public void Brain()
        {
            if (m_DeadTime <= 0.0f)
            {
                owner.SetDestroy();
            }
            else
            {
                m_DeadTime -= Time.deltaTime;
            }
        }
    }
}