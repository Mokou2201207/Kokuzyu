using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace StateMachineAI
{
    /// <summary>
    /// 待機モード
    /// </summary>
    public class BTA_Idol : State<BattleTesterAI>
    {
        //待機時間
        public float m_CheckTime;

        //コンストラクタ
        public BTA_Idol(BattleTesterAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //プレイヤーがいない場合
            if (!owner.m_Player)
                owner.SetPlayer();

            //ナビゲーション停止
            owner.m_NavMeshAgent.enabled = false;
            //AnimatorのStateを待機モードへブレンド
            owner.AnimatorStateSetUp("待機モード");
            //Animatorは待機モードを実行
            owner.m_Animator.SetInteger("モード", 0);

            //パトロールへの切り替え時間2～4秒
            m_CheckTime = Random.Range(2.0f, 4.0f);
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
            //待機時間が0以下
            if (m_CheckTime <= 0.0f)
            {
                //パトロール実行
                owner.ChangeState(AIState_BattleType.Patrol);
            }
            else
            {
                //時間減少
                m_CheckTime -= 1.0f * Time.deltaTime;
            }
            //敵を発見
            if (owner.Sensor_EnemyDetected())
            {
                //追跡実行
                owner.ChangeState(AIState_BattleType.Chase);
            }
        }
    }
}