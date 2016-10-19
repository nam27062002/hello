using UnityEngine;
using System.Collections;

public class GlideBehaviour : StateMachineBehaviour {

	public Range m_glidingTime = new Range(4f, 6f);
	private float m_timer = 4f;
	private Animator m_animator;
	private FlyLoopBehaviour m_flyLoopBehaviour;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_flyLoopBehaviour == null)
			m_flyLoopBehaviour = animator.GetBehaviour<FlyLoopBehaviour>();
		ResetTimer();
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {		
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			if (m_flyLoopBehaviour)
				m_flyLoopBehaviour.ResetTimer();
			animator.SetBool("glide", false);

		}
	}

	public void ResetTimer() {
		m_timer = m_glidingTime.GetRandom();
	}
}
