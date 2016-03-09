using UnityEngine;
using System.Collections.Generic;

public class DragonPetEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_petTier;
	[SerializeField] private float m_bite;

	void Start() {
		m_motion = GetComponent<PreyMotion>();

		m_tier = m_petTier;
		m_biteSkill = m_bite;
	}

	protected override void SlowDown(bool _enable) {
		if (_enable) {
			m_motion.SetSpeedMultiplier(0.25f);
			m_slowedDown = true;
		} else {
			m_motion.SetSpeedMultiplier(1f);
			m_slowedDown = false;
		}
	}
}
