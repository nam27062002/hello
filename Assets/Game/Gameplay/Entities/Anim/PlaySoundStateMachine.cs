using UnityEngine;

public class PlaySoundStateMachine : StateMachineBehaviour {

	[SerializeField] private string m_impactSound;
	[SerializeField] private float m_delay = 0f;
	private float m_timer = 0f;

	// Use this for initialization
	void Awake() {	
		m_timer = 0f;
	}
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		
		m_timer = 0f;
		if (m_delay > 0f) {
			m_timer = m_delay;
		} else {
			PlaySound( animator );
		}
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				PlaySound( animator );
			}
		}
	}

	private void PlaySound( Animator animator ){
		if ( !string.IsNullOrEmpty(m_impactSound)  ){
			AudioController.Play(m_impactSound,animator.transform.position );
		}
	}

}
