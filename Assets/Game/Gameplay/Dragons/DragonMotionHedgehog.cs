using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMotionHedgehog : DragonMotion {

	// Extra_1 - Charging
	// Extra_2 - Ricocheting?
	protected Vector3 m_ricochetDir = Vector3.zero;
	public float m_sonicSpeed = 5;
    public float m_maxRotationSpeed = 720;
    protected float m_currentRotationSpeed = 0;
	bool m_cheskStateForResume = true;
    public Transform m_rotationPivot;
    protected Quaternion m_idleQuaternion;
    protected Quaternion m_currentQuaternion;

	override protected void Start() {
		base.Start();
		m_boost.alwaysDrain = true;
        m_idleQuaternion = m_rotationPivot.localRotation;
        m_currentQuaternion = m_idleQuaternion;
			// Wait for boost config to end
		StartCoroutine( DelayedBoostSet());

	}

	IEnumerator DelayedBoostSet()
	{
		yield return new WaitForSeconds(1.0f);
		m_boost.energyRequiredToBoost = m_dragon.energyMax;
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
				if (m_dragon.energy >= m_dragon.energyMax)
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
				else if ( m_outterSpace )
				{
					if (m_transform.position.y > FlightCeiling)
					{
						if ( m_impulse.y > 0 )
						{
							m_direction = Vector3.Reflect( m_direction,  GameConstants.Vector3.down);
						}
					}
				}
			}break;
		}        

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

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if ( m_state == State.Extra_1 || m_state == State.Extra_2 )
        {
            if ( m_state == State.Extra_1 )
                m_currentRotationSpeed = Mathf.Lerp(m_currentRotationSpeed, m_maxRotationSpeed, Time.deltaTime * 2);
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
				ComputeImpulseToZero(_deltaTime);
				ApplyExternalForce();
				m_rbody.velocity = m_impulse;
                m_spinning = false;
                RotateToDirection( m_direction );
                
			}break;
			case State.Extra_2:
			{
				m_impulse = m_direction * m_sonicSpeed;
				ApplyExternalForce();
				m_rbody.velocity = m_impulse;
                m_spinning = false;
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
                m_animationEventController.allowHitAnimation = true;
				m_dragon.TryResumeEating();
				m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , false);
			}break;
			case State.Extra_2:
			{
                m_animationEventController.allowHitAnimation = true;
				m_cheskStateForResume = false;
				m_dragon.TryResumeEating();
				m_cheskStateForResume = true;
                m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , false);
                // if power level >= 2 then shoot spikes!!
			}break;
		}
		base.ChangeState( _nextState );
		switch( m_state )
		{
			case State.Extra_1:
			{
                m_animationEventController.allowHitAnimation = false;
				m_dragon.PauseEating();
				m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , true);
			}break;
			case State.Extra_2:
			{
                m_animationEventController.allowHitAnimation = false;
                m_animator.SetBool( GameConstants.Animator.HEDGEHOG_FORM , true);
				m_dragon.PauseEating();
			}break;
		}
	}

    protected override void OnTriggerEnter(Collider _other)
    {
        if ( m_state == State.Extra_2 && _other.gameObject.layer == GameConstants.Layers.MINES )    // Check power level > 1
        {
            // Bounce? how? we dont have a normal to reflect?
        }
        else
        {
            base.OnTriggerEnter( _other );
        }
    }

	override protected void OnCollisionEnter(Collision collision)
	{
		base.OnCollisionEnter(collision);
		if ( m_state == State.Extra_2 && Vector3.Dot( collision.contacts[0].normal, m_impulse) < 0)
		{
			m_direction = Vector3.Reflect( m_direction,  collision.contacts[0].normal);
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


}
