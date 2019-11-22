using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonEatBehaviour : EatBehaviour {		

	private DragonPlayer m_dragon;
	private DragonBoostBehaviour m_dragonBoost;
	private DragonHealthBehaviour m_dragonHealth;
	private DragonBreathBehaviour m_dragonBreath;
	private Animator m_animator;
    private float m_eatingSpeed = -1;
    public Range m_randomSpeedRange = new Range(1.0f, 1.5f);
	private float m_randomSpeed = 1;
	private const float m_boostEatingSpeed = 1.5f;
	protected float m_powerUpEatPercentage = 0;
	protected float m_powerUpEatDistance = 0;

	protected float m_sizeUpEatSpeedFactor = 1;
	public float sizeUpEatSpeedFactor
	{
		get { return m_sizeUpEatSpeedFactor; }
		set { m_sizeUpEatSpeedFactor = value; }
	}

	protected List<string> m_immuneTrash = new List<string>();
	private DragonMotion m_dragonMotion;

    //--------------

    override protected void Awake()
	{
		base.Awake();
		m_animator = transform.Find("view").GetComponent<Animator>();
		m_animator.logWarnings = false;
		m_type = IEntity.Type.PLAYER;
	}


	override protected void Start() 
	{
        base.Start();
		m_dragon = GetComponent<DragonPlayer>();
		m_dragonBoost = m_dragon.dragonBoostBehaviour;
		m_dragonHealth = m_dragon.dragonHealthBehaviour;
		m_dragonBreath = m_dragon.breathBehaviour;
		m_dragonMotion = GetComponent<DragonMotion>();
		m_motion = m_dragonMotion;
		m_origin = "player";

		m_tier = m_dragon.data.tier;
		//TONI
		//m_eatSpeedFactor = m_dragon.data.def.GetAsFloat("eatSpeedFactor");
		m_eatSpeedFactor = m_dragon.data.maxEatSpeedFactor;

		SetupHoldParametersForTier( m_dragon.data.tierDef.sku );
		m_holdStunTime = 0;
		m_rewardsPlayer = true;

		DragonAnimationEvents animEvents = GetComponentInChildren<DragonAnimationEvents>();
		if ( animEvents != null )
		{
			animEvents.onEatEvent += onEatEvent;
			animEvents.onEatEndEvent += onEatEndEvent;
			m_waitJawsEvent = true;
		}

		Messenger.AddListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEntityEaten);
		Messenger.AddListener(MessengerEvents.SCORE_MULTIPLIER_LOST, OnMultiplierLost);
		// m_waitJawsEvent = false;// not working propertly for the moment!
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEntityEaten);
		Messenger.RemoveListener(MessengerEvents.SCORE_MULTIPLIER_LOST, OnMultiplierLost);
	}

	override protected void OnDisable()
	{
		base.OnDisable();

		if (m_animator && m_animator.isInitialized) 
		{
			m_animator.SetBool( GameConstants.Animator.EAT , false);
		}
	}

	override protected void Update()
	{
		base.Update();

		float eatSpeed = 1;
		if (m_dragonBoost.IsBoostActive())
		{
			eatSpeed = m_boostEatingSpeed;
			/*
			if (m_holdingPrey != null)
			{
				eatSpeed = m_holdBoostDamageMultiplier / 2.0f;
			}
			else
			{
		
			}
			*/
		}
		eatSpeed *= m_randomSpeed;

		if (m_eatingSpeed != eatSpeed){
			m_eatingSpeed = eatSpeed;
			m_animator.SetFloat( GameConstants.Animator.EAT_SPEED , m_eatingSpeed);
		}
	}


	void onEatEvent()
	{
		OnJawsClose();
	}

	void onEatEndEvent(){
		m_randomSpeed = m_randomSpeedRange.GetRandom();
	}

	protected override float GetEatSpeedFactor()
	{
		return m_eatSpeedFactor * sizeUpEatSpeedFactor;
	}

    protected override void EatExtended(PreyData preyData)
    {        		
        m_animator.SetBool( GameConstants.Animator.EAT , true);        
        if ((preyData != null && preyData.eatingAnimationDuration >= 0.5f) ||
            PreyCount > 2)
        {                        
			m_animator.SetTrigger( GameConstants.Animator.EAT_CRAZY );            
        }             
	}

	protected override void UpdateEating() {
		base.UpdateEating();
		if (PreyCount <= 0 && m_attackTarget == null)
			m_animator.SetBool( GameConstants.Animator.EAT, false);	
	}

	void OnEntityEaten(Transform _t, IEntity _e, Reward _reward, KillType _type) {

        if (_type == KillType.EATEN)
        {
            if (_reward.health >= 0)
            {
                m_dragon.AddLife(m_dragonHealth.GetBoostedHp(_reward.origin, _reward.health), DamageType.NONE, _t);
            }
            else
            {
                if (!m_immuneTrash.Contains(_reward.origin))
                {
                    m_dragonHealth.ReceiveDamage(Mathf.Abs(_reward.health), DamageType.NORMAL, _t, true);
                }
            }
            m_dragon.AddEnergy(_reward.energy);
        }
	}

	void OnMultiplierLost()
	{
		if ( Random.Range(0, 100.0f) <= 60.0f )
		{
			Burp();
		}
	}



	public override void StopAttackTarget()
	{
		if ( m_attackTarget != null && PreyCount <= 0)
		{
			m_animator.SetBool( GameConstants.Animator.EAT , false);	// Stop targeting animation
		}
		base.StopAttackTarget();
	}

	override public void StartHold(AI.IMachine _prey, bool grab = false) 
	{
		base.StartHold(_prey, grab);
		if ( grab )
		{
			m_dragonMotion.StartGrabPreyMovement(m_holdingPrey, m_holdTransform);
		}
		else
		{
			m_dragonMotion.StartLatchMovement(m_holdingPrey, m_holdTransform);
		}

		m_animator.SetBool( GameConstants.Animator.EAT_HOLD , true);
	}


	override protected void UpdateHoldingPrey()
	{
		m_dragon.AddLife( m_holdHealthGainRate * Time.deltaTime, DamageType.NONE, m_holdingPrey.transform );
		base.UpdateHoldingPrey();
	}

	override public void EndHold()
	{
		if ( m_grabbingPrey )
		{
			m_dragonMotion.EndGrabMovement();
		}
		else
		{
			m_dragonMotion.EndLatchMovement();
		}
		m_animator.SetBool( GameConstants.Animator.EAT_HOLD, false);        
		base.EndHold();
	}

	override public bool IsBoosting(){
	 	return m_dragonBoost.IsBoostActive();
	 }


	protected override float GetEatDistance()
	{
		float ret = m_eatDistance * transform.localScale.x;
		ret += m_powerUpEatDistance;
		return ret;
	}    

	public void AddEatDistance(float percentage)
	{
		m_powerUpEatPercentage += percentage;
		m_powerUpEatDistance = (m_eatDistance * transform.localScale.x) * m_powerUpEatPercentage / 100.0f;
	}

	public void AddImmuneTrash( string sku )
	{
		m_immuneTrash.Add(sku);
	}



	public override void StartAttackTarget (Transform _transform)
	{
		base.StartAttackTarget (_transform);
		// Start attack animation
		m_animator.SetBool( GameConstants.Animator.EAT, true);
		m_animator.SetTrigger( GameConstants.Animator.EAT_CRAZY);
	}

	public override void PauseEating()
	{
		base.PauseEating();
		m_animator.SetBool( GameConstants.Animator.EAT, false);
	}
}