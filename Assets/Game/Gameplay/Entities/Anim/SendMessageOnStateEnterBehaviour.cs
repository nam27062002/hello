using UnityEngine;
using System.Collections;

public class SendMessageOnStateEnterBehaviour : StateMachineBehaviour {

	public string m_messageName = ""; 

	override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, animatorStateInfo,layerIndex);
		animator.SendMessage( m_messageName );
	}
}
