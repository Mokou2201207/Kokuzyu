using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace StateMachineAI
{
    public enum StateType
    {
        A_Mode,
    }
    public class TestAI:StatefulObjectBase<TestAI, StateType>
    {
        void Start()
        {
            //stateList.Add(new )
            stateMachine = new StateMachine<TestAI>();
            ChangeState(StateType.A_Mode);
        }
    }
}
