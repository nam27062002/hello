using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewParticleSpawnerAndSound : ViewParticleSpawner {

	[SerializeField] private string m_sound;
	private AudioObject m_idleAudioAO;

	override protected void SpawnInternal() {
		base.SpawnInternal();
		SpawnSound();
	}

	private void SpawnSound() {
		if (!string.IsNullOrEmpty(m_sound)) {
			if (m_idleAudioAO == null || !m_idleAudioAO.IsPlaying()) {
				m_idleAudioAO = AudioController.Play(m_sound, transform);
				if (m_idleAudioAO != null)
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
            InstanceManager.musicController.DelayedRemoveAudioParent( ao );
            /*
			ao.transform.parent = null;	// delay this?
			ao.completelyPlayedDelegate = null;
			if (ao.IsPlaying() && ao.audioItem.Loop != AudioItem.LoopMode.DoNotLoop)
				ao.Stop();
                */
		}
		ao = null;
	}

	override protected void CancelReturn() {
		base.CancelReturn();
		SpawnSound();
	}

	override protected bool StopAndReturn() {
		bool ret = base.StopAndReturn();
		RemoveAudioParent(ref m_idleAudioAO);
        return ret;
	}
	 
	override protected void ForceReturn(){
		base.ForceReturn();
		RemoveAudioParent(ref m_idleAudioAO);
	}

}
