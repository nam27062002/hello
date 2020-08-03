using UnityEngine;
using System.Collections;

public class BakerWomanViewControl : ViewControl {

	[SeparatorAttribute("Baker Woman Flour")]
	[SerializeField] private ParticleData m_flourParticles;
	[SerializeField] private Transform m_flourSpawnTransform;

	protected override void Awake() {
		base.Awake();

		m_flourParticles.CreatePool();
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		//spawn flour object, particle system	
		if (m_moving) {
			m_flourParticles.Spawn(m_flourSpawnTransform.position);
		}			
	}
}
