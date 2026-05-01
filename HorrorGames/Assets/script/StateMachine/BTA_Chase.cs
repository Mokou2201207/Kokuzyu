using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateMachineAI
{
    /// <summary>
    /// 追跡モード
    /// </summary>
    public class BTA_Chase : State<BattleTesterAI>
    {
        //コンストラクタ
        public BTA_Chase(BattleTesterAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //プレイヤーがいない場合
            if (!owner.m_Player)
                owner.SetPlayer();


            //ナビゲーション起動
            owner.m_NavMeshAgent.enabled = true;
            //AnimatorのStateを追跡モードへブレンド
            owner.AnimatorStateSetUp("追跡モード");
            //Animatorは待機モードを実行
            owner.m_Animator.SetInteger("モード", 2);

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
            //追跡中に、プレイヤーがいない?
            if (!owner.m_Player)
            {
                //追跡停止、待機モード
                owner.ChangeState(AIState_BattleType.Idle);
            }
            //追跡中に索敵範囲を外れた
            else if (!owner.Sensor_EnemyDetected())
            {
                //追跡中止、待機モード
                owner.ChangeState(AIState_BattleType.Idle);
            }
            //追跡中に戦闘可能範囲に入った
            else if (owner.Sensor_AttackEnemyDistance(0))
            {
                //追跡中止、戦闘モード
                owner.ChangeState(AIState_BattleType.Battle);
            }
            else
            {
                if (owner.m_Player)
                {
                    //プレイヤーを追いかける
                    owner.m_NavMeshAgent.SetDestination(owner.m_Player.position);
                }
                else
                {
                    //追跡中止、待機モード
                    owner.ChangeState(AIState_BattleType.Idle);
                }
            }
        }
    }
}