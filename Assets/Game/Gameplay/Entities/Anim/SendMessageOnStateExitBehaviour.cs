using UnityEngine;
using System.Collections;

public class SendMessageOnStateExitBehaviour : StateMachineBehaviour {

	public string m_sendMessageIfNotGoingToThisState = "Attack End";
	public string m_messageName = "AttackEnd"; 

	override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateExit(animator, animatorStateInfo,layerIndex);
		if (!animator.GetNextAnimatorStateInfo(layerIndex).IsName( m_sendMessageIfNotGoingToThisState ))
		{
			// if we dont do attack end animation we force "AttackEnd" event
			animator.SendMessage( m_messageName );
		}

	}
}
