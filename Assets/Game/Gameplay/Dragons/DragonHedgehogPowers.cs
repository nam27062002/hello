using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class DragonHedgehogPowers : MonoBehaviour, IBroadcastListener {

    [Header("Damage Area Settings")]
    public DragonTier m_tier = DragonTier.TIER_4;
    public IEntity.Type m_type = IEntity.Type.PLAYER;
    public float m_fireBoostMultiplier = 2;

    [Header("Level 2 Spikes")]
    public int m_spikesNumber = 4;
    public float m_spikesOperture = 75.0f;
    protected bool m_shootLevel2Spikes = false;
    public bool shootLevel2Spikes{
        set{ m_shootLevel2Spikes = value; }
    }

    [Header("Level 3 Spikes")]
    public float m_shootingRatio = 0.1f;
    protected float m_shootingTimer = 0;
    
    
    [Header("Visual Settings")]
    public GameObject m_spikesLvl1;
    public GameObject m_spikesLvl2;
    
	private CircleArea2D m_circle;
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;
	DragonMotionHedgehog m_motion;
	private float m_originalRadius;
	private bool m_active = false;
	protected Transform m_transform;
	protected DragonPlayer m_player;
	protected bool m_fire = false;
	protected DragonBreathBehaviour.Type m_fireType;
    
    private int m_powerLevel = 0;
    DragonBreathBehaviour m_breathBehaviour;
    DragonHealthBehaviour m_healthBehaviour;
    private PoolHandler m_poolHandler;
    private PoolHandler m_level3PoolHandler;
    Vector3 m_tmpVector = GameConstants.Vector3.right;

    public bool m_isUsingCircleFire = true;

	// Use this for initialization
	void Start () {
		m_circle = GetComponent<CircleArea2D>();
		m_originalRadius = m_circle.radius;
		m_player = InstanceManager.player;
		m_motion = m_player.dragonMotion as DragonMotionHedgehog;
		m_tier = m_player.data.tier;
		m_transform = transform;

        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);

        m_breathBehaviour = GetComponentInParent<DragonBreathBehaviour>();
        m_healthBehaviour = GetComponentInParent<DragonHealthBehaviour>();
        
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.powerLevel;
        
        m_spikesLvl1.SetActive( m_powerLevel > 0 );
        m_spikesLvl2.SetActive( m_powerLevel > 1 );
        
        
        if ( m_powerLevel >= 2 )
        {
            // Create pool of spikes!
            CreatePool();
        }

	}

	void OnDestroy()
	{
		Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFuryRushToggled( furyRushToggled.activated, furyRushToggled.type );
            }break;
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                if (m_powerLevel >= 2)
                {
                    CreatePool();
                }
            }break;
        }
    }
    
    void CreatePool() {
        m_poolHandler = PoolManager.CreatePool("PF_Hedgehog_Horn", "Game/Projectiles/", m_spikesNumber, true);
        m_level3PoolHandler = PoolManager.CreatePool("PF_Hedgehog_Horn_P3", "Game/Projectiles/", 3, true);
    }
    
    private void IgnoreLevel2Spikes()
    {
            // It would be nice to have a callback when the pool grows
        Transform containerTransform = m_poolHandler.pool.containerObj.transform;
        int childCount = containerTransform.childCount;
        
        for (int i = 0; i < childCount; i++)
        {
            Collider c = containerTransform.GetChild(i).GetComponent<Collider>();
            for (int j = 0; j < childCount; j++)
            {
                Physics.IgnoreCollision(containerTransform.GetChild(j).GetComponent<Collider>(), c);
            }
            
        }
    }
            
    
	// Update is called once per frame
	void Update () {

		if ( m_motion.state == DragonMotion.State.Extra_2 || m_motion.state == DragonMotion.State.Extra_1)
		{
            if (!m_active)
            {
                m_active = true;
                RewardManager.instance.canLoseMultiplier = false;
                m_healthBehaviour.AddDamageIgnore( DamageType.ARROW );
                m_healthBehaviour.AddDamageIgnore( DamageType.NORMAL );
                m_healthBehaviour.AddDamageIgnore( DamageType.EXPLOSION );
                if ( m_powerLevel >= 1 )
                    m_healthBehaviour.AddDamageIgnore( DamageType.MINE );
                
                // Play start particle
            }
            
            if ( m_fire && !m_isUsingCircleFire )   // if we are using circle fury we dont need to pause mouth fire
            {
                if (!m_breathBehaviour.isFuryPaused)
                    m_breathBehaviour.PauseFury();
                // Advance fire timer to make it end even if not breathing because we are in ricochet form
                if ( !m_breathBehaviour.IsInfiniteFury() )
                {
                    m_breathBehaviour.AdvanceRemainingFire();
                    if ( m_breathBehaviour.remainingFuryDuration <= 0 )
                    {
                        // Let it end
                        m_breathBehaviour.ResumeFury();
                    }
                }
            }
            
            
            if ( m_powerLevel >= 3 && m_motion.state == DragonMotion.State.Extra_2 ) 
            {
                m_shootingTimer -= Time.deltaTime;
                if ( m_shootingTimer <= 0 )
                {
                    m_shootingTimer = m_shootingRatio;
                    // Shoot spikes!
                    Vector3 dir = m_motion.direction.RotateXYDegrees(Random.Range(-20, 20));
                    ShootHorn( dir, m_level3PoolHandler, true, m_motion.speed * 1.5f );
                }
            }

            bool checkSmah = false;
            if ( m_isUsingCircleFire )
            {
                checkSmah = m_motion.state == DragonMotion.State.Extra_2 && !m_fire;    // Only smash if we cannot burn
            }
            else
            {
                checkSmah = m_motion.state == DragonMotion.State.Extra_2 || m_fire;
            }
            
            if ( m_motion.state == DragonMotion.State.Extra_2 || (m_fire && !m_isUsingCircleFire) )
            {
    			m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2)m_circle.center, m_circle.radius, m_checkEntities);
    			for (int i = 0; i < m_numCheckEntities; i++) 
    			{
                    // if power up level >= 1 check if mine, and do not destroy it, we need to bounce off of it
    				Entity prey = m_checkEntities[i];
    				if ( m_fire )
    				{
    					if (prey.IsBurnable(m_tier) || m_fireType == FireBreath.Type.Mega) {
    						AI.IMachine machine =  m_checkEntities[i].machine;
    						if (machine != null) {
    							machine.Burn(transform, IEntity.Type.PLAYER);
    						}
    					}
    
    				}else{
    					if ( prey.CanBeSmashed( m_tier ) )
    					{
    						AI.IMachine machine =  prey.machine;
    						if (machine != null) 
    						{
    							machine.Smash( m_type );
    							// User this if you want it to count as eaten
    							// machine.BeginSwallowed(m_transform, true, m_type);
    							// machine.EndSwallowed(m_transform);
    						}
    					}
    				}
    			}
            }
		}
		else
		{
			if (m_active)
			{
                m_active = false;
                RewardManager.instance.canLoseMultiplier = true;
				m_healthBehaviour.RemoveDamageIgnore( DamageType.ARROW );
                m_healthBehaviour.RemoveDamageIgnore( DamageType.NORMAL );
                m_healthBehaviour.RemoveDamageIgnore( DamageType.EXPLOSION );
                if ( m_powerLevel >= 1 )
                    m_healthBehaviour.RemoveDamageIgnore( DamageType.MINE );
                
                // if fire still active resume breathing
                if ( m_fire && !m_isUsingCircleFire)
                {
                    if ( m_breathBehaviour.isFuryPaused )
                        m_breathBehaviour.ResumeFury();
                }
                
                if (m_shootLevel2Spikes)
                {
                    m_shootLevel2Spikes = false;
                    // Shoot end movement spikes!
                    Vector3 dir = m_motion.direction;
                    Util.RotateXYDegrees(ref dir, -m_spikesOperture );
                    float angle = (m_spikesOperture  * 2.0f) / m_spikesNumber;
                    for (float i = -m_spikesOperture ; i <= m_spikesOperture ; i += angle)
                    {
                        Util.RotateXYDegrees(ref dir, angle);
                        ShootHorn( dir, m_poolHandler );
                    }
                    IgnoreLevel2Spikes();
                }
                
                // Stop Particles
			}
		}
	}

	void OnFuryRushToggled( bool fire, DragonBreathBehaviour.Type fireType)
	{
    	m_fire = fire;
    	m_fireType = fireType;
    	if ( m_fire )
    	{
    		m_circle.radius = m_originalRadius * m_fireBoostMultiplier;	
    	}
    	else
    	{   
    		m_circle.radius = m_originalRadius;
            // if in ricochet form stop fire
            if (m_motion.state == DragonMotion.State.Extra_2 || m_motion.state == DragonMotion.State.Extra_1)
            {
                // Stop fire particles if playing
            }
    	}
	}
    
    protected void ShootHorn(Vector3 _direction, PoolHandler _pool, bool overrideSpeed = false, float speed = 10)
    {
        GameObject go = _pool.GetInstance();
        go.transform.position = m_motion.m_rotationPivot.position + m_motion.direction;
        Projectile projectile = go.GetComponent<Projectile>();
        if ( overrideSpeed )
        {
            projectile.ShootTowards(_direction, speed, 1000, transform );
        }else{
            projectile.ShootTowards(_direction, projectile.speed, 1000, transform );
        }
        
    }
}
