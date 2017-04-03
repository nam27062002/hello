using UnityEngine;
using System.Collections;

public class KamikazeViewControl : ViewControl {

	[SeparatorAttribute("Particles")]
	[SerializeField] private ParticleData m_jetParticleData;
	[SerializeField] private Transform m_jetParticleTransform;

	private GameObject m_jetParticles;


	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		if (m_jetParticleData.IsValid()) {
			ParticleManager.CreatePool(m_jetParticleData.name, m_jetParticleData.path);
		}

		m_jetParticles = null;
	}

	public override void PreDisable() {
		if (m_jetParticles != null) {
			ParticleManager.ReturnInstance(m_jetParticles); 
			m_jetParticles = null; 
		}
		base.PreDisable();
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: 
				m_jetParticles = ParticleManager.Spawn(m_jetParticleData, Vector3.zero);
				if (m_jetParticles != null) {
					m_jetParticles.transform.SetParent(m_jetParticleTransform, false);
					m_jetParticles.transform.localPosition = Vector3.zero;
					m_jetParticles.transform.localRotation = Quaternion.identity;
				}
				break;
		}
	}

	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: 
				if (m_jetParticles != null) {
					ParticleManager.ReturnInstance(m_jetParticles); 
					m_jetParticles = null; 
				}
				break;
		}
	}
}
