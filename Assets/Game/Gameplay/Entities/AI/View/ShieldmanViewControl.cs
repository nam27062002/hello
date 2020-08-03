using UnityEngine;
using System.Collections;

public class ShieldmanViewControl : ViewControl {

	[SeparatorAttribute("Shield")]
	[SerializeField] private GameObject m_shield;



	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_shield.SetActive(false);
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		m_shield.SetActive(true);
	}

	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		m_shield.SetActive(false);
	}
}
