using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerEatBehaviour : EatBehaviour {		

	private DragonPlayer m_dragon;
	private DragonBoostBehaviour m_dragonBoost;
	private Dictionary<string, float> m_eatingBoosts = new Dictionary<string, float>();
	private Animator m_animator;

	//--------------

	override protected void Awake()
	{
		base.Awake();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		Messenger.AddListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
		Messenger.AddListener(GameEvents.SCORE_MULTIPLIER_LOST, OnMultiplierLost);
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
	}

	private void MouthCache() 
	{
		m_mouth = transform.FindTransformRecursive("Fire_Dummy");
		m_head = transform.FindTransformRecursive("Dragon_Head");

		m_suction = transform.FindTransformRecursive("Fire_Dummy");
		if (m_suction == null) {
			m_suction = m_mouth;
		}
	}


	protected void Start() 
	{
		m_dragon = GetComponent<DragonPlayer>();
		m_dragonBoost = GetComponent<DragonBoostBehaviour>();
		m_motion = GetComponent<DragonMotion>();

		m_tier = m_dragon.data.tier;
		m_eatSpeedFactor = m_dragon.data.def.Get<float>("eatSpeedFactor");

		SetupHoldParametersForTier( m_dragon.data.tierDef.sku );
		m_rewardsPlayer = true;

		DragonAnimationEvents animEvents = GetComponentInChildren<DragonAnimationEvents>();
		if ( animEvents != null )
		{
			animEvents.onEatEvent += onEatEvent;
			m_waitJawsEvent = true;
		}
		m_waitJawsEvent = false;// not working propertly for the moment!
	}

	override protected void OnDisable()
	{
		base.OnDisable();

		if (m_animator && m_animator.isInitialized) 
		{
			m_animator.SetBool("eat", false);
		}
	}


	void onEatEvent()
	{
		OnJawsClose();
	}


	void OnDestroy() {
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_EATEN, OnEntityEaten);
		Messenger.RemoveListener(GameEvents.SCORE_MULTIPLIER_LOST, OnMultiplierLost);
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
	}


	protected override void Eat(AI.Machine _prey) {
		base.Eat( _prey );
		m_animator.SetBool("eat", true);
		if (m_eatingTime >= 0.5f || m_prey.Count > 2) 
		{
			m_animator.SetTrigger("eat crazy");
		}
	}

	protected override void UpdateEating()
	{
		base.UpdateEating();
		if ( m_prey.Count <= 0 )
			m_animator.SetBool("eat", false);	
	}


	void OnEntityEaten( Transform t, Reward reward )
	{
		// Check if origin is in power up and give proper boost
		if ( m_eatingBoosts.ContainsKey( reward.origin ) )
		{
			m_dragon.AddLife(reward.health + (reward.health * m_eatingBoosts[reward.origin] / 100.0f));
		}
		else
		{
			m_dragon.AddLife(reward.health);
		}

		m_dragon.AddEnergy(reward.energy);
	}

	void OnFuryToggled( bool toogle, DragonBreathBehaviour.Type type)
	{
		if (toogle)
			m_attackTarget = null;
	}

	void OnMultiplierLost()
	{
		if ( Random.Range(0, 100.0f) <= 60.0f )
		{
			Burp();
		}
	}

	public void AddEatingBost( string entitySku, float value )
	{
		m_eatingBoosts.Add( entitySku, value);
	}

	public override void StopAttackTarget()
	{
		if ( m_attackTarget != null )
		{
			m_animator.SetBool("eat", false);	// Stop targeting animation
		}
		base.StopAttackTarget();
	}

	override protected void BiteKill( bool _canHold = true ) 
	{
		
		base.BiteKill();
	}

	override protected void StartHold(AI.Machine _prey, bool grab = false) 
	{
		base.StartHold(_prey, grab);
		DragonMotion motion = GetComponent<DragonMotion>();
		if ( grab )
		{
			motion.StartGrabPreyMovement(m_holdingPrey, m_holdTransform);
		}
		else
		{
			motion.StartLatchMovement(m_holdingPrey, m_holdTransform);
		}

		m_animator.SetBool("eatHold", true);
	}

	protected override void UpdateHoldingPrey()
	{
		base.UpdateHoldingPrey();
		// if active boost
		if (m_dragonBoost.IsBoostActive())
		{
			// Increase eating speed
			m_animator.SetFloat("eatingSpeed", m_holdBoostDamageMultiplier / 2.0f);
		}
		else
		{
			// Change speed back
			m_animator.SetFloat("eatingSpeed", 1);
		}
	}


	override public void EndHold()
	{
		base.EndHold();
		DragonMotion motion = GetComponent<DragonMotion>();
		if ( m_grabbingPrey )
		{
			motion.EndGrabMovement();
		}
		else
		{
			motion.EndLatchMovement();
		}

		// Check if boosting!!
		if (m_dragonBoost.IsBoostActive())
		{
			m_animator.SetFloat("eatingSpeed", m_dragonBoost.boostMultiplier);
		}
		else
		{
			m_animator.SetFloat("eatingSpeed", 1);
		}

		m_animator.SetBool("eatHold", false);
	}

	override public bool IsBoosting(){
	 	return m_dragonBoost.IsBoostActive();
	 }


	protected override float GetEatDistance()
	{
		float ret = m_eatDistance * transform.localScale.x;
		if ( m_dragonBoost.IsBoostActive() )
			ret *= 2;
		if (DebugSettings.eatDistancePowerUp) {
			ret *= 2;
		}
		return ret;
	}


	public override void StartAttackTarget (Transform _transform)
	{
		base.StartAttackTarget (_transform);
		// Start attack animation
		m_animator.SetBool("eat", true);
	}

	public override void PauseEating()
	{
		base.PauseEating();
		m_animator.SetBool("eat", false);
	}
}