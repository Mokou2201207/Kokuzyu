using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace StateMachineAI
{
    /// <summary>
    /// 戦闘モード
    /// </summary>
    public class BTA_Battle : State<BattleTesterAI>
    {
        float m_EnemyChange;
        //切り替え時間
        float m_CoolTime;
        //コンストラクタ
        public BTA_Battle(BattleTesterAI owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            m_EnemyChange = 10.0f;

            //プレイヤーがいない場合
            if (!owner.m_Player)
                owner.SetPlayer();

            //ナビゲーション停止
            owner.m_NavMeshAgent.enabled = false;

            //AnimatorのStateを戦闘モードへブレンド
            owner.AnimatorStateSetUp("戦闘モード");
            //Animatorは待機モードを実行
            owner.m_Animator.SetInteger("モード", 3);

            //CoolTimeセット
            m_CoolTime = Random.Range(0.5f, 1.0f);
            //攻撃停止(初期化)
            owner.m_Animator.SetInteger("攻撃", 0);
            //戦闘移動前進(初期化)
            owner.m_Animator.SetFloat("戦闘Z", 1.0f);
            owner.m_Animator.SetFloat("戦闘X", 0.0f);

        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            Brain();
        }
        public override void Exit() 
        {
            //攻撃停止
            owner.m_Animator.SetInteger("攻撃", 0);
            //戦闘移動停止
            owner.m_Animator.SetFloat("戦闘X", 0);
            owner.m_Animator.SetFloat("戦闘Z", 0);
        }
        public void Brain()
        {
            //実験用
            if (m_EnemyChange <= 0)
            {
                m_EnemyChange = 10.0f;
                //プレイヤー削除
                owner.m_Player = null;
                //プレイヤーを再選出
                owner.SetPlayer();
            }

            //戦闘行動クールタイム
            if (m_CoolTime <= 0.0f)
            {
                //Action確率算出
                int ActionCheck = Random.Range(0, 100);
                //攻撃停止
                owner.m_Animator.SetInteger("攻撃", 0);
                //戦闘移動停止
                owner.m_Animator.SetFloat("戦闘X", 0);
                owner.m_Animator.SetFloat("戦闘Z", 0);

                if (owner.m_Player)
                {
                    if (ActionCheck > 40)
                    {
                        //牽制(60%)
                        MoveAction();
                    }
                    else
                    {
                        //攻撃(40%)
                        AttackAction();
                    }
                }
            }
            else
            {
                //ターゲットへゆっくり向く
                LookUnit();
                //クールタイム減少
                m_CoolTime -= Time.deltaTime;
            }

            //プレイヤーがいない
            if (!owner.m_Player)
            {
                //待機モード
                owner.ChangeState(AIState_BattleType.Idle);
            }
            //攻撃範囲から離れたが索敵範囲にいる
            if (!owner.Sensor_AttackEnemyDistance(2.0f) && owner.Sensor_EnemyDetected())
            {
                //追跡開始
                owner.ChangeState(AIState_BattleType.Chase);
            }
            //索敵範囲から離れた
            if (!owner.Sensor_EnemyDetected())
            {
                //待機モード
                owner.ChangeState(AIState_BattleType.Idle);
            }
        }
        /// <summary>
        /// 攻撃処理
        /// </summary>
        public void AttackAction()
        {
            //攻撃コンボ数算出
            int SetActionPoint = Random.Range(1, 4);
            //攻撃パターン設定
            owner.m_Animator.SetInteger("攻撃", SetActionPoint);
            //戦闘移動停止
            owner.m_Animator.SetFloat("戦闘X", 0);
            owner.m_Animator.SetFloat("戦闘Z", 0);
            //CoolTimeをコンボ数で加算
            m_CoolTime = Random.Range(0.5f, 1.0f) * SetActionPoint;
        }
        /// <summary>
        /// 牽制処理
        /// </summary>
        public void MoveAction()
        {
            //攻撃停止
            owner.m_Animator.SetInteger("攻撃", 0);
            //戦闘移動の左右移動ランダム
            owner.m_Animator.SetFloat("戦闘X", Random.Range(1.0f,-1.0f));
            //プレイヤーとの相対距離算出
            if (Vector3.Distance(owner.m_Player.position, owner.transform.position) < 2.5f)
            {
                //2.5m以下の距離ならプレイヤーから離れる
                owner.m_Animator.SetFloat("戦闘Z", Random.Range(-0.5f, -1.0f));
            }
            else if (Vector3.Distance(owner.m_Player.position, owner.transform.position) > 4.5f)
            {
                //4.5m以上離れているならプレイヤーに近づく
                owner.m_Animator.SetFloat("戦闘Z", Random.Range(0.5f, 1.0f));
            }
            else
            {
                //それ以外の場合は現状維持
                owner.m_Animator.SetFloat("戦闘Z", 0);
            }
            m_CoolTime = Random.Range(1.0f, 3.0f);
        }
        /// <summary>
        /// 相手にゆっくり向く
        /// </summary>
        public void LookUnit()
        {
            //プレイヤーがいない場合は実行しない
            if (owner.m_Player == null) return;
            //プレイヤーへの向き
            Vector3 direction = owner.m_Player.position - owner.transform.position;
            //y軸0にして傾きをなくす
            direction.y = 0;
            //向きが変わらないなら処理終了
            if (direction == Vector3.zero) return;
            //相手への向きをクォータニオン化する
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 現在の回転から目標回転へ一定速度で近づける
            owner.transform.rotation = Quaternion.RotateTowards(
                owner.transform.rotation,               //現在のユニットの向き
                targetRotation,                         //ターゲットへのクォータニオン
                owner.m_RotateSpeed * Time.deltaTime    //回転速度(/秒)
            );
        }
    }
}