using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateMachineAI
{
    /// <summary>
    /// パトロールモード(徘徊モード)
    /// </summary>
    public class BTA_Patrol : State<BattleTesterAI>
    {
        public Vector3 m_PatrolPoint;
        //コンストラクタ
        public BTA_Patrol(BattleTesterAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //プレイヤーがいない場合
            if (!owner.m_Player)
                owner.SetPlayer();

            //ナビゲーション起動
            owner.m_NavMeshAgent.enabled = true;

            //AnimatorのStateを徘徊モードへブレンド
            owner.AnimatorStateSetUp("徘徊モード");
            //Animatorは待機モードを実行
            owner.m_Animator.SetInteger("モード", 1);

            //適当な場所を指定
            m_PatrolPoint = new Vector3(Random.Range(10.0f, -10.0f), 0, Random.Range(10.0f, -10.0f));
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
            if (Vector3.Distance(owner.transform.position,m_PatrolPoint)<= 3.0f)
            {
                //パトロール終了に付き待機実行
                owner.ChangeState(AIState_BattleType.Idle);
            }
            else
            {
                //パトロールポイントー向かう
                owner.m_NavMeshAgent.SetDestination(m_PatrolPoint);
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