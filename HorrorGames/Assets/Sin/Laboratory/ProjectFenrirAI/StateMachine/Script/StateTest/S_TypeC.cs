using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace StateMachineAI
{
    public class S_TypeC : State<AITester>
    {
        /// <summary>
        /// TypeC用コンストラクタ
        /// </summary>
        /// <param name="owner">AITester</param>
        public S_TypeC(AITester owner) : base(owner) { }


        //このAIが起動した瞬間に実行(Startと同義)
        public override void Enter()
        {
        }


        //このAIが起動中に常に実行(Updateと同義)
        public override void Stay()
        {
            //Updateと同じように処理を描く
            owner.transform.localScale = new Vector3(2.0f,2.0f,2.0f);
        }

        //このAI(State)が終了して、次のステートへ移る前に実行
        public override void Exit() 
        {
        }
    }
}