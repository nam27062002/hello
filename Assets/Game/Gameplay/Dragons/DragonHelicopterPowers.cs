using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonHelicopterPowers : MonoBehaviour, IBroadcastListener 
{
	DragonBoostBehaviour m_playerBoost;
	DragonMotion m_playerMotion;
    DragonBreathBehaviour m_playerBreath;
    DragonEatBehaviour m_eatBehaviour;
    Animator m_animator;
    int m_powerLevel = 0;
    DragonTier m_tier;

    [Header("Power Level 0 - Machinegun")]
    private bool m_machinegunFiring = false;
    private Entity[] m_checkEntities = new Entity[30];
    private int m_numCheckEntities = 0;
    private bool m_killEverything = false;
    public float m_machinegunDistance = 1;
    public float m_machinegunAngle = 90;
    public Transform m_machingegunAnchor;
    public string m_machineGunParticleName;
    public Transform m_machineGunParticleTransform;
    protected ParticleSystem m_machinegunParticle;
    
    
    // Missiles - Power Level 1
    [Header("Power Level 1 - Missiles")]
    private PoolHandler m_missilesPoolHandler;
    private float m_missileTimer;
    public float m_missilesFireRate;
	public List<Transform> m_missilesFirePositions = new List<Transform>();
	public string m_missilesProjectileName;
	public float m_missilesRange = 10;
    
    
    // Bombs - Power Level 2
    [Header("Power Level 2 - Bombs")]
    private PoolHandler m_bombsPoolHandler;
    private float m_bombTimer = 0;
    public float m_bombFireRate;
    
    public int m_burstCount = 3;    // number of bombs per burst
    protected int m_burstCounter = 0;   // current burst bombs remaining
    
    public float m_burstFireRate = 0.25f;
    protected float m_burstTimer = 0;
    
    
    public string m_bombProjectileName;
    public Transform m_bombFirePosition;
    protected bool m_hatchOpen = false;

    [Header("Power Level 3 - Custom Pet")]
    public string m_petSku = "";

    [Header("Other")]
    public List<Transform> m_scaleParticles = new List<Transform>();
    public float m_minParticleScale = 0.5f;
    public float m_maxParticleScale = 1.1f;
    protected float m_particleScale = 1;

    protected float m_neckDistance = 0;

    RaycastHit[] results;
    int layerMask;
    bool destroys = false;
    int destroyFrame = 0;
    ToggleParam m_toggleParam = new ToggleParam();
    
    private void Awake()
    {
        destroys = FeatureSettingsManager.instance.IsHelicopterDestroying;
        if ( destroys )
        {
            results = new RaycastHit[3];
            layerMask = 1 << LayerMask.NameToLayer("Triggers") | 1 << LayerMask.NameToLayer("Obstacle");
        }
    }

    // Use this for initialization
    void Start () {
		m_playerBoost = InstanceManager.player.dragonBoostBehaviour;
		m_playerMotion = InstanceManager.player.dragonMotion;
        m_playerMotion.canDive = true;  // This dragon can move freely inside water
        m_playerBreath = InstanceManager.player.breathBehaviour;
        m_eatBehaviour = InstanceManager.player.dragonEatBehaviour;
        
        m_powerLevel = (InstanceManager.player.data as DragonDataSpecial).m_powerLevel;
        m_tier = InstanceManager.player.data.tier;
        m_animator = GetComponent<Animator>();
        
        CreatePool();
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);

        // Resize
        float scale = InstanceManager.player.data.scale;
        m_missilesRange = m_missilesRange * scale;
        m_machinegunDistance = m_machinegunDistance * scale;

        if (!string.IsNullOrEmpty(m_machineGunParticleName)) {
            m_machinegunParticle = ParticleManager.InitLeveledParticle(m_machineGunParticleName, m_machineGunParticleTransform);
            m_machinegunParticle.gameObject.SetActive(true);
        }

        // Check if we need to spawn the drone!
        if ( m_powerLevel >= 3 )
        {
            DragonEquip equip = transform.parent.GetComponent<DragonEquip>();
            equip.EquipPet(m_petSku, 4);
        }
        
        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
	}

	void OnDestroy()
	{
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
	}
	
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                CreatePool();
            }break;
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFuryToggled(furyRushToggled.activated, furyRushToggled.type);
            }break;
        }
    }
    
	// Update is called once per frame
	void Update () {

        // Enlargin neck
        Transform target = m_eatBehaviour.GetAttackTarget();
        if ( target == null )
        {
            m_neckDistance = Mathf.Lerp(m_neckDistance, 0, Time.deltaTime * 10);
        }
        else
        {
            // Distance to target?
            m_neckDistance = Mathf.Lerp(m_neckDistance, 1, Time.deltaTime * 10);
        }
        m_animator.SetFloat( GameConstants.Animator.NECK_DISTANCE, m_neckDistance );
        
        // InstanceManager.player.dragonEatBehaviour.enabled = false;  // Dirty code to test
		if ( m_playerBoost.IsBoostActive() || m_playerBreath.IsFuryOn())
		{
            if ( !m_machinegunFiring )
            {
                m_machinegunFiring = true;
                m_animator.SetBool( GameConstants.Animator.SHOOTING, true);
                if ( m_machinegunParticle != null)
                    m_machinegunParticle.Play();
                    
                m_toggleParam.value = m_machinegunFiring;
                Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
            }
            Vector3 arcOrigin = m_machingegunAnchor.position;
            arcOrigin.z = 0;
            
            Vector3 dir = -m_machingegunAnchor.right;
            dir.z = 0;
            dir.Normalize();
            
            // Machinegun killing
            m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(arcOrigin, m_machinegunDistance, m_checkEntities);
            for (int e = 0; e < m_numCheckEntities; e++) 
            {
                Entity entity = m_checkEntities[e];
                if (entity.transform != transform && entity.IsEdible())
                {
                    if ( (entity.hideNeedTierMessage ) && !entity.IsEdible( m_tier ) && !m_killEverything)
                        continue;
                    
                    // Start bite attempt
                    Vector3 heading = (entity.transform.position - arcOrigin);
                    float dot = Vector3.Dot(heading, dir);
                    if ( dot > 0)
                    {
                        // Check arc
                        Vector3 circleCenter = entity.circleArea.center;
                        circleCenter.z = 0;
                        if (MathUtils.TestArcVsCircle( arcOrigin, m_machinegunAngle, m_machinegunDistance, dir, circleCenter, entity.circleArea.radius))
                        {
                            // Kill! Despite we are shooting the machine gun, we treat it like a bite
                            entity.machine.Bite();
                            entity.machine.BeginSwallowed(entity.transform, true, IEntity.Type.PLAYER, KillType.SHOT); // Specify the kill type
                            entity.machine.EndSwallowed(entity.transform);
                        }
                    }
                }
            }
        
        
            if ( m_powerLevel >= 1 )
            {
                m_missileTimer -= Time.deltaTime;
                if ( m_missileTimer <= 0 )
                {
                    m_animator.SetTrigger( GameConstants.Animator.MISSILE );
                    m_missileTimer += m_missilesFireRate;
                }
            }
            
            if ( m_powerLevel >= 2 )
            {
                if (!m_hatchOpen)
                {
                    m_hatchOpen = true;
                    m_animator.SetBool(GameConstants.Animator.BOMB, true);
                    m_bombTimer = m_bombFireRate;
                    m_burstCounter = 0;
                }
                
                if ( m_burstCounter > 0 )
                {
                    m_burstTimer -= Time.deltaTime;
                    if ( m_burstTimer <= 0 )
                    {
                        OnLaunchBomb();
                        m_burstTimer += m_burstFireRate;
                        m_burstCounter--;
                        if ( m_burstCounter <= 0 )
                        {
                            m_bombTimer = m_bombFireRate;
                        }
                    }
                }
                else
                {
                    m_bombTimer -= Time.deltaTime;
                    if ( m_bombTimer <= 0 )
                    {
                        m_burstCounter = m_burstCount;
                        m_burstTimer = 0;
                    }    
                }
                
            }

            if ( destroys )
            {
                destroyFrame++;
                if ( destroyFrame % 2 == 0 )
                {
                    destroyFrame = 0;
                    // Break things
                    int num = Physics.RaycastNonAlloc(arcOrigin, dir, results, m_machinegunDistance, layerMask);
                    for (int i = 0; i < num; i++)
                    {
                        DestructibleDecoration decoration = results[i].collider.GetComponent<DestructibleDecoration>();
                        if ( decoration != null && decoration.CanBreakByShooting())
                        {
                            decoration.Break(false);
                        }
                    }
                }
            }

            m_particleScale += Time.deltaTime * 10;
            if (m_particleScale > m_maxParticleScale) m_particleScale = m_maxParticleScale;
        }
        else
        {
            if ( m_machinegunFiring )
            {
                m_machinegunFiring = false;
                m_animator.SetBool( GameConstants.Animator.SHOOTING, false);
                if ( m_machinegunParticle != null )
                    m_machinegunParticle.Stop();
                m_toggleParam.value = m_machinegunFiring;
                Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
            }
            
            if ( m_hatchOpen )
            {
                m_hatchOpen = false;
                m_animator.SetBool(GameConstants.Animator.BOMB, false);
            }
            
            m_particleScale -= Time.deltaTime * 10;
            if (m_particleScale < m_minParticleScale) m_particleScale = m_minParticleScale;
        }

        int max = m_scaleParticles.Count;
        for (int i = 0; i < max; i++)
        {
            m_scaleParticles[i].localScale = GameConstants.Vector3.one * m_particleScale;
        }
    }

	private void FireMissile( int index )
	{
        
		// Fire!!
		Transform originTransform = m_missilesFirePositions[index];

		// Search target!
		Transform alternateTarget = null;
		Transform target = null;
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(originTransform.position, m_missilesRange);
		for (int i = 0; i < preys.Length && target == null; i++) 
		{
			if (preys[i].IsBurnable(m_tier)) 
			{
				if ( alternateTarget == null ){
					alternateTarget = preys[i].transform;
				}else if ( Vector3.Dot( -originTransform.right, preys[i].transform.position - originTransform.position) > 0 ){
					target = preys[i].transform;
				}
			}
		}
		if ( target == null && alternateTarget != null )
			target = alternateTarget;

        GameObject go = m_missilesPoolHandler.GetInstance();
        if ( go != null)
        { 
            PetProjectile projectile = go.GetComponent<PetProjectile>();
            projectile.tier = m_tier;
            projectile.transform.position = originTransform.position;
            projectile.transform.rotation = originTransform.rotation;   
    		if ( target != null )
    		{
                projectile.explodeIfHomingtargetNull = true;
    			projectile.Shoot(target, originTransform.forward, 9999, originTransform);
    		}
            else 
            {
                projectile.explodeIfHomingtargetNull = false;
                projectile.ShootAtPosition( originTransform.position + originTransform.forward * 1000, originTransform.forward, 9999, originTransform);
            }
		}
	}

	void CreatePool() {
        
        if ( m_powerLevel >= 1 && !string.IsNullOrEmpty(m_missilesProjectileName))
		    m_missilesPoolHandler = PoolManager.CreatePool(m_missilesProjectileName, 2, true);
        if ( m_powerLevel >= 2 && !string.IsNullOrEmpty(m_bombProjectileName))
            m_bombsPoolHandler = PoolManager.CreatePool(m_bombProjectileName, m_burstCount, true);
	}
    
    public void OnLaunchMissile1()
    {
        FireMissile(0);
    }
    
    public void OnLaunchMissile2()
    {
        FireMissile(1);
    }
    
    public void OnLaunchBomb()
    {    
        // Fire!!
        Transform originTransform = m_bombFirePosition;

        GameObject go = m_bombsPoolHandler.GetInstance();
        if (go != null) {
            PetProjectile projectile = go.GetComponent<PetProjectile>();
            projectile.tier = m_tier;
            projectile.transform.position = originTransform.position;
            projectile.transform.rotation = originTransform.rotation;
            projectile.ShootAtPosition(transform.position, Vector3.down, 9999, originTransform);
            projectile.velocity = GameConstants.Vector3.up * Mathf.Min(m_playerMotion.velocity.y, -projectile.speed);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (m_machingegunAnchor == null)
            return;
        Gizmos.color = Color.white;
        Vector3 arcOrigin = m_machingegunAnchor.position;
        arcOrigin.z = 0;
        Gizmos.DrawWireSphere(arcOrigin, m_machinegunDistance);

        Vector2 dir = (Vector2)(-m_machingegunAnchor.right);
        dir.Normalize();
        
        Vector2 dUp = dir.RotateDegrees(m_machinegunAngle/2.0f);
        Debug.DrawLine( m_machingegunAnchor.position, m_machingegunAnchor.position + (Vector3)(dUp * m_machinegunDistance) );
        Vector2 dDown = dir.RotateDegrees(-m_machinegunAngle/2.0f);
        Debug.DrawLine( m_machingegunAnchor.position, m_machingegunAnchor.position + (Vector3)(dDown * m_machinegunDistance) );
        
    }

    void OnFuryToggled(bool toogle, DragonBreathBehaviour.Type type)
    {
        if ( type == DragonBreathBehaviour.Type.Mega )
        {
            m_animator.SetBool( GameConstants.Animator.MEGA, toogle );
        }
    }

}
