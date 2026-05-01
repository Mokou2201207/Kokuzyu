using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class S_TypeB : State<AITester>
    {
        //切り替え時間
        float m_Times;
        //コンストラクタ
        public S_TypeB(AITester owner) : base(owner) { }
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //切り替え時間を初期化
            m_Times = 0.0f;
            Debug.Log("S_TypeBを起動しました!!");
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //キューブをZ軸移動
            owner.transform.Translate(new Vector3(0, 0, 0.1f));
            //1秒経ったら...
            if (m_Times > 1.0f)
            {
                //S_TypeA(A_Mode)へステート移動
                owner.ChangeState(AIState_ABType.A_Mode);
            }
            else
            {
                //秒間で代入
                m_Times += 1.0f * Time.deltaTime;
            }
        }
        public override void Exit()
        {
            owner.m_Counter++;
        }
    }
}
