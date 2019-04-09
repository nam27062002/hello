using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonMotionDino : DragonMotion {

    public float m_maxUpAngle = 45.0f;
    public float m_fallingAngle = -45.0f;
    public float m_freeFallGravityMultiplier = 1;
    public float m_freeFallFriction = 0.5f;
    public float m_walkSpeed = 2.0f;

    public float m_adaptHeight = 2.0f;
    public float m_snapHeight = 1.6f;
    public float m_snapHisteresis = 0.8f;
    public bool m_grounded = false;

    
    [Range(0,100.0f)]
    public float m_speedPercentageToKill = 1;
    [Header("Kill Area")]
    public float m_killArea = 2;
    public float m_level2KillArea = 3;
    [Header("Stun Area")]
    public float m_stunArea = 4;
    public float m_level2StunArea = 3;
    public float m_stunDuration = 2;
    
    [Header("Modified on run")]
    public float m_currentKillArea = 2;
    public float m_currentStunArea = 2;
    protected int m_powerLevel = 1;
    protected float m_speedToKill;
    
    private Entity[] m_checkEntities = new Entity[50];
    private int m_numCheckEntities = 0;
    private DragonTier m_tier = DragonTier.TIER_4;

    protected Vector3 m_lastFeetValidPosition;

    protected override void Start()
    {
        base.Start();
        m_adaptHeight = m_adaptHeight * m_transform.localScale.y;
        m_snapHeight = m_snapHeight * m_transform.localScale.y;
        m_snapHisteresis = m_snapHisteresis * m_transform.localScale.y;
        
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.powerLevel;
        m_tier = dataSpecial.tier;

        UpdatePowerAreas();
        UpdateSpeedToKill();
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
                    if ( m_grounded )
                    {
                        SetGrounded(false);
                    }
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

        CheckFeet();

	}
    
    // Make sure feet dont get inside collision
    protected void CheckFeet()
    {
        Vector3 pos = m_sensor.bottom.position;
        pos.z = 0f;
        
        if ( m_state != State.Intro)
        {
            // check pos           
            if (DebugSettings.ingameDragonMotionSafe && Physics.Linecast( m_lastFeetValidPosition, pos, out m_raycastHit, GameConstants.Layers.GROUND_PLAYER_COLL, QueryTriggerInteraction.Ignore ))
            {
                Vector3 diff = m_lastFeetValidPosition - pos;
                Vector3 dir = diff.normalized;
                float magnitude = diff.magnitude;
                float dot =  Vector3.Dot(m_raycastHit.normal, dir);
                if ( dot > 0 )
                { 
                    Vector3 add = m_raycastHit.normal * magnitude * dot * 1.2f;
                    m_transform.position += add;
                }
            }
            else
            {
                m_lastFeetValidPosition = pos;
            }
            
        }
        else
        {
             m_lastFeetValidPosition = pos;
        }

        /*
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
                float cos = Mathf.Cos(Mathf.Deg2Rad * angle);
                float h = c1 / cos;
                float upDistance = h;
                pos.y += upDistance;
                if (float.IsNaN(pos.y))
                    Debug.Log("NAN!!!!!");
                m_impulse.y = 0;
            }
            
            m_transform.position = pos;
        }
        */
    }

    protected void CustomIdleMovement( float delta )
    {
    
        CustomCheckGround( out m_raycastHit );
        if ( m_height < m_adaptHeight )
        {
            if ( m_height < m_snapHeight /*&& !AngleIsTooMuch( m_lastGroundHitNormal )*/)
            {
                if (!m_grounded)
                {
                    // STOMP!!
                    GroundStomp();
                }
                
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
                m_impulse.y = -9.81f * m_freeFallGravityMultiplier * delta;
                
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
        
        CustomCheckGround( out m_raycastHit );
        if ( m_height < m_adaptHeight )
        {
            if ( m_height < m_snapHeight /*&& !AngleIsTooMuch( m_lastGroundHitNormal )*/)
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
                m_impulse = m_direction * m_walkSpeed;
                m_impulse.y += -9.81f * m_freeFallGravityMultiplier * delta;
                SnapToGround();
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
        Vector3 bPos = m_sensor.bottom.position;
        bPos.z = 0;
        Vector3 tPos = m_transform.position;
        tPos.z = 0;
        Vector3 diff = tPos - bPos;
        m_transform.position = m_lastGroundHit + diff + m_lastGroundHitNormal * (m_snapHeight - m_snapHisteresis) / m_transform.localScale.y;
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
        m_impulse += -(m_impulse.normalized * m_freeFallFriction * impulseMag * _deltaTime);
            
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
        if ( m_impulse.sqrMagnitude > m_speedToKill)
        {
            StunAndKill(m_sensor.bottom.position);
        }
    }
    
    
    override protected void CustomOnCollisionEnter(Collider _collider, Vector3 _normal, Vector3 _point)
    {
        base.CustomOnCollisionEnter( _collider, _normal, _point );
        OnDinoCollision( _collider, _normal, _point );
    }

    protected void OnDinoCollision(Collider _collider, Vector3 _normal, Vector3 _point)
    {
        // Check speed and head stomp!
        if ( m_powerLevel >= 3 && m_impulse.sqrMagnitude > m_speedToKill)
        {
            StunAndKill(_point);
        }
    }
    
    protected void StunAndKill(Vector3 center)
    {
        Messenger.Broadcast<float, float>(MessengerEvents.CAMERA_SHAKE, 0.5f, 0f);
        float area = Mathf.Max(m_currentStunArea, m_currentKillArea);
        float sqrKill = m_currentKillArea * m_currentKillArea;
        m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2)center, area, m_checkEntities);
        for (int i = 0; i < m_numCheckEntities; i++) 
        {
            Entity prey = m_checkEntities[i];
            AI.Machine machine =  prey.machine as AI.Machine;
            if ( machine != null )
            {
                Vector3 diff = machine.position - center;
                diff.z = 0;
                if (prey.CanBeSmashed(m_tier) && diff.sqrMagnitude < sqrKill)
                {
                    machine.Smash( IEntity.Type.PLAYER );
                }
                else
                {
                    machine.Stun( m_stunDuration );
                }
            }
        }
    }

    protected bool CustomCheckGround(out RaycastHit _bottomHit) 
    {
        Vector3 distance = -m_transform.up ;
        distance.z = 0;
        distance.Normalize();
        distance = distance * 10;
        
        bool hit_Bottom = false;

        Vector3 bottomSensor  = m_sensor.bottom.position;
        hit_Bottom = Physics.Linecast(bottomSensor, bottomSensor + distance, out _bottomHit, GameConstants.Layers.GROUND_PLAYER_COLL);

        if (hit_Bottom) {
            m_height = _bottomHit.distance * m_transform.localScale.y;
            m_closeToGround = m_height < 1f;
            m_lastGroundHit = _bottomHit.point;
            m_lastGroundHitNormal = _bottomHit.normal;
        } else {
            m_height = 100f;
            m_closeToGround = false;
        }
        return m_closeToGround;
    }
    
    public bool AngleIsTooMuch( Vector3 _normal )
    {
        float angle = _normal.ToAngleDegreesXY();
        float maxAngle = 45.0f;
        return angle > (180 - maxAngle) || angle < maxAngle;
    }
    
    public void UpdatePowerAreas()
    {
        m_currentKillArea = m_killArea;
        m_currentStunArea = m_stunArea;
        if ( m_powerLevel >= 2 )
        {
            m_currentKillArea = m_level2KillArea;
            m_currentStunArea = m_level2StunArea;
        }
        
        float scale = transform.localScale.y;
        m_currentKillArea = m_currentKillArea * scale;
        m_currentStunArea = m_currentStunArea * scale;
    }
    
    public void UpdateSpeedToKill()
    {
        m_speedToKill = absoluteMaxSpeed * m_speedPercentageToKill / 100.0f;
        m_speedToKill = m_speedToKill * m_speedToKill;
    }

    private void OnDrawGizmos() {
        UpdatePowerAreas();
        UpdateSpeedToKill();
    
        Gizmos.color = new Color(1, 0, 0, 0.1f);

        Transform center = m_sensor.bottom;
        if ( center == null )
        {
            Transform sensors   = transform.Find("sensors").transform;
            center     = sensors.Find("BottomSensor").transform;
        }
        Gizmos.DrawSphere( center.position, m_currentKillArea);
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawSphere( center.position, m_stunArea);
    }
    


}
