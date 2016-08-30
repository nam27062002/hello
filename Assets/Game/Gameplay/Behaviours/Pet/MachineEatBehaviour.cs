using UnityEngine;
using System.Collections.Generic;

public class MachineEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_petTier;

	protected void Start() {
		m_motion = GetComponent<PreyMotion>();
		m_tier = m_petTier;
		m_eatSpeedFactor = 0.5f;	// [AOC] HARDCODED!!
		m_canHold = false;
		m_limitEating = true;
		m_limitEatingValue = 1;
		m_isPlayer = false;

		SetupHoldParametersForTier( DragonData.TierToSku( m_petTier));
	}

}
