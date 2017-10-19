﻿using UnityEngine;
using System.Collections;

public class KamikazeViewControl : ViewControl {

	[SeparatorAttribute("Particles")]
	[SerializeField] private ParticleData m_jetParticleData;
	[SerializeField] private Transform m_jetParticleTransform;
	[SerializeField] private string m_dashAudio = "";
	private AudioObject m_dashAudioGO;

	private GameObject m_jetParticles;


	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_jetParticleData.CreatePool();
		m_jetParticles = null;
	}

	public override void PreDisable() {
		if (m_jetParticles != null) {
			m_jetParticleData.ReturnInstance(m_jetParticles); 
			m_jetParticles = null; 
		}
		base.PreDisable();
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		base.OnSpecialAnimationEnter(_anim);
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: 
				m_jetParticles = m_jetParticleData.Spawn(m_jetParticleTransform);
				if (!string.IsNullOrEmpty( m_dashAudio )){
					m_dashAudioGO = AudioController.Play( m_dashAudio, transform);
				}
				break;
		}
	}

	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		base.OnSpecialAnimationExit(_anim);
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: 
				if (m_jetParticles != null) {
					m_jetParticleData.ReturnInstance(m_jetParticles); 
					m_jetParticles = null; 
				}
			break;
		}
	}

	protected override void RemoveAudios()
    {
    	base.RemoveAudios();
		if ( ApplicationManager.IsAlive )
    	{
			RemoveAudioParent( ref m_dashAudioGO );
		}
    }
}
