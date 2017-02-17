using UnityEngine;
using System.Collections;

public class SpartakusViewControl : ViewControl {

	[SeparatorAttribute("Dizzy")]
	[SerializeField] private GameObject m_stars;



	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_stars.SetActive(false);
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
			case SpecialAnims.B: m_stars.SetActive(false); break;
		}
	}
}
