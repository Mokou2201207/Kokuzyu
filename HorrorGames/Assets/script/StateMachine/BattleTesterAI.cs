using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くでなきい。
    /// </summary>
    /// 
    public enum AIState_BattleType
    {
        Idle,           //待機
        Patrol,         //徘徊
        Chase,          //追跡
        Battle,         //戦闘
        Dead,           //死亡
    }

    public class BattleTesterAI
        : StatefulObjectBase<BattleTesterAI, AIState_BattleType>
    {
        [Header("アニメーターリンク")]
        public Animator m_Animator;
        [Header("Navigationリンク")]
        public NavMeshAgent m_NavMeshAgent;
        [Header("ターゲット")]
        public Transform m_Player;
        [Header("戦闘中の旋回速度")]
        public float m_RotateSpeed = 180.0f;


        void Start()
        {
            //Animatorをリンクする
            m_Animator = GetComponent<Animator>();
            stateList.Add(new BTA_Idol(this));      //待機Stateを導入
            stateList.Add(new BTA_Patrol(this));    //徘徊Stateを導入
            stateList.Add(new BTA_Chase(this));     //追跡Stateを導入
            stateList.Add(new BTA_Battle(this));    //戦闘Stateを導入
            stateList.Add(new BTA_Dead(this));      //死亡Stateを導入
            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<BattleTesterAI>();

            //初期起動時は、待機に移行させる
            ChangeState(AIState_BattleType.Patrol);
        }
        /// <summary>
        /// 強制的にAnimationステートへ切り替える
        /// </summary>
        /// <param name="StateName">レイヤー・ステート名</param>
        public void AnimatorStateSetUp(string StateName)
        {
            // StateName内の名前のレイヤー番号を取得
            int layerIndex = m_Animator.GetLayerIndex(StateName);
            // 現在のアニメーションからStateNameの名前のステートへ0.1秒かけてブレンド
            m_Animator.CrossFade(StateName, 0.1f, layerIndex, 0f);
        }



        /// <summary>
        /// 【簡易】センサーが敵を発見、敵を発見した
        /// </summary>
        public bool Sensor_EnemyDetected()
        {
            //フラグなし
            bool Flag = false;
            //プレイヤーがいる
            if (m_Player)
            {
                //相対距離10m以内
                if (Vector3.Distance(transform.position, m_Player.position) < 10.0f)
                {
                    //フラグオン
                    Flag = true;
                }
            }
            //フラグを返す
            return Flag;
        }
        /// <summary>
        /// 【簡易】センサーが敵との交戦距離に入った事を伝える
        /// </summary>
        /// <returns></returns>
        public bool Sensor_AttackEnemyDistance(float AddPoint)
        {
            //フラグを無し
            bool Flag = false;
            //プレイヤーがいる
            if (m_Player)
            {
                //相対距離3m以内
                if (Vector3.Distance(transform.position, m_Player.position) < 3.0f + AddPoint)
                {
                    //フラグオン
                    Flag = true;
                }
            }
            //フラグを返す
            return Flag;
        }
        /// <summary>
        /// テスト用。プレイヤーを強制セットアップ
        /// </summary>
        public void SetPlayer()
        {
            //プレイヤーがいない?!
            if (!m_Player)
            {
                //全てのオブジェクトで【プレイヤータグ】を全て洗い出す
                GameObject[] Dummy = GameObject.FindGameObjectsWithTag("Player");
                //全てのプレイヤータグからランダムで抽選
                m_Player = Dummy[UnityEngine.Random.Range(0, Dummy.Length)].transform;
                //もし、自分と同じ者がターゲットになった場合は、無効(nullにしてやり直し)
                if (m_Player == transform)
                    m_Player = null;
                else
                {
                    //対象が死んでいる場合は除外
                    if (m_Player.GetComponent<Parameta>().m_Hp <= 0)
                        m_Player = null;
                }
            }
        }

        /// <summary>
        /// 被弾
        /// プラスは正面から、マイナスは後ろから
        /// </summary>
        public void Hit()
        {
            // 「被弾」という名前のレイヤー番号を取得
            int layerIndex = m_Animator.GetLayerIndex("被弾");
            // 正面被弾
            m_Animator.SetInteger("被弾", UnityEngine.Random.Range(0, 2));
            // 現在のアニメーションから「Hit」ステートへ0.1秒かけてブレンド
            m_Animator.CrossFade("被弾", 0.1f, layerIndex, 0f);
        }

        /// <summary>
        /// 回避
        /// </summary>
        public void Dodge()
        {
            // 「被弾」という名前のレイヤー番号を取得
            int layerIndex = m_Animator.GetLayerIndex("回避");
            // ランダム回避
            m_Animator.SetInteger("回避", UnityEngine.Random.Range(0, 7));
            // Animation強制実行
            m_Animator.Play("回避", layerIndex, 0f);
        }
        /// <summary>
        /// 死亡
        /// </summary>
        public void Dead()
        {
            // 「被弾」という名前のレイヤー番号を取得
            int layerIndex = m_Animator.GetLayerIndex("死亡");
            // 現在のアニメーションから「Hit」ステートへ0.1秒かけてブレンド
            m_Animator.CrossFade("死亡", 0.1f, layerIndex, 0f);
            //死亡Stateに移行させる
            ChangeState(AIState_BattleType.Dead);

        }
        /// <summary>
        /// 死亡によるユニット削除
        /// </summary>
        public void SetDestroy()
        {
            Destroy(gameObject);
        }
    }
}