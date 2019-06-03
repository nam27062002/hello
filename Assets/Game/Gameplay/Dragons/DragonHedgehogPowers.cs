using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class DragonHedgehogPowers : MonoBehaviour, IBroadcastListener {

    [Header("Damage Area Settings")]
    public DragonTier m_tier = DragonTier.TIER_4;
    public IEntity.Type m_type = IEntity.Type.PLAYER;
    public float m_fireAreaMultiplier = 2;

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
    
    [Header("Particle")]
    public Transform m_particleCenter;
    public string m_fireParticle = "FireCircle/PS_SonicFireRush";
    private ParticleSystem m_fireParticleInstance;
    public string m_fireParticleStart = "FireCircle/PS_SonicFireRushBoost";
    private ParticleSystem m_fireParticleStartInstance;
    
    public string m_megaFireParticle = "FireCircle/PS_SonicMegaFireRush";
    private ParticleSystem m_megaFireParticleInstance;
    public string m_megaFireParticleStart = "FireCircle/PS_SonicMegaFireRushBoost";
    private ParticleSystem m_megaFireParticleStartInstance;
    
    public Transform m_trailPosition;
    public string m_trailParticle = "FireCircle/PS_SonicDragonFireTrail";
    private GameObject m_trailParticleInstance;
    public string m_megaTrailParticle = "FireCircle/PS_SonicDragonMegaFireTrail";
    private GameObject m_megaTrailParticleInstance;
    
    
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

    protected float m_fireNodeTimer = 0;
    Rect m_bounds2D;
    ToggleParam m_toggleParam = new ToggleParam();
    float m_biggerDragonCheck = 0;
    
    
	// Use this for initialization
	void Start () {
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
		m_circle = GetComponent<CircleArea2D>();
        m_circle.radius = m_circle.radius * dataSpecial.scale;
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
        
        
        m_powerLevel = dataSpecial.powerLevel;
        
        m_spikesLvl1.SetActive( m_powerLevel > 1 );
        m_spikesLvl2.SetActive( m_powerLevel > 2 );
        
        if ( m_powerLevel >= 1 )
        {
            if ( m_powerLevel >= 2 )
            {
                // Create pool of spikes!
                CreatePool();
            }
        }
        
        
        m_fireParticleInstance = ParticleManager.InitLeveledParticle( m_fireParticle, m_particleCenter);
        m_fireParticleStartInstance = ParticleManager.InitLeveledParticle( m_fireParticleStart, m_particleCenter);
        m_megaFireParticleInstance = ParticleManager.InitLeveledParticle( m_megaFireParticle, m_particleCenter);
        m_megaFireParticleStartInstance = ParticleManager.InitLeveledParticle( m_megaFireParticleStart,m_particleCenter);


        m_trailParticleInstance = ParticleManager.InitLeveledParticleObject( m_trailParticle, m_trailPosition);
        m_megaTrailParticleInstance = ParticleManager.InitLeveledParticleObject( m_megaTrailParticle, m_trailPosition);
        
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
        m_poolHandler = PoolManager.CreatePool("PF_Hedgehog_Horn", m_spikesNumber, true);
        m_level3PoolHandler = PoolManager.CreatePool("PF_Hedgehog_Horn_P3", 3, true);
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

                m_shootingTimer = 0.1f;
                m_toggleParam.value = m_active;
                Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
            }
            
            if ( m_fire )
            {
                if (!m_breathBehaviour.isFuryPaused)
                {
                    m_breathBehaviour.PauseFury();
                    StartSonicFire( m_fireType );
                }
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
            
            if ( m_motion.state == DragonMotion.State.Extra_2 || m_fire )
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
                        else if ( (Time.time - m_biggerDragonCheck) > 1.0f && prey.IsBurnable( DragonTierGlobals.LAST_TIER ) )
                        {
                            m_biggerDragonCheck = Time.time;
                            // Show message saying I cannot burn it
					        Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, DragonTier.COUNT, prey.sku);
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
                        else if ((Time.time - m_biggerDragonCheck) > 1.0f && prey.CanBeSmashed( DragonTierGlobals.LAST_TIER ) )
                        {
                            m_biggerDragonCheck = Time.time;
                            Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, prey.MinSmashTier(), prey.sku);
                        }
    				}
    			}
                
                if ( m_fire )
                {
                    m_fireNodeTimer -= Time.deltaTime;
                    if (m_fireNodeTimer <= 0) {
                        m_fireNodeTimer += 0.25f;
                        m_bounds2D.Set(m_circle.center.x - m_circle.radius, m_circle.center.y - m_circle.radius, m_circle.radius * 2, m_circle.radius * 2);
                        FirePropagationManager.instance.FireUpNodes(m_bounds2D, Overlaps, m_tier, m_fireType, m_motion.direction, IEntity.Type.PLAYER);
                    }
                }
            }
		}
		else
		{
			if (m_active)
			{
                m_active = false;
                
                m_toggleParam.value = m_active;
                Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
                    
                RewardManager.instance.canLoseMultiplier = true;
				m_healthBehaviour.RemoveDamageIgnore( DamageType.ARROW );
                m_healthBehaviour.RemoveDamageIgnore( DamageType.NORMAL );
                m_healthBehaviour.RemoveDamageIgnore( DamageType.EXPLOSION );
                if ( m_powerLevel >= 1 )
                    m_healthBehaviour.RemoveDamageIgnore( DamageType.MINE );
                
                // if fire still active resume breathing
                if ( m_fire )
                {
                    if ( m_breathBehaviour.isFuryPaused )
                    {
                        m_breathBehaviour.ResumeFury();
                        StopSonicFire( m_fireType );
                    }
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
                
			}
		}
	}
    
    public bool Overlaps(CircleAreaBounds _circle)
    {
        return m_circle.Overlaps(_circle.center, _circle.radius);
    }
    
    
    void LateUpdate()
    {
        if ( m_fire && ( m_motion.state == DragonMotion.State.Extra_1 || m_motion.state == DragonMotion.State.Extra_2 ))
        {
            switch(m_fireType)
            {
                case  DragonBreathBehaviour.Type.Standard:
                {
                    if (m_fireParticleInstance)
                        m_fireParticleInstance.transform.rotation = Quaternion.identity;
                    if (m_fireParticleStartInstance)
                        m_fireParticleStartInstance.transform.rotation = Quaternion.identity;
                }break;
                case DragonBreathBehaviour.Type.Mega:
                {
                    if (m_megaFireParticleInstance)
                        m_megaFireParticleInstance.transform.rotation = Quaternion.identity;
                    if (m_megaFireParticleStartInstance)
                        m_megaFireParticleStartInstance.transform.rotation = Quaternion.identity;
                }break;
            }
        }
    }

	void OnFuryRushToggled( bool fire, DragonBreathBehaviour.Type fireType)
	{
    	m_fire = fire;
    	m_fireType = fireType;
    	if ( m_fire )
    	{
    		m_circle.radius = m_originalRadius * m_fireAreaMultiplier;	
    	}
    	else
    	{   
    		m_circle.radius = m_originalRadius;
            StopSonicFire( fireType );
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
    
    
    private void StartSonicFire( DragonBreathBehaviour.Type fireType )
    {
        // Play start particle
        switch( fireType )
        {
            case DragonBreathBehaviour.Type.Standard:
            {
                if (m_fireParticleInstance)
                    m_fireParticleInstance.gameObject.SetActive(true);
                if (m_fireParticleStartInstance)
                {
                    m_fireParticleStartInstance.gameObject.SetActive(true);
                    m_fireParticleStartInstance.Play();
                }
                m_trailParticleInstance.SetActive(true);
                
            }break;
            case DragonBreathBehaviour.Type.Mega:
            {
                if (m_megaFireParticleInstance)
                    m_megaFireParticleInstance.gameObject.SetActive(true);
                if (m_megaFireParticleStartInstance)
                {
                    m_megaFireParticleStartInstance.gameObject.SetActive(true);
                    m_megaFireParticleStartInstance.Play();
                }
                m_megaTrailParticleInstance.SetActive(true);
            }break;
        }
        
        
    }
    
    private void StopSonicFire( DragonBreathBehaviour.Type fireType )
    {
        switch( fireType )
        {
            case DragonBreathBehaviour.Type.Standard:
            {
                if (m_fireParticleInstance)
                    m_fireParticleInstance.gameObject.SetActive(false);
                if ( m_fireParticleStartInstance )
                    m_fireParticleStartInstance.gameObject.SetActive(false);
                m_trailParticleInstance.SetActive(false);
            }break;
            case DragonBreathBehaviour.Type.Mega:
            {
                if (m_megaFireParticleInstance)
                    m_megaFireParticleInstance.gameObject.SetActive(false);
                if (m_megaFireParticleStartInstance)
                    m_megaFireParticleStartInstance.gameObject.SetActive(false);
                m_megaTrailParticleInstance.SetActive(false);
            }break;
        }
    }
}
