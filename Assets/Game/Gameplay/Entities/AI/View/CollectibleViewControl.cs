using System;
using UnityEngine;

public class CollectibleViewControl : MonoBehaviour, IViewControl {
	
	//-----------------------------------------------
	[SeparatorAttribute("Collect")]
	[SerializeField] private ParticleData m_onCollectParticle;
	[SerializeField] private string m_onCollectAudio;

	private IEntity m_entity;
	private AudioObject m_onCollectAudioAO;


    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
		m_entity = GetComponent<Entity>();

		// Preload particle
		m_onCollectParticle.CreatePool();
    }

    void OnDestroy() {
    	RemoveAudios();
    }

    public void PreDisable() {
		RemoveAudios();
    }

    private void RemoveAudios() {
		if (ApplicationManager.IsAlive) {
			RemoveAudioParent(m_onCollectAudioAO);
		}
    }

	protected void RemoveAudioParent(AudioObject ao) {
		if (ao != null && ao.transform.parent == transform) {
			ao.transform.parent = null;		
		}
	}

	public void Collect() {
		if (m_entity.isOnScreen && !string.IsNullOrEmpty(m_onCollectAudio)) {
			m_onCollectAudioAO = AudioController.Play(m_onCollectAudio, transform);
		}

		if (FeatureSettingsManager.IsDebugEnabled) {
			// If the debug settings for particles eaten is disabled then they are not spawned
			if (!Prefs.GetBoolPlayer(DebugSettings.INGAME_PARTICLES_EATEN, true))
				return;
		} 

		m_onCollectParticle.Spawn(transform.position + m_onCollectParticle.offset);
	}
}
