using UnityEngine;
using System.Collections.Generic;

public class DragonPetEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_petTier;

	void Start() {
		m_motion = GetComponent<PreyMotion>();
		m_tier = m_petTier;
		m_eatSpeedFactor = 0.5f;	// [AOC] HARDCODED!!
		m_canHold = false;
		m_limitEating = true;
		m_limitEatingValue = 1;
		m_isPlayer = false;
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
