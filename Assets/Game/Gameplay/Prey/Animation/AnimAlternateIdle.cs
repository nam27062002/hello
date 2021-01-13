using UnityEngine;
using System.Collections;

public class AnimAlternateIdle : StateMachineBehaviour {

	[SerializeField] private string m_idleTriggerKey;
	[SerializeField] private Range m_idleTriggerTime = new Range(4f, 6f);

	private float m_timer = 4f;

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		ResetTimer();
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			animator.SetTrigger(m_idleTriggerKey);
			ResetTimer();
		}
	}

	public void ResetTimer() {
		m_timer = m_idleTriggerTime.GetRandom();
	}
}
