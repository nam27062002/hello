using UnityEngine;
using System.Collections;

public class SlimePinkViewControl : ViewControl {
	private static Material sm_pinkSlimeSkin = null;

	[SeparatorAttribute("Particles")]
	[SerializeField] private ParticleData m_poisonParticleData;
	[SerializeField] private Transform m_poisonParticleTransform;

	private DisableInSeconds m_poisonParticles;


	protected override void Awake() {
		base.Awake();

		if (sm_pinkSlimeSkin == null) sm_pinkSlimeSkin = new Material(Resources.Load("Game/Materials/MT_SlimePinkPoisonMode") as Material);

		SkinData skin = new SkinData();
		skin.chance = 0f;
		skin.skin = sm_pinkSlimeSkin;

		m_skins.Clear();
		m_skins.Add(skin);
	}

	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_poisonParticleData.CreatePool();
		m_poisonParticles = null;
	}

	public override void PreDisable() {
		if (m_poisonParticles != null) {
			m_poisonParticleData.ReturnInstance(m_poisonParticles.gameObject); 
			m_poisonParticles = null; 
		}
		base.PreDisable();
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		base.OnSpecialAnimationEnter(_anim);
		switch(_anim) {
			case SpecialAnims.A: {
					// equip skin
					m_skins[0].chance = 100f;
					RefreshMaterial();

					// add particles
					GameObject ps = m_poisonParticleData.Spawn(m_poisonParticleTransform, Vector3.zero, false);
					if (ps != null) {
						m_poisonParticles = ps.GetComponent<DisableInSeconds>();
						m_poisonParticles.activeTime = 0f;
					}
				} break;
		}
	}

	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		base.OnSpecialAnimationExit(_anim);
		switch(_anim) {
			case SpecialAnims.A: 
				// unequip skin
				m_skins[0].chance = 0f;
				RefreshMaterial();

				// remove particles
				if (m_poisonParticles != null) {
					m_poisonParticles.Activate();
					m_poisonParticles = null; 
				}
			break;
		}
	}
}