using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonMotionDino : DragonMotion {

    public float m_maxUpAngle = 45.0f;
    public float m_fallingAngle = -45.0f;
    public float m_freeFallGravityMultiplier = 1;
    public float m_walkSpeed = 2.0f;

    protected float m_adaptHeight = 2.0f;
    protected float m_snapHeight = 0.6f;
    protected float m_snapHisteresis = 0.1f;
    protected bool m_grounded = false;
    

	override protected void FixedUpdate() {
        m_closeToGround = false;
        
        switch(m_state)
        {
            case State.Idle:
            {
                CustomIdleMovement(Time.fixedDeltaTime);
                AfterFixedUpdate();
            }break;
            case State.Fly:
            case State.Fly_Down:
            {
                if ( m_boost.IsBoostActive() )
                {
                    if ( m_grounded )
                    {
                        SetGrounded(false);
                    }
                    UpdateMovement(Time.fixedDeltaTime);
                    CheckFeet();
                }
                else
                {
                    CustomUpdateMovement(Time.fixedDeltaTime);
                }
                AfterFixedUpdate();
            }break;
            default:
            {
                base.FixedUpdate();
            }break;
        }

	}
    
    // Make sure feet dont get inside collision
    protected void CheckFeet()
    {
        if ( m_state != State.Intro)
        {
            Vector3 pos = m_transform.position;
            pos.z = 0f;
            Vector3 bottomPos = m_sensor.bottom.position + Vector3.down * m_snapHeight / m_transform.localScale.y;
            bottomPos.z = 0;
            if (DebugSettings.ingameDragonMotionSafe && Physics.Linecast( m_transform.position, bottomPos, out m_raycastHit, GameConstants.Layers.GROUND_PLAYER_COLL, QueryTriggerInteraction.Ignore ))
            {
                Vector3 diff = pos - bottomPos;
                float dist = diff.magnitude;
                float dot1 = Vector3.Dot(m_raycastHit.normal, diff.normalized);
                float c1 = (dist - m_raycastHit.distance) * dot1;
                float angle = Vector3.Angle(m_raycastHit.normal, Vector3.up);
                float upDistance = Mathf.Acos(Mathf.Deg2Rad * angle) * c1;
                pos.y += upDistance;
            }
            
            m_transform.position = pos;
        }
    }

    protected void CustomIdleMovement( float delta )
    {
    
        CheckGround( out m_raycastHit );
        if ( m_height < m_adaptHeight )
        {
            if ( m_height < m_snapHeight )
            {
                Vector3 dir = m_lastGroundHitNormal;
                dir.NormalizedXY();
                if ( m_direction.x < 0 )
                {
                    dir = dir.RotateXYDegrees(90);
                }
                else
                {
                    dir = dir.RotateXYDegrees(-90);
                }
                m_direction = dir;
                m_impulse = GameConstants.Vector3.zero;
                if (!m_grounded)
                {
                    // STOMP!!
                    GroundStomp();
                }
                SnapToGround();
            }
            else
            {
                // if angle < falling angle and angle < max up angle rotate towards this
                // else just fall
                ComputeImpulseToZero(delta);
                FreeFall(delta, m_direction);
                CheckFeet();
            }
        }
        // Free fall
        else
        {
            ComputeImpulseToZero(delta);
            FreeFall(delta, m_direction);
            CheckFeet();
        }
        
        RotateToDirection(m_direction, false);
        m_desiredRotation = m_transform.rotation;
        ApplyExternalForce();
        m_rbody.velocity = m_impulse;
    }
    
    protected void CustomUpdateMovement(float delta)
    {
        // Ground movement
        Vector3 impulse = Vector3.zero;
        m_controls.GetImpulse(1, ref impulse);

        if (impulse == GameConstants.Vector3.zero)
        {
            ChangeState(State.Idle);
            impulse = m_direction;
        }
        
        CheckGround( out m_raycastHit );
        if ( m_height < m_adaptHeight )
        {
            if ( m_height < m_snapHeight )
            {
                Vector3 dir = m_lastGroundHitNormal;
                dir.NormalizedXY();
                if ( impulse.x < 0 )
                {
                    dir = dir.RotateXYDegrees(90);
                }
                else
                {
                    dir = dir.RotateXYDegrees(-90);
                }
                m_direction = dir;   
                if (!m_grounded)
                {
                    // STOMP!!
                    GroundStomp();
                }
                // Snap? Move right left
                SnapToGround();
                m_impulse = m_direction * m_walkSpeed;
            }
            else
            {
                // Adapt to angle?
                FreeFall(delta, impulse);
            }
        }
        else
        {
            FreeFall(delta, impulse);
        }
        
        RotateToDirection(m_direction, false);
        m_desiredRotation = m_transform.rotation;
        ApplyExternalForce();
        m_rbody.velocity = m_impulse;
        
    }
    
    protected void SnapToGround()
    {
        Vector3 diff = m_transform.position - m_sensor.bottom.position;
        m_transform.position = m_lastGroundHit + diff + Vector3.up * (m_snapHeight - m_snapHisteresis) / m_transform.localScale.y;
        if (!m_grounded)
        {
            SetGrounded(true);
        }
    }

    protected void FreeFall( float _deltaTime, Vector3 direction )
    {       
        // stroke's Drag
        m_impulse = m_rbody.velocity;
        float impulseMag = m_impulse.magnitude;
        m_impulse.y += -9.81f * m_freeFallGravityMultiplier * _deltaTime;
        m_impulse += -(m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime * 0.37f);
            
        if ( direction.x > 0 ){
            m_direction = GameConstants.Vector3.right;
        }else{
            m_direction = GameConstants.Vector3.left;
        }
        
        if (m_grounded)
        {
            SetGrounded(false);
        }
        
    }
    
    public void SetGrounded(bool _grounded)
    {
        m_grounded = _grounded;
        m_capVerticalRotation = !_grounded;
        m_animator.SetBool( GameConstants.Animator.GROUNDED , m_grounded);
        
        if (!m_grounded)
        {
            m_rbody.ResetCenterOfMass();
        }
        else
        {
            m_rbody.centerOfMass = m_transform.InverseTransformPoint( m_lastGroundHit + Vector3.up * (m_snapHeight - m_snapHisteresis));
        }

    }

    protected void GroundStomp()
    {
        
    }

}
