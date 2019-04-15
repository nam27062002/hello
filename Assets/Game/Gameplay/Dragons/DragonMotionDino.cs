using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DragonMotionDino : DragonMotion {

    public float m_maxUpAngle = 45.0f;
    public float m_fallingAngle = -45.0f;
    public float m_freeFallGravityMultiplier = 1;
    public float m_freeFallFriction = 0.5f;
    public float[] m_walkSpeedByTier = new float[(int)DragonTier.COUNT];
    protected float m_walkSpeed = 2.0f;
    public float m_walkBoostMultiplier = 1.5f;
    public float m_walkRotationSpeed = 10.0f;

    Transform m_groundSensor;
    Transform m_magnetSensor;
    public bool m_grounded = false;
    public float m_maxWalkAngle = 20;
    public float m_maxStationaryAngle = 45;

    
    [Range(0,100.0f)]
    public float m_speedPercentageToKill = 1;
    [Header("Kill Area")]
    public float m_killArea = 2;
    public float m_level2KillArea = 3;
    [Header("Stun Area")]
    public float m_stunArea = 4;
    public float m_level2StunArea = 3;
    public float m_stunDuration = 2;
    [Header("Step killing")]
    public float m_stepKillArea = 1;
    public float m_stepStunArea = 1;
    public float m_stepStunDuration = 2;
    [Header("Enery Lvl1 Multipliers")]
    [Range(0,1)]
    public float m_energyDrainReduction;
    [Range(0,100)]
    public float m_energyRefillBonus;
    
    [Header("Modified on run")]
    public float m_currentKillArea = 2;
    public float m_currentStunArea = 2;
    protected int m_powerLevel = 1;
    public int powerLevel { get { return m_powerLevel; } }
    protected float m_speedToKill;
    protected float m_fallSpeedToKill;
    public float m_currentStepKillArea = 2;
    public float m_currentStepStunArea = 2;
    
    private Entity[] m_checkEntities = new Entity[50];
    private int m_numCheckEntities = 0;
    private DragonTier m_tier = DragonTier.TIER_4;

    protected Vector3 m_lastFeetValidPosition;

    protected DragonDinoAnimationEvents m_animEvents;

    protected override void Start()
    {
        base.Start();
        
        Transform sensors   = m_transform.Find("sensors").transform;
        m_groundSensor = sensors.Find("GroundSensor");
        m_magnetSensor = sensors.Find("MagnetSensor");
        
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.powerLevel;
        m_tier = dataSpecial.tier;
        m_walkSpeed = m_walkSpeedByTier[(int)m_tier];
        UpdatePowerAreas();
        UpdateSpeedToKill();
        
        m_animEvents = GetComponentInChildren<DragonDinoAnimationEvents>();
        
        if ( powerLevel >= 1 )
        {
            DragonBoostBehaviour boostBehaviour = InstanceManager.player.dragonBoostBehaviour;
            boostBehaviour.energyDrain = boostBehaviour.energyDrain * m_energyDrainReduction;
            boostBehaviour.AddRefillBonus( m_energyRefillBonus );
        }
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
                if ( m_grounded || !m_boost.IsBoostActive() )
                {
                    CustomUpdateMovement(Time.fixedDeltaTime);
                }
                else
                {
                    UpdateMovement(Time.fixedDeltaTime);
                    CustomCheckGround(out m_raycastHit);
                    if ( m_height < 90 )
                    {
                        // Ground it
                        SetGrounded(true);
                    }
                }
                AfterFixedUpdate();
            }break;
            case State.Dead:
                {
                    if ( m_previousState == State.InsideWater || m_insideWater)
                    {
                        DeadDrowning( Time.fixedDeltaTime );
                    }
                    else
                    {
                        if (m_grounded)
                        {
                            GroundDead( Time.fixedDeltaTime );
                        }
                        else
                        {
                            DeadFall(Time.fixedDeltaTime);
                        }
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
    
    protected void GroundDead(float delta)
    {
        CustomCheckGround(out m_raycastHit);
        if ( m_height <= 90 && !GroundAngleBiggerThan( m_lastGroundHitNormal, m_maxWalkAngle ) ) 
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
            if (GroundAngleBiggerThan(m_lastGroundHitNormal, m_maxStationaryAngle))
                m_impulse.y = -9.81f * m_freeFallGravityMultiplier * delta;
            RotateToGround( m_direction );
            SnapToGround();
        }
        else
        {
            SetGrounded(false);
        }
        
        ApplyExternalForce();
        m_rbody.velocity = m_impulse;
    }
    
    // Make sure feet dont get inside collision
    protected void CheckFeet()
    {
        Vector3 pos = m_sensor.bottom.position;
        pos.z = 0f;
        
        if ( m_state != State.Intro && m_state != State.Dead && m_state != State.Reviving)
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
    }

    protected void CustomIdleMovement( float delta )
    {
    
        CustomCheckGround( out m_raycastHit );
        if ( m_height < 90 )
        {
            if ( !GroundAngleBiggerThan( m_lastGroundHitNormal, m_maxWalkAngle ))
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
                if (GroundAngleBiggerThan(m_lastGroundHitNormal, m_maxStationaryAngle))
                    m_impulse.y = -9.81f * m_freeFallGravityMultiplier * delta;
                RotateToGround( m_direction );
                SnapToGround();
                
            }
            else
            {
                FreeFall(delta, GameConstants.Vector3.zero);
            }
        }
        // Free fall
        else
        {
            FreeFall(delta, GameConstants.Vector3.zero);
        }
        
        // m_desiredRotation = m_transform.rotation;
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
        
        if ( m_boost.IsBoostActive() && Vector3.Dot( m_lastGroundHitNormal, impulse) > 0 )
        {
            // Despegar
            SetGrounded(false);
            UpdateMovement(Time.fixedDeltaTime);
        }
        else
        {
            if ( m_height < 90 )
            {
                if ( !GroundAngleBiggerThan( m_lastGroundHitNormal, m_maxWalkAngle ))
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
                    if ( m_boost.IsBoostActive() )
                    {
                        m_impulse = m_direction * m_walkSpeed * m_walkBoostMultiplier;
                    }
                    else
                    {
                        m_impulse = m_direction * m_walkSpeed;
                    }
                    m_impulse.y += -9.81f * m_freeFallGravityMultiplier * delta;
                    RotateToGround( m_direction );
                    SnapToGround();
                }
                else
                {
                    // Adapt to angle?
                    impulse.y = 0;
                    impulse.Normalize();
                    FreeFall(delta, impulse);
                }
            }
            else
            {
                impulse.y = 0;
                impulse.Normalize();
                FreeFall(delta, impulse);
            }
            
            // m_desiredRotation = m_transform.rotation;
            ApplyExternalForce();
            m_rbody.velocity = m_impulse;
        }
    }
    
    protected void ComputeFreeFallImpulse(float delta)
    {
        // stroke's Drag
        m_impulse = m_rbody.velocity;
        float impulseMag = m_impulse.magnitude;
        m_impulse.y += -9.81f * m_freeFallGravityMultiplier * delta;
        m_impulse += -(m_impulse.normalized * m_freeFallFriction * impulseMag * delta);
    }
    
    protected void RotateToGround( Vector3 direction )
    {
        m_desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
        m_transform.rotation = Quaternion.Lerp(m_transform.rotation, m_desiredRotation, m_walkRotationSpeed * Time.deltaTime);
        m_angularVelocity = GameConstants.Vector3.zero;
        if ( m_spinning != false )
            m_animator.SetBool(GameConstants.Animator.SPIN, false);
        m_spinning = false;
    }

    protected void SnapToGround()
    {
        Vector3 snapPoint = m_lastGroundHit;
        snapPoint.z = 0;
        Vector3 diff = m_transform.position - m_groundSensor.position;
        m_transform.position = snapPoint + diff;
        
        if (!m_grounded)
        {
            SetGrounded(true);
        }
    }

    protected void FreeFall( float delta, Vector3 inputVector )
    {       
        if (m_grounded)
        {
            SetGrounded(false);
        }    
            // Input
        Vector3 acceleration = (inputVector * m_dragonForce * 0.1f) / m_dragonMass;
            // Gravity
        acceleration.y += -9.81f * m_freeFallGravityMultiplier;
        
        // stroke's Drag
        m_impulse = m_rbody.velocity;
        float impulseMag = m_impulse.magnitude;
        m_impulse += (acceleration-(m_impulse.normalized * m_freeFallFriction * impulseMag)) * delta;
        
        if ( inputVector != GameConstants.Vector3.zero )
        { 
            if ( inputVector.x > 0 ){
                m_direction = GameConstants.Vector3.right;
            }else{
                m_direction = GameConstants.Vector3.left;
            }
        }
        else
        {
            if ( m_direction.x > 0 ){
                m_direction = GameConstants.Vector3.right;
            }else{
                m_direction = GameConstants.Vector3.left;
            }
        }
        RotateToDirection(m_direction, false);    
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
            m_rbody.centerOfMass = m_transform.InverseTransformPoint( m_groundSensor.position );
        }

    }

    protected void GroundStomp()
    {
        if ( m_impulse.sqrMagnitude > m_fallSpeedToKill)
        {
            m_animEvents.OnGroundStomp();
            StunAndKill(m_sensor.bottom.position, m_currentKillArea, m_currentStunArea, m_stunDuration);
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
            m_animEvents.OnHeadButt(_point, _normal);
            StunAndKill(_point, m_currentKillArea, m_currentStunArea, m_stunDuration);
        }
    }
    
    public void OnStep()
    {
        StunAndKill(m_lastGroundHit, m_currentStepKillArea, m_currentStunArea, m_stepStunDuration, 0.1f);
    }

    protected void StunAndKill(Vector3 center, float killArea, float stunArea, float stunDuration, float shake = 0.5f)
    {
        Messenger.Broadcast<float, float>(MessengerEvents.CAMERA_SHAKE, shake, 0f);
        float area = Mathf.Max(stunArea, killArea);
        float sqrKill = killArea * killArea;
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
                    machine.Stun( stunDuration );
                }
            }
        }
    }

    protected bool CustomCheckGround(out RaycastHit _bottomHit) 
    {
        Vector3 bottomSensor  = m_sensor.bottom.position;
        bottomSensor.z = 0;
        Vector3 magnetSensorPos = m_magnetSensor.position;
        magnetSensorPos.z = 0;
        bool hit_Bottom = Physics.Linecast(bottomSensor, magnetSensorPos, out _bottomHit, GameConstants.Layers.GROUND_PLAYER_COLL, QueryTriggerInteraction.Ignore );

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
    
    public bool GroundAngleBiggerThan( Vector3 _normal, float maxAngle)
    {
        float angle = _normal.ToAngleDegreesXY();
        return (angle > 180 - maxAngle) || (angle < maxAngle);
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

        m_currentStepKillArea = m_stepKillArea * scale;
        m_currentStepStunArea = m_stepStunArea * scale;
    }
    
    public void UpdateSpeedToKill()
    {
        m_speedToKill = absoluteMaxSpeed * m_speedPercentageToKill / 100.0f;
        m_speedToKill = m_speedToKill * m_speedToKill;

        float terminalFallSpeed = (9.81f * m_freeFallGravityMultiplier) / m_freeFallFriction;
        m_fallSpeedToKill = terminalFallSpeed * m_speedPercentageToKill / 100.0f;
        m_fallSpeedToKill = m_fallSpeedToKill * m_fallSpeedToKill;
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
        Gizmos.DrawSphere( center.position, m_currentStunArea);
        
        
        Gizmos.color = new Color(0, 1, 1, 0.1f);
        Gizmos.DrawSphere( center.position, m_currentStepKillArea);
        
        Gizmos.color = new Color(1, 0, 1, 0.1f);
        Gizmos.DrawSphere( center.position, m_currentStepStunArea);
        
    }
    
        
    


}
