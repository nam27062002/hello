using UnityEngine;
using System.Collections;

public class GlideBehaviour : StateMachineBehaviour {

	[SerializeField] private Range m_glidingTime = new Range(4f, 6f);
	private float m_timer = 4f;
	private float m_altTimer;

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		m_timer = m_glidingTime.GetRandom();
		m_altTimer = m_timer * Random.Range(0.3f, 0.6f);
	}
	
	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		animator.SetBool("glide", false);
	}

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	//override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			animator.SetBool("glide", false);
		}

		if (m_altTimer > 0) {
			m_altTimer -= Time.deltaTime;
			if (m_altTimer <= 0) {
				animator.SetTrigger("glide alt");
				m_altTimer = 0;
			}
		}
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
