using UnityEngine;
using System.Collections;

public class TriggerWithDelayBehaviour : StateMachineBehaviour {

	[SerializeField] private Range m_delay = new Range(2f, 10f);
	[SerializeField] private string m_trigger = "blink";
	// private bool m_blinking;
	private float m_timer;

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		// m_blinking = false;
		m_timer = m_delay.GetRandom();
	}

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
	
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			animator.SetTrigger( m_trigger );
			m_timer = m_delay.GetRandom();
		}
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
