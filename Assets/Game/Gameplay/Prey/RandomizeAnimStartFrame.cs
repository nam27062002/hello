﻿using UnityEngine;
using System.Collections;

public class RandomizeAnimStartFrame : StateMachineBehaviour {
	private bool m_randomizeFrame = true;
	 
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_randomizeFrame) {
			animator.Play(stateInfo.fullPathHash, layerIndex, Random.Range(0f, 1f));
			m_randomizeFrame = false;
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layerIndex);
		m_randomizeFrame = nextStateInfo.length > 0 && nextStateInfo.fullPathHash != stateInfo.fullPathHash;
	}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
