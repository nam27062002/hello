using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NumLoopsBehaviour : StateMachineBehaviour {

	public int m_minLoops = 1;
	public int m_maxLoops = 1;
	public int m_optionalId = -1;
	private float m_missingLoops = 0;
	private float m_offset = 0.25f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_missingLoops = Random.Range( m_minLoops, m_maxLoops+1 );
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( stateInfo.normalizedTime >= (m_missingLoops - m_offset) ){
			animator.SetInteger(GameConstants.Animator.ALT_ANIMATION, -1);
		}
	}


}
