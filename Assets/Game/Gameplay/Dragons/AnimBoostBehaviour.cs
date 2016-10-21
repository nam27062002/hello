using UnityEngine;
using System.Collections;

public class AnimBoostBehaviour : StateMachineBehaviour 
{
	int loopHash = Animator.StringToHash("Loop");

	override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, animatorStateInfo,layerIndex);
		if (animatorStateInfo.shortNameHash == loopHash)
		{
			animator.gameObject.SendMessage("TurboLoopStart");			
		}
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateExit(animator, animatorStateInfo,layerIndex);
		if (animatorStateInfo.shortNameHash == loopHash)
		{
			animator.gameObject.SendMessage("TurboLoopEnd");
		}
	}
}
