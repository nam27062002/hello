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

		Entity entity = GetComponent<Entity>();
		// if is pet -> m_rewardPlayer = true

		SetupHoldParametersForTier( DragonData.TierToSku( m_petTier));
	}

}
