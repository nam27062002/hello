﻿using UnityEngine;
using System.Collections.Generic;

public class MachineEatBehaviour : EatBehaviour {

	[SerializeField] private DragonTier m_eaterTier;
    public DragonTier eaterTier { get {return m_eaterTier; } set{ m_eaterTier = value; }}

	[SerializeField] private bool m_isPet = false;

	[SerializeField] private bool m_isAquatic = false;
	protected override bool isAquatic { get { return m_isAquatic; } }

	[SerializeField] private bool m_canMultipleLatchOnPlayer = false;
	public override bool canMultipleLatchOnPlayer { get { return m_canMultipleLatchOnPlayer; } }

	[SerializeField] private float m_damageOnBitePlayer = 10;
	public float damageOnBitePlayer
	{
		get{ return m_damageOnBitePlayer; }
	}

	private IAttacker m_attacker;
	private AI.IMachine m_machine;

	[SerializeField] private bool m_ignoreBadJunk = true;

	override protected void Awake() {

		base.Awake();
		m_motion = GetComponent<IMotion>();
		m_tier = m_eaterTier;
		m_eatSpeedFactor = 0.5f;	// [AOC] HARDCODED!!
		m_canHold = false;
		m_limitEating = true;
		m_limitEatingValue = 1;
		m_holdDuration = 10;
		SetupHoldParametersForTier( IDragonData.TierToSku(m_eaterTier));

		m_machine = GetComponent<AI.IMachine>();
		m_attacker = GetComponent<IAttacker>();

		m_entity = GetComponent<IEntity>();
		if (m_entity != null) {
			m_origin = m_entity.sku;
		}

		if (m_isPet) {
			m_type = IEntity.Type.PET;
			m_canLatchOnPlayer = false;	
		} else {
			m_type = IEntity.Type.OTHER;
			m_canLatchOnPlayer = true;
			m_canBitePlayer = false;
		}

		if (m_ignoreBadJunk)
		{
			AddToIgnoreList("BadJunk");
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

		if (m_attacker != null) {
			m_attacker.StartAttackTarget(_transform);
		}
	}

	public override void StopAttackTarget()
	{
		if (m_attackTarget != null) {
			m_attacker.StopAttackTarget();
		}
		base.StopAttackTarget();
	}

	override protected void BiteKill( bool _canHold = true ) 
	{
		if ( !m_machine.IsDead() && !m_machine.IsDying() )
			base.BiteKill(_canHold);
	}



    protected override void EatExtended(PreyData preyData) {         
		if ( m_attacker != null)
		{
			// Start Eating Animation!
			m_attacker.StartEating();
		}        
	}

	protected override void UpdateEating()
	{
		base.UpdateEating();
		if (PreyCount <= 0 && m_attacker != null)
			m_attacker.StopEating();
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

	protected override float GetBitePlayerDamage()
	{
		return m_damageOnBitePlayer;
	}
}
