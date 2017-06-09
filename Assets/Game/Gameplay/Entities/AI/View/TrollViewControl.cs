using UnityEngine;
using System.Collections;

public class TrollViewControl : ViewControl {

	[SeparatorAttribute("Troll Attack Sounds")]
	[SerializeField] protected string[] m_attackAudios;
	private AudioObject m_currentAttackAudio;

	protected virtual void Awake() {
		base.Awake();
		if (m_animEvents != null) {
			m_animEvents.onAttackEventId += OnAttackId;
		}
	}

	protected virtual void RemoveAudios()
    {
    	base.RemoveAudios();
		if ( ApplicationManager.IsAlive )
    	{
			RemoveAudioParent( m_currentAttackAudio );
		}
    }

	protected virtual void OnAttackId( int attackId ) {
		if (attackId < m_attackAudios.Length){
			if (!string.IsNullOrEmpty(m_attackAudios[attackId])){
				StopAttackAudio();
				m_currentAttackAudio = AudioController.Play( m_attackAudios[attackId], transform );
			}
		}
	}

	protected void StopAttackAudio()
	{
		if ( m_currentAttackAudio != null && m_currentAttackAudio.IsPlaying() )
			m_currentAttackAudio.Stop();
	}
	
}
