using UnityEngine;
using System.Collections;

public class AnimRandom : StateMachineBehaviour {

	[SerializeField] private string m_versionATriggerKey;
	[SerializeField] private string m_versionBTriggerKey;

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
		float rnd = Random.Range(0, 100);
		animator.SetBool(m_versionATriggerKey, rnd <= 50);
		animator.SetBool(m_versionBTriggerKey, rnd > 50);
	}
}
