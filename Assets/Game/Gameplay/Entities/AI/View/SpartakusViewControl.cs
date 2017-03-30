using UnityEngine;
using System.Collections;

public class SpartakusViewControl : ViewControl {

	[SeparatorAttribute("Dizzy")]
	[SerializeField] private GameObject m_stars;

	[SeparatorAttribute("Effects")]
	[SerializeField] private ParticleData m_slashData;


	private float m_timer;



	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		if (m_slashData.IsValid()) {
			ParticleManager.CreatePool(m_slashData);
		}

		m_stars.SetActive(false);
		m_timer = 0f;
	}

	public override void OnJumpImpulse(Vector3 _pos) {
		base.OnJumpImpulse(_pos);
		if (m_slashData.IsValid()) {
			GameObject ps = ParticleManager.Spawn(m_slashData, _pos);
			if (ps != null) {
				ps.transform.parent = transform;
			}
		}
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: m_stars.SetActive(true); break;
		}
	}

	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		switch(_anim) {
			case SpecialAnims.A: break;
			case SpecialAnims.B: m_timer = 2.5f; break;
		}
	}

	public override void CustomUpdate() {
		base.CustomUpdate();

		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_stars.SetActive(false);
			}
		}
	}
}
