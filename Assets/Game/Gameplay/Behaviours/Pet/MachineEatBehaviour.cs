using UnityEngine;
using System.Collections.Generic;

public class MachineEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_eaterTier;
	public DragonTier eaterTier { get {return m_eaterTier; }}

	public bool m_isPet = false;

	override protected void Awake() {

		base.Awake();
		m_motion = GetComponent<AI.Machine>();
		m_tier = m_eaterTier;
		m_eatSpeedFactor = 0.5f;	// [AOC] HARDCODED!!
		m_canHold = false;
		m_limitEating = true;
		m_limitEatingValue = 1;
		m_isPlayer = false;
		m_holdDuration = 10;
		SetupHoldParametersForTier( DragonData.TierToSku( m_eaterTier));

		if ( m_isPet )
		{
			m_canLatchOnPlayer = false;	
			m_canEatEntities = true;
		}
		else
		{
			m_canLatchOnPlayer = true;
			m_canEatEntities = false;
		}

		// Check if view has eat event
		PreyAnimationEvents animEvents = gameObject.FindComponentRecursive<PreyAnimationEvents>();
		if ( animEvents )
		{
			animEvents.onEat += OnJawsClose;
			m_waitJawsEvent = true;
		}
		else
		{
			m_waitJawsEvent = false;
		}
	}


	public override void StartAttackTarget (Transform _transform)
	{
		base.StartAttackTarget (_transform);
		// Start attack animation
		// Tell vie to play eat event!
		// m_animator.SetBool("eat", true);
	}


}
