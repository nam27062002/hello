﻿using UnityEngine;
using System.Collections;

public class FlyLoopBehaviour : StateMachineBehaviour {

	public Range m_timeToGlide = new Range(3f, 4f);
	private float m_timer = 4f;
	
	
	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		ResetTimer();
	}
	
	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {		
		animator.SetBool("glide", false);
		animator.SetBool("glide alt", false);
	}

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			animator.SetBool("glide", true);
			ResetTimer();
		}
	}


	public void ResetTimer()
	{
		m_timer = m_timeToGlide.GetRandom();
	}
	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called before OnStateMove is called on any state inside this state machine
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called before OnStateIK is called on any state inside this state machine
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
