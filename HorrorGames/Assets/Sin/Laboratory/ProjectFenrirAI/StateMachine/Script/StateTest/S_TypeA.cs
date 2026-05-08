using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace StateMachineAI
{
    public class S_TypeA : State<AITester>
    {
        //切り替え時間
        float m_Times;
        //コンストラクタ
        public S_TypeA(AITester owner) : base(owner){}
        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
            //切り替え時間を初期化
            m_Times = 0.0f;
            Debug.Log("S_TypeAを起動しました!!");

            if (owner.m_Counter > 2)
            {
                owner.stateList[1] = new S_TypeC(owner);
            }
        }
        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay() 
        {
            //キューブを回転
            owner.transform.Rotate(new Vector3(1, 1, 1));
            //５秒経ったら...
            if (m_Times > 5.0f)
            {
                //S_TypeB(B_Mode)へステート移動
                owner.ChangeState(AIState_ABType.B_Mode);
            }
            else
            {
                //秒間で代入
                m_Times += 1.0f * Time.deltaTime;
            }
        }
        public override void Exit() {}
    }
}