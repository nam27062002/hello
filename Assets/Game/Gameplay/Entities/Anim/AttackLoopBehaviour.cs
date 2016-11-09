using UnityEngine;
using System.Collections;

public class AttackLoopBehaviour : StateMachineBehaviour {

	public string m_attackEndState = "Attack End";

	override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateExit(animator, animatorStateInfo,layerIndex);
		if (!animator.GetNextAnimatorStateInfo(layerIndex).IsName( m_attackEndState))
		{
			// if we dont do attack end animation we force "AttackEnd" event
			animator.SendMessage("AttackEnd");
		}

	}
}
