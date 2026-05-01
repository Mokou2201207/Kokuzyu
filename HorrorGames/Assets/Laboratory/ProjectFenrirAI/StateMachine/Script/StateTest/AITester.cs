using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;

namespace StateMachineAI
{
    /// <summary>
    /// 敵のステートリスト
    /// ここでステートを登録していない場合、
    /// 該当する行動が全くでなきい。
    /// </summary>
    /// 
    public enum AIState_ABType
    {
        A_Mode,     //0
        B_Mode,     //1
    }


    public class AITester 
        : StatefulObjectBase<AITester, AIState_ABType>
    {
        public int m_Counter = 0;
        void Start()
        {

            /*
            //存在していないクラスが指定されたら本体消滅
            if (!AddStateByName("S_TypeA"))
                Destroy(gameObject);
            if (!AddStateByName("S_TypeB"))
                Destroy(gameObject);
            */
            //前回の奴
            //S_TypeA ステートを登録する(ステートリスト0番目)
            stateList.Add(new S_TypeA(this));
            //S_TypeB ステートを登録する(ステートリスト1番目)
            stateList.Add(new S_TypeB(this));
            //ステートマシーンを自身として設定
            stateMachine = new StateMachine<AITester>();

            //初期起動時は、A_Modeに移行させる
            ChangeState(AIState_ABType.A_Mode);
        }
        /// <summary>
        /// クラス名を元にステートを生成して追加する
        /// </summary>
        /// <param name="ClassName">生成するクラスの名前</param>
        public bool AddStateByName(string ClassName)
        {
            try
            {
                // 現在のアセンブリからクラスを取得
                Type StateType = Assembly.GetExecutingAssembly().GetType($"StateMachineAI.{ClassName}");

                // クラスが見つからなかった場合の対処
                if (StateType == null)
                {
                    Debug.LogError($"{ClassName} クラスが見つかりませんでした。");
                    return true;
                }

                // 型が State<AITester> かどうかをチェック
                if (!typeof(State<AITester>).IsAssignableFrom(StateType))
                {
                    Debug.LogError($"{ClassName} は State<EnemyAI> 型ではありません。\nだからよ…止まるんじゃ…ねぇぞ…。");
                    return true;
                }

                // インスタンスを生成
                System.Reflection.ConstructorInfo Constructor =
                    StateType.GetConstructor(new[] { typeof(AITester) });
                

                if (Constructor == null)
                {
                    Debug.LogError($"{ClassName} のコンストラクタが見つかりませんでした。\nああ――今夜はこんなにも、月が綺麗だ――。");
                    return true;
                }

                State<AITester> StateInstance = 
                    Constructor.Invoke(new object[] { this }) as State<AITester>;

                if (StateInstance != null)
                {
                    // ステートリストに追加
                    stateList.Add(StateInstance);
                    Debug.Log($"{ClassName} をステートリストに追加しました。");
                    return true;
                }
                else
                {
                    Debug.LogError($"{ClassName} のインスタンス生成に失敗しました。みんな死ぬしかないじゃない!!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"エラーが発生しました。ありえんw: {ex.Message}");
                return false;
            }
        }
    }
}
