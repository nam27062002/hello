using UnityEngine;
using System.Collections;

public class SpartakusViewControl : ViewControl {

	[SeparatorAttribute("Dizzy")]
	[SerializeField] private GameObject m_stars;

	private float m_timer;



	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_stars.SetActive(false);
		m_timer = 0f;
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
