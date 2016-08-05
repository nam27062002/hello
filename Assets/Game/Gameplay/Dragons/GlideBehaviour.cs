using UnityEngine;
using System.Collections;

public class GlideBehaviour : StateMachineBehaviour {

	public Range m_glidingTime = new Range(4f, 6f);
	private float m_timer = 4f;


	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		ResetTimer();
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {		
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			animator.SetBool("glide", false);
		}
	}

	public void ResetTimer() {
		m_timer = m_glidingTime.GetRandom();
	}
}
