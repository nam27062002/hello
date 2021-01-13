using UnityEngine;
using System.Collections;

public class AngelViewControl : ViewControl {

	private static int SPECIAL_HASH = Animator.StringToHash("Special");
    [SeparatorAttribute("Special Audios")]
    public string m_reviveAudio = "";

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
    
    protected override void OnSpecialAnimationEnter(SpecialAnims _anim) 
    {
        base.OnSpecialAnimationEnter(_anim);
        // Play Audio
        if ( !string.IsNullOrEmpty(m_reviveAudio) ) {
            AudioController.Play( m_reviveAudio );
        }
    }
}
