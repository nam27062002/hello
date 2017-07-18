using UnityEngine;
using System.Collections;

public class AngelViewControl : ViewControl {

	private static int SPECIAL_HASH = Animator.StringToHash("Special");

	protected override void Awake() {
		base.Awake();
		StartEndStateMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndStateMachineBehaviour>();
		for( int i = 0; i<behaviours.Length; i++ ){
			behaviours[i].onStateExit += onStateExit;
		}
	}

	void onStateExit( int shortName )
	{
		if ( shortName == SPECIAL_HASH) 
		{
			onReviveEnd();
		}
	}

	void onReviveEnd()
	{
		// Lose Hale and Harp
		transform.FindTransformRecursive("Aura_LOW").gameObject.SetActive(false);
		transform.FindTransformRecursive("Harp_LOW").gameObject.SetActive(false);
	}
}
