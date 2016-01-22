using UnityEngine;
using System.Collections;

public class AnimBoostBehaviour : StateMachineBehaviour 
{
	int loopHash = Animator.StringToHash("Loop");

	void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (animatorStateInfo.shortNameHash == loopHash)
		{
			animator.gameObject.SendMessage("TurboLoopStart");			
		}
	}

	void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (animatorStateInfo.shortNameHash == loopHash)
		{
			animator.gameObject.SendMessage("TurboLoopEnd");
		}
	}
}
