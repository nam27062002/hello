using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewParticleSpawnerAndSound : ViewParticleSpawner {

	[SerializeField] private string m_sound;
	private AudioObject m_idleAudioAO;

	override protected void Spawn() {
		base.Spawn();
		if ( !string.IsNullOrEmpty( m_sound ) )
		{
			if ( m_idleAudioAO == null || !m_idleAudioAO.IsPlaying() )
			{
				m_idleAudioAO = AudioController.Play(m_sound, transform);
				if (m_idleAudioAO != null )
					m_idleAudioAO.completelyPlayedDelegate = OnIdleCompleted;
			}
		}
	}

	void OnIdleCompleted(AudioObject ao){
		RemoveAudioParent( ref m_idleAudioAO);
	}

	protected void RemoveAudioParent(ref AudioObject ao)
	{
		if ( ao != null && ao.transform.parent == transform )
		{
			ao.transform.parent = null;	
			ao.completelyPlayedDelegate = null;
			if ( ao.IsPlaying() && ao.audioItem.Loop != AudioItem.LoopMode.DoNotLoop )
				ao.Stop();
		}
		ao = null;
	}

	override protected void Return() {
		base.Return();
		RemoveAudioParent( ref m_idleAudioAO );
	}
	 
	override protected void ForceReturn(){
		base.ForceReturn();
		RemoveAudioParent( ref m_idleAudioAO );
	}

}
