﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMotionSonic : DragonMotion {

	// Extra_1 - Charging
	// Extra_2 - Ricocheting?
	protected DragonBoostBehaviour m_boost;
	protected Vector3 m_ricochetDir = Vector3.zero;
	public float m_sonicSpeed = 5;

	protected virtual void Start() {
		m_boost = GetComponent<DragonBoostBehaviour>();
	}

	override protected void Update()
	{
		switch( m_state )
		{
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
			case State.Extra_2:
			{
				if (m_dragon.energy >= m_dragon.energyMax)
				{
					// End Ricocheting!
					if ( m_insideWater )
					{
						ChangeState(State.InsideWater);
					}
					else
					{
						ChangeState(State.Idle);
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


	override protected void FixedUpdate() {
		float _deltaTime = Time.fixedDeltaTime;
		switch( m_state )
		{
			case State.Extra_1:
			{
				ComputeImpulseToZero(_deltaTime);
				ApplyExternalForce();
				m_rbody.velocity = m_impulse;
				m_rbody.angularVelocity = GameConstants.Vector3.zero;
			}break;
			case State.Extra_2:
			{
				m_impulse = m_direction * m_sonicSpeed;
				ApplyExternalForce();
				m_rbody.velocity = m_impulse;
				m_rbody.angularVelocity = GameConstants.Vector3.zero;
			}break;
		}
		base.FixedUpdate();
	}


	override protected void ChangeState(State _nextState) {
		if (m_state == _nextState)
			return;

		switch( m_state )
		{
			case State.Extra_1:
			{
				m_dragon.TryResumeEating();
				m_animator.SetBool( GameConstants.Animator.SONIC_FORM , false);
			}break;
			case State.Extra_2:
			{
				m_dragon.TryResumeEating();
			}break;
		}
		base.ChangeState( _nextState );
		switch( m_state )
		{
			case State.Extra_1:
			{
				m_dragon.PauseEating();
				m_animator.SetBool( GameConstants.Animator.SONIC_FORM , true);
			}break;
			case State.Extra_2:
			{
				m_dragon.PauseEating();
			}break;
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

	public virtual bool CanIResumeEating()
	{
		bool ret = true;
		if ( m_state == State.Extra_1 || m_state == State.Extra_2 )
			ret = false;
		return ret;
	}

	override protected void CheckOutterSpace()
	{
		if ( m_state != State.Extra_1 && m_state != State.Extra_2 )
		{
			if (!m_outterSpace && m_transform.position.y > SpaceStart){
				OnEnterSpaceEvent();
			}else if ( m_outterSpace && m_transform.position.y < SpaceStart ){
				OnExitSpaceEvent();
			}
		}
	}


}
