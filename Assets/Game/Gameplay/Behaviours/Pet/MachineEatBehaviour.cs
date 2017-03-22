using UnityEngine;
using System.Collections.Generic;

public class MachineEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_eaterTier;
	public DragonTier eaterTier { get {return m_eaterTier; }}

	[SerializeField] private bool m_isPet = false;

	[SerializeField] private bool m_isAquatic = false;
	protected override bool isAquatic { get { return m_isAquatic; } }

	[SerializeField] private bool m_canMultipleLatchOnPlayer = false;
	public override bool canMultipleLatchOnPlayer { get { return m_canMultipleLatchOnPlayer; } }

	private AI.MachineOld m_machine;

	override protected void Awake() {

		base.Awake();
		m_motion = GetComponent<AI.MachineOld>();
		m_tier = m_eaterTier;
		m_eatSpeedFactor = 0.5f;	// [AOC] HARDCODED!!
		m_canHold = false;
		m_limitEating = true;
		m_limitEatingValue = 1;
		m_isPlayer = false;
		m_holdDuration = 10;
		SetupHoldParametersForTier( DragonData.TierToSku( m_eaterTier));

		m_machine = GetComponent<AI.MachineOld>();
		if (m_isPet) {
			m_canLatchOnPlayer = false;	
			AddToIgnoreList("badJunk");
		} else {
			m_canLatchOnPlayer = true;
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

		m_rewardsPlayer = m_isPet;
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

	public override void StopAttackTarget()
	{
		if ( m_attackTarget != null )
		{
			m_machine.StopAttackTarget();
		}
		base.StopAttackTarget();
	}

	override protected void BiteKill( bool _canHold = true ) 
	{
		if ( !m_machine.IsDead() && !m_machine.IsDying() )
			base.BiteKill(_canHold);
	}



    protected override void EatExtended(PreyData preyData) {         
		if ( m_machine )
		{
			// Start Eating Animation!
			m_machine.StartEating();
		}        
	}

	protected override void UpdateEating()
	{
		base.UpdateEating();
		if (PreyCount <= 0 && m_machine)
			m_machine.StopEating();
	}


	// find mouth transform 
	protected override void MouthCache() 
	{
		if (m_isPet)
		{
            Transform cacheTransform = transform;
			m_mouth = cacheTransform.FindTransformRecursive("Fire_Dummy");// SuctionPoint
			m_bite = cacheTransform.FindTransformRecursive("BitePoint");
			m_swallow = cacheTransform.FindTransformRecursive("Pet_Head");// SwallowPoint
			// To remove. Just here to back compatibility with older pets
			if ( m_swallow == null )
				m_swallow = cacheTransform.FindTransformRecursive("Dragon_Head");// SwallowPoint
			// End to remove
			m_suction = cacheTransform.FindTransformRecursive("SuctionPoint");

			if ( m_bite == null )
				m_bite = m_mouth;	
			if (m_suction == null)
				m_suction = m_mouth;
		}
		else
		{
			base.MouthCache();
		}
	}

	override protected void OnDrawGizmos() {
		if ( m_motion == null )
			m_motion = GetComponent<AI.MachineOld>();
		base.OnDrawGizmos();
	}
}
