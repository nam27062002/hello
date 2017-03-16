using UnityEngine;
using System.Collections;

public class BakerWomanViewControl : ViewControl {

	[SeparatorAttribute("Baker Woman Flour")]
	[SerializeField] private ParticleData m_flourParticles;
	[SerializeField] private Transform m_flourSpawnTransform;


	protected override void Awake() {
		base.Awake();

		if (m_flourParticles.IsValid()) {
			ParticleManager.CreatePool(m_flourParticles);
		}
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		//spawn flour object, particle system	
		if (m_flourParticles.IsValid()) {			
			if (m_moving) {
				ParticleManager.Spawn(m_flourParticles, m_flourSpawnTransform.position);
			}			
		}
	}
}
