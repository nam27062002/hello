using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineEnterExitFlag : StateMachineBehaviour {

	[SerializeField] private string m_flag = "";

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		animator.SetBool(m_flag, true);
	}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		animator.SetBool(m_flag, false);
	}
}
