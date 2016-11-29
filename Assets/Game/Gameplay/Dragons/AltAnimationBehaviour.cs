using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AltAnimationBehaviour : StateMachineBehaviour {

	

	public Range m_altAnimationTimer = new Range(4f, 6f);
	public int m_numAlternativeAnim;
	private float m_timer = 4f;
	private int m_currentAnimIndex = -1;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_timer = m_altAnimationTimer.GetRandom();
		m_currentAnimIndex = -1;
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {		
		// if main looping animation
		if ( m_currentAnimIndex < 0 )	
		{
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) 
			{
				// Set alternative animation
				m_currentAnimIndex = Random.Range(0,m_numAlternativeAnim);
				animator.SetInteger("AltAnimation", m_currentAnimIndex);
			}	
		}
	}


}
