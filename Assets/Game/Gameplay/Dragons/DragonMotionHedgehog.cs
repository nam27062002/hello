using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonMotionHedgehog : DragonMotion {

	// Extra_1 - Charging
	// Extra_2 - Ricocheting?
	protected Vector3 m_ricochetDir = Vector3.zero;
    public float m_sonicMaxSpeed = 5;
	public float m_sonicMinSpeed = 5;
    protected Vector3 m_sonicImpulse;
    public float m_maxRotationSpeed = 720;
    public float m_minRotationSpeed = 720;
    protected float m_currentRotationSpeed = 0;
	bool m_cheskStateForResume = true;
    public Transform m_rotationPivot;
    protected Quaternion m_idleQuaternion;
    protected Quaternion m_currentQuaternion;
    private int m_powerLevel = 0;
    public float m_dpadDistance = 500;
    private float m_currentDpadDistance = 0;

    protected float m_boostLevelStart = 0;

    [ Header("Custom Sounds") ]
    public string m_rollUpSound;
    public string m_shootSound;
    public string m_bounceSound;
    
    [ Header("Particles") ]
    public string m_fireParticleImpact = "FireCircle/PS_SonicDragonFireImpact";
    private ParticleSystem m_fireParticleImpactInstance;
    
    public string m_megaFireParticleImpact = "FireCircle/PS_SonicDragonMegaFireImpact";
    private ParticleSystem m_megaFireParticleImpactInstance;


    DragonBreathBehaviour m_breath;
    DragonHedgehogPowers m_powers;

    Vector3 m_sphereLocalPosition;

    public float m_sonicControlSpeed = 1f;
    
    override public float absoluteMaxSpeed
    {
        get
        {
            return Mathf.Max(base.absoluteMaxSpeed,m_sonicMaxSpeed);
        }
    }
    

	override protected void Start() {
		base.Start();
        m_sphereLocalPosition = m_mainGroundCollider.transform.localPosition;
		m_boost.alwaysDrain = true;
        m_idleQuaternion = m_rotationPivot.localRotation;
        m_currentQuaternion = m_idleQuaternion;

        m_powers = GetComponentInChildren<DragonHedgehogPowers>();
        
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.m_powerLevel;
        m_canSpin = false;
			// Wait for boost config to end
		StartCoroutine( DelayedBoostSet());

        m_breath = GetComponent<DragonBreathBehaviour>();
        m_fireParticleImpactInstance = ParticleManager.InitLeveledParticle( m_fireParticleImpact, transform.parent);
        SceneManager.MoveGameObjectToScene(m_fireParticleImpactInstance.gameObject, gameObject.scene);
        m_megaFireParticleImpactInstance = ParticleManager.InitLeveledParticle( m_megaFireParticleImpact, transform.parent);
        SceneManager.MoveGameObjectToScene(m_megaFireParticleImpactInstance.gameObject, gameObject.scene);
    }

	IEnumerator DelayedBoostSet()
	{
		yield return new WaitForSeconds(1.0f);
		m_boost.energyRequiredToBoost = m_dragon.energyMax;
        m_boost.energyRequiredToBoost = 0;
	}

	override protected void Update()
	{
		switch( m_state )
		{
            // Charging Ricochet
			case State.Extra_1: 
			{
				if ( !m_boost.IsBoostActive() )
				{
                    m_direction.Normalize();
					ChangeState(State.Extra_2);
				}
				else
				{
					Vector3 impulse = GameConstants.Vector3.zero;
					m_controls.GetImpulse(1, ref impulse);
					if ( impulse != GameConstants.Vector3.zero )
						m_direction = impulse;
				}
                    
			}break;
            // Bouncing!
			case State.Extra_2:
			{
				if (m_dragon.energy >= m_dragon.energyMax || m_controls.actionTap || m_controls.movingTap)
				{
					// End Ricocheting!
					if ( m_insideWater )
					{
						ChangeState(State.InsideWater);
					}
					else if ( m_outterSpace ){
						StartSpaceMovement();
					}
					else
					{
						ChangeState(State.Idle);
					}
				}
                else if (m_transform.position.y > FlightCeiling)
    			{
                    ModifyImpulse();
    				if ( m_impulse.y > 0 )
    				{
                        CustomBounce(m_transform.position, GameConstants.Vector3.down);
    				}    
    			}
                else 
                {
                    ModifyImpulse();
                }
				
			}break;
		}        
        
        if ( m_state == State.Extra_1 )
        {
            m_currentDpadDistance += m_dpadDistance * Time.deltaTime * 2;
        }
        else
        {
            m_currentDpadDistance -= m_dpadDistance * Time.deltaTime * 10;
        }
        m_currentDpadDistance = Mathf.Clamp(m_currentDpadDistance, 0, m_dpadDistance);
        m_controls.SetArrowDistance( m_currentDpadDistance );

		if (   m_state == State.Idle 
			|| m_state == State.Fly
			|| m_state == State.Fly_Down
			|| m_state == State.InsideWater
			|| m_state == State.ExitingWater
			|| m_state == State.OuterSpace
			|| m_state == State.ExitingSpace
		)
		{
			if ( m_boost.IsBoostActive() )
			{
				ChangeState(State.Extra_1);
			}
		}
		base.Update();
	}
    
    protected void ModifyImpulse()
    {
        Vector3 impulse = GameConstants.Vector3.zero;
        m_controls.GetImpulse(1, ref impulse);
        
        if ( impulse != GameConstants.Vector3.zero )
        {
            m_direction = Vector3.Slerp(m_direction, impulse, Time.deltaTime * m_sonicControlSpeed);
            m_sonicImpulse = Vector3.Slerp(m_sonicImpulse, impulse * m_sonicImpulse.magnitude, Time.deltaTime * m_sonicControlSpeed);
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if ( m_state == State.Extra_1 || m_state == State.Extra_2 )
        {
            if ( m_state == State.Extra_1 )
            {
                float rotationSpeed = (m_boostLevelStart - m_dragon.energy) / m_dragon.energyMax;
                m_currentRotationSpeed = Mathf.Lerp(m_minRotationSpeed, m_maxRotationSpeed, rotationSpeed);
            }
                
            // Rotate view
            m_currentQuaternion *= Quaternion.Euler(GameConstants.Vector3.back * m_currentRotationSpeed * Time.deltaTime);
        }
        else
        {
            m_currentRotationSpeed = 0;
            // Move rotation to idle
            m_currentQuaternion = Quaternion.Lerp(m_currentQuaternion, m_idleQuaternion, Time.deltaTime * 10);    
        }
        m_rotationPivot.localRotation = m_currentQuaternion;
        
    }


	override protected void FixedUpdate() {
		float _deltaTime = Time.fixedDeltaTime;
		switch( m_state )
		{
			case State.Extra_1:
			{
				float impulseMag = m_impulse.magnitude;
                m_impulse += -(m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime * 0.1f);
				ApplyExternalForce();
                if (m_impulse.sqrMagnitude > m_sonicMaxSpeed * m_sonicMaxSpeed)
                    m_impulse = m_impulse.normalized * m_sonicMaxSpeed;
				m_rbody.velocity = m_impulse;
                RotateToDirection( m_direction );
                
			}break;
			case State.Extra_2:
			{
                if ( m_transform.position.y > SpaceStart )
                {
                    // Add Gravity
                    m_sonicImpulse += GameConstants.Vector3.down * 9.81f * m_dragonAirGravityModifier * _deltaTime;
                }
                m_impulse = m_sonicImpulse;
				// ApplyExternalForce();    
                // ignore external forces
                m_externalForce = GameConstants.Vector3.zero;
				m_rbody.velocity = m_impulse;
				RotateToDirection( m_direction );
			}break;
		}
		base.FixedUpdate();
	}


	override protected void ChangeState(State _nextState) {
		if (m_state == _nextState)
			return;

		// No stun if in extra mode
		if ( m_state == State.Extra_1 || m_state == State.Extra_2 )
		{
			if ( _nextState == State.Stunned )
				return;
		}

		switch( m_state )
		{
			case State.Extra_1:
			{
                m_boost.energyRequiredToBoost = m_dragon.energyMax * m_dragon.data.energyRequiredToBoost;
                m_animationEventController.allowHitAnimation = true;
                m_cheskStateForResume = false;
				m_dragon.TryResumeEating();
                m_cheskStateForResume = true;
				m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , false);
                // m_mainGroundCollider.transform.localPosition = m_sphereLocalPosition;
                InstanceManager.timeScaleController.m_ignoreHitStops = false;
			}break;
			case State.Extra_2:
			{
                m_boost.energyRequiredToBoost = m_dragon.energyMax * m_dragon.data.energyRequiredToBoost;
                m_animationEventController.allowHitAnimation = true;
				m_cheskStateForResume = false;
				m_dragon.TryResumeEating();
				m_cheskStateForResume = true;
                m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , false);
                if ( m_powerLevel >= 2 )
                    m_impulse = GameConstants.Vector3.zero;
                    
                // m_mainGroundCollider.transform.localPosition = m_sphereLocalPosition;
                InstanceManager.timeScaleController.m_ignoreHitStops = false;
			}break;
		}
		base.ChangeState( _nextState );
		switch( m_state )
		{
			case State.Extra_1:
			{
                m_boost.energyRequiredToBoost = m_dragon.energyMax;
        
                m_boostLevelStart = m_dragon.energy;
                m_animationEventController.allowHitAnimation = false;
				m_dragon.PauseEating();
				m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , true);
                
                if ( !string.IsNullOrEmpty(m_rollUpSound) )
                {
                     AudioController.Play( m_rollUpSound, m_transform );
                }
                // m_mainGroundCollider.transform.position = m_rotationPivot.position;
                InstanceManager.timeScaleController.m_ignoreHitStops = true;
			}break;
			case State.Extra_2:
			{
                m_boost.energyRequiredToBoost = m_dragon.energyMax;
            
                // m_sonicImpulse = m_direction * m_sonicSpeed;
                float impulseSpeed = (m_boostLevelStart - m_dragon.energy) / m_dragon.energyMax;
                if ( impulseSpeed > 0.1f && m_powerLevel >= 2)
                {
                    // Tell powers to throw spikes at the end
                    m_powers.shootLevel2Spikes = true;
                }
                impulseSpeed = Mathf.Lerp(m_sonicMinSpeed, m_sonicMaxSpeed, impulseSpeed);
                m_sonicImpulse = m_direction * impulseSpeed;
                m_animationEventController.allowHitAnimation = false;
                m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , true);
				m_dragon.PauseEating();
                if ( !string.IsNullOrEmpty(m_shootSound) )
                {
                     AudioController.Play( m_shootSound, m_transform );
                }
                // m_mainGroundCollider.transform.position = m_rotationPivot.position;
                InstanceManager.timeScaleController.m_ignoreHitStops = true;
			}break;
		}
	}

    protected override void OnTriggerEnter(Collider _other)
    {
        if ( m_state == State.Extra_2 && m_powerLevel >= 1 && ((1<<_other.gameObject.layer) & GameConstants.Layers.MINES) > 0)
        {
            // Bounce if mine
            if ( _other.attachedRigidbody != null )
            {
                Entity entity = _other.attachedRigidbody.GetComponent<Entity>();
                if ( entity.HasTag(IEntity.Tag.Mine) )
                {
                    Vector3 vec = m_transform.position - _other.attachedRigidbody.position;
                    Vector3 normal = vec.normalized;
                    CustomBounce( m_transform.position + (vec * 0.5f) , normal );
                }
            }
            base.OnTriggerEnter( _other );
        }
        else
        {
            base.OnTriggerEnter( _other );
        }
    }

    override protected void CustomOnCollisionEnter(Collider _collider, Vector3 _normal, Vector3 _point)
    {
        base.CustomOnCollisionEnter( _collider, _normal, _point );
        OnHedgehogCollision( _collider, _normal, _point );
    }
    
    public override void OnCollisionStay(Collision collision)
    {
        base.OnCollisionStay(collision);
        OnHedgehogCollision( collision.collider, collision.contacts[0].normal, collision.contacts[0].point );
    }

    protected void OnHedgehogCollision(Collider _collider, Vector3 _normal, Vector3 _point)
    {
        if ( m_state == State.Extra_2 && Vector3.Dot( _normal, m_impulse) < 0)
        {
            if ( _collider.gameObject.layer != GameConstants.Layers.OBSTACLE_INDEX)
            {
                IEntity entity = _collider.gameObject.GetComponent<IEntity>();
                if ( entity == null )
                    CustomBounce(_point, _normal);
            }
            else
            {
                BreakableBehaviour breakableBehaviour = _collider.gameObject.GetComponent<BreakableBehaviour>();
                if( breakableBehaviour != null )
                {
                    if ( breakableBehaviour.unbreakableBlocker || m_dragon.GetTierWhenBreaking() < breakableBehaviour.tierWithTurboBreak )
                    {
                        // if I cannot breake it then bounce
                        CustomBounce(_point, _normal);        
                    }
                }
                else
                {
                    CustomBounce(_point, _normal);
                }
            }
        }
    }



    override public bool CanIResumeEating()
	{
		bool ret = base.CanIResumeEating();
		if ( m_cheskStateForResume && (m_state == State.Extra_1 || m_state == State.Extra_2) )
			ret = false;
		return ret;
	}

	override public bool IsBreakingMovement()
	{
		bool ret = base.IsBreakingMovement();
		if ( m_state == State.Extra_2 ) 
			ret = true;
		return ret;
	}

	override protected void CheckOutterSpace()
	{
		if ( m_state != State.Extra_1 && m_state != State.Extra_2 )
		{
			base.CheckOutterSpace();
		}
	}
    
    override protected bool CanChangeStateToInsideWater()
    {
        bool ret = false;
        if (m_state != State.Extra_1 && m_state != State.Extra_2)
            ret = base.CanChangeStateToInsideWater();
        return ret;
    }
    
    override protected bool CanChangeStateToExitWater()
    {
        bool ret = false;
        if (m_state != State.Extra_1 && m_state != State.Extra_2)
            ret = base.CanChangeStateToExitWater();
        return ret;
    }
    
    
    public override void AddForce(Vector3 _force, bool isDamage = true) {
        if ( m_dragon.IsInvulnerable() )
            return;
        if ( isDamage && m_state != State.Extra_1 && m_state != State.Extra_2 )
        {
            m_animator.SetTrigger(GameConstants.Animator.DAMAGE);
        }
        m_impulse = _force;
        if ( IsAliveState() )
            ChangeState(State.Stunned);
    }

    protected void CustomBounce(  Vector3 _position,  Vector3 normal )
    {
        if ( m_breath.IsFuryOn() )
        {
            switch( m_breath.type )
            {
                case DragonBreathBehaviour.Type.Standard:
                {
                    m_fireParticleImpactInstance.gameObject.SetActive(true);
                    m_fireParticleImpactInstance.transform.position = _position;
                    m_fireParticleImpactInstance.Play();
                }break;
                case DragonBreathBehaviour.Type.Mega:
                {
                    m_megaFireParticleImpactInstance.gameObject.SetActive(true);
                    m_megaFireParticleImpactInstance.transform.position = _position;
                    m_megaFireParticleImpactInstance.Play();
                }break;
            }
        }
    
        if ( !string.IsNullOrEmpty(m_bounceSound))
        {
            AudioController.Play( m_bounceSound, m_transform );
        }
        m_direction = Vector3.Reflect( m_direction,  normal);
        m_direction.Normalize();
        // m_sonicImpulse = Vector3.Reflect( m_sonicImpulse,  normal);
        m_sonicImpulse = m_direction * Mathf.Min(m_sonicImpulse.magnitude, m_sonicMaxSpeed);
        m_impulse = m_sonicImpulse;
        // Increase multiplier
        Messenger.Broadcast(MessengerEvents.SCORE_MULTIPLIER_FORCE_UP);
    }

}
