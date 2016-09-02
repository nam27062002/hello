using UnityEngine;
using System.Collections.Generic;

public class MachineEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_eaterTier;
	public DragonTier eaterTier { get {return m_eaterTier; }}

	protected void Start() {

		m_canLatchOnPlayer = true;

		m_motion = GetComponent<AI.Machine>();
		m_tier = m_eaterTier;
		m_eatSpeedFactor = 0.5f;	// [AOC] HARDCODED!!
		m_canHold = false;
		m_limitEating = true;
		m_limitEatingValue = 1;
		m_isPlayer = false;
		m_holdDuration = 10;
		m_canEatEntities = false;
		SetupHoldParametersForTier( DragonData.TierToSku( m_eaterTier));
	}

}
