using UnityEngine;
using System.Collections;

public class SpartakusViewControl : ViewControl {

	[SeparatorAttribute("Dizzy")]
	[SerializeField] private GameObject m_stars;

	[SeparatorAttribute("Spartakus Audios")]

	[SerializeField] private string m_onJumpAttackAudio;
	private AudioObject m_onJumpAttackAudioAO;

	[SerializeField] private string m_onReceptionAudio;
	private AudioObject m_onReceptionAudioAO;

	[SerializeField] private string m_onDizzyAudio;
	private AudioObject m_onDizzyAudioAO;

	private float m_timer;

	protected override void Awake()
	{
		base.Awake();
		SpartakusAnimationEvents spartakusAnimEvents = transform.FindComponentRecursive<SpartakusAnimationEvents>();
		if (spartakusAnimEvents != null) {
			spartakusAnimEvents.onJumpImpulse += onJumpImpulse;
			spartakusAnimEvents.onJumpReception += onJumpReception;
		}
	}


	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		if ( m_stars )
			m_stars.SetActive(false);
		m_timer = 0f;
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B:{
				if ( !string.IsNullOrEmpty( m_onDizzyAudio ) )
				{
					m_onDizzyAudioAO = AudioController.Play( m_onDizzyAudio, transform );
					if ( m_onDizzyAudioAO != null )
						m_onDizzyAudioAO.completelyPlayedDelegate = OnDizzyFinished;
				}
				if ( m_stars != null )
			 		m_stars.SetActive(true); 
			 }break;	// Dizzy start
		}
	}

	void OnDizzyFinished(AudioObject ao)
	{
		RemoveAudioParent( ref m_onDizzyAudioAO);
	}

	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: m_timer = 2.5f; break;		
		}
	}

	protected override void RemoveAudios()
	{
		base.RemoveAudios();
		if ( ApplicationManager.IsAlive )
    	{
			RemoveAudioParent( ref m_onJumpAttackAudioAO );
			RemoveAudioParent( ref m_onReceptionAudioAO );
			RemoveAudioParent( ref m_onDizzyAudioAO );
    	}
	}

	protected void onJumpImpulse()
	{
		if ( !string.IsNullOrEmpty(m_onJumpAttackAudio) )
		{
			m_onJumpAttackAudioAO = AudioController.Play( m_onJumpAttackAudio, transform );
		}
	}

	protected void onJumpReception()
	{
		if ( !string.IsNullOrEmpty(m_onReceptionAudio) )
		{
			m_onReceptionAudioAO = AudioController.Play( m_onReceptionAudio, transform );
		}
	}

	public override void CustomUpdate() {
		base.CustomUpdate();

		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				if ( m_stars != null )
					m_stars.SetActive(false);
				// Dizzy end
				RemoveAudioParent( ref m_onDizzyAudioAO );
			}
		}
	}
}
