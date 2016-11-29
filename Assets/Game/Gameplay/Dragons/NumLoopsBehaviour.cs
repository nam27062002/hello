using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NumLoopsBehaviour : StateMachineBehaviour {

	public int m_minLoops = 1;
	public int m_maxLoops = 1;
	private int m_missingLoops = 0;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_missingLoops = Random.Range( m_minLoops, m_maxLoops+1 );
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( (int)stateInfo.normalizedTime >= m_missingLoops )
		{
			animator.SetInteger("AltAnimation", -1);
		}
	}


}
