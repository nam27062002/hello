using UnityEngine;
using System.Collections.Generic;

public class MachineEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_eaterTier;
	public DragonTier eaterTier { get {return m_eaterTier; }}
	public bool m_isPet = false;

	private AI.Machine m_machine;

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

		m_machine = GetComponent<AI.Machine>();
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
		// else
		{
			m_waitJawsEvent = false;
		}
	}


	public override void StartAttackTarget (Transform _transform)
	{
		base.StartAttackTarget (_transform);
		// Start attack animation
		// Tell vie to play eat event!

		if ( m_machine )
		{
			m_machine.StartAttackTarget(_transform);
		}
	}

	override protected void BiteKill( bool _canHold = true ) 
	{
		if ( m_machine )
		{
			m_machine.StopAttackTarget();	// Stop Attacking Target
		}
		if ( !m_machine.IsDead() && !m_machine.IsDying() )
			base.BiteKill();
	}



	protected override void Eat(AI.Machine _prey) {
		base.Eat( _prey );
		if ( m_machine )
		{
			// Start Eating Animation!
			m_machine.StartEating();
		}
	}

	protected override void UpdateEating()
	{
		base.UpdateEating();
		if ( m_prey.Count <= 0 && m_machine)
			m_machine.StopEating();
	}

}
