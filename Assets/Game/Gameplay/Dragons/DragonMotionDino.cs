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
    protected float m_snapHeight = 0.5f;
    

	override protected void Start() {
		base.Start();
        m_adaptHeight = m_adaptHeight * m_transform.localScale.y;
        m_snapHeight = m_snapHeight * m_transform.localScale.y;
    }


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
                    UpdateMovement(Time.fixedDeltaTime);
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
    
    protected void CustomIdleMovement( float delta )
    {
    
        CheckGround( out m_raycastHit );
        if ( m_height < m_adaptHeight )
        {
            if ( m_height < m_snapHeight )
            {
                Vector3 dir = m_lastGroundHitNormal;
                dir.z = 0;
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
                SnapToGround();
            }
            else
            {
                // if angle < falling angle and angle < max up angle rotate towards this
                // else just fall
                ComputeImpulseToZero(delta);
                FreeFall(delta, m_direction);
            }
        }
        // Free fall
        else
        {
            ComputeImpulseToZero(delta);
            FreeFall(delta, m_direction);
        }
        
        RotateToDirection(m_direction, true);
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
        }
        
        CheckGround( out m_raycastHit );
        if ( m_height < m_adaptHeight )
        {
            if ( m_height < m_snapHeight )
            {
                // TODO: Get proper gorund normal
                Vector3 dir = m_lastGroundHitNormal;
                dir.z = 0;
                if ( impulse.x < 0 )
                {
                    dir = dir.RotateXYDegrees(90);
                }
                else
                {
                    dir = dir.RotateXYDegrees(-90);
                }
                m_direction = dir;
                // Snap? Move right left
                SnapToGround();
                m_impulse = m_direction * m_walkSpeed;
            }
            else
            {
                FreeFall(delta, impulse);
            }
        }
        else
        {
            FreeFall(delta, impulse);
        }
        
        RotateToDirection(m_direction, true);
        m_desiredRotation = m_transform.rotation;
        ApplyExternalForce();
        m_rbody.velocity = m_impulse;
        
    }
    
    protected void SnapToGround()
    {
        Vector3 diff = m_transform.position - m_sensor.bottom.position;
        m_transform.position = m_lastGroundHit + diff + Vector3.up * (m_snapHeight - 0.1f);
    }

    protected void FreeFall( float _deltaTime, Vector3 direction )
    {
        Vector3 gravityAcceleration;
        gravityAcceleration.x = 0;
        gravityAcceleration.y = -9.81f * m_freeFallGravityMultiplier;
        gravityAcceleration.z = 0;
            
        // stroke's Drag
        m_impulse = m_rbody.velocity;
        float impulseMag = m_impulse.magnitude;
        m_impulse += (gravityAcceleration * _deltaTime) - ( m_impulse.normalized * m_dragonFricction * impulseMag * _deltaTime);
            
        if ( direction.x > 0 ){
            m_direction = GameConstants.Vector3.right;
        }else{
            m_direction = GameConstants.Vector3.left;
        }
        
    }

}
