using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonHelicopterPowers : MonoBehaviour 
{
	DragonBoostBehaviour m_playerBoost;
	DragonMotion m_playerMotion;
    DragonBreathBehaviour m_playerBreath;
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
    public ParticleSystem m_machinegunParticle;
    
    
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
    public string m_bombProjectileName;
    public Transform m_bombFirePosition;

    [Header("Power Level 3 - Custom Pet")]
    public string m_petSku = "";
    
	// Use this for initialization
	void Start () {
		m_playerBoost = InstanceManager.player.dragonBoostBehaviour;
		m_playerMotion = InstanceManager.player.dragonMotion;
        m_playerBreath = InstanceManager.player.breathBehaviour;
        
        m_powerLevel = (InstanceManager.player.data as DragonDataSpecial).m_powerLevel;
        m_tier = InstanceManager.player.data.tier;
        m_animator = GetComponent<Animator>();
        
        CreatePool();
        Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, CreatePool);

        // Resize
        float scale = InstanceManager.player.data.scale;
        m_missilesRange = m_missilesRange * scale;
        m_machinegunDistance = m_machinegunDistance * scale;
        
        
        // Check if we need to spawn the drone!
        if ( m_powerLevel >= 3 )
        {
            DragonEquip equip = transform.parent.GetComponent<DragonEquip>();
            equip.EquipPet(m_petSku, 4);
        }
        
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, CreatePool);
	}
	
	// Update is called once per frame
	void Update () {
        // InstanceManager.player.dragonEatBehaviour.enabled = false;  // Dirty code to test
		if ( m_playerBoost.IsBoostActive() || m_playerBreath.IsFuryOn())
		{
            if ( !m_machinegunFiring )
            {
                m_machinegunFiring = true;
                if ( m_machinegunParticle != null)
                    m_machinegunParticle.Play();
            }
            Vector3 arcOrigin = m_machingegunAnchor.position;
            arcOrigin.z = 0;
            // Machinegun killing
            m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(arcOrigin, m_machinegunDistance, m_checkEntities);
            for (int e = 0; e < m_numCheckEntities; e++) 
            {
                Entity entity = m_checkEntities[e];
                if (entity.transform != transform && entity.IsEdible())
                {
                    if ( (entity.hideNeedTierMessage ) && !entity.IsEdible( m_tier ) && !m_killEverything)
                        continue;
                    Vector3 dir = -m_machingegunAnchor.right;
                    // Start bite attempt
                    Vector3 heading = (entity.transform.position - arcOrigin);
                    float dot = Vector3.Dot(heading, dir);
                    if ( dot > 0)
                    {
                        // Check arc
                        Vector3 circleCenter = entity.circleArea.center;
                        circleCenter.z = 0;
                        dir.z = 0;
                        dir.Normalize();
                        if (MathUtils.TestArcVsCircle( arcOrigin, m_machinegunAngle, m_machinegunDistance, dir, circleCenter, entity.circleArea.radius))
                        {
                            // Kill!
                            entity.machine.Bite();
                            entity.machine.BeginSwallowed(entity.transform, true, IEntity.Type.PLAYER);//( m_mouth );
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
                    m_animator.SetTrigger("missile");
                    m_missileTimer += m_missilesFireRate;
                }
            }
            
            if ( m_powerLevel >= 2 )
            {
                m_bombTimer -= Time.deltaTime;
                if ( m_bombTimer <= 0 )
                {
                    m_animator.SetTrigger("bomb");
                    m_bombTimer += m_bombFireRate;
                }
            }
		}
        else
        {
            if ( m_machinegunFiring )
            {
                m_machinegunFiring = false;
                if ( m_machinegunParticle != null )
                    m_machinegunParticle.Stop();
            }
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
        PetProjectile projectile = go.GetComponent<PetProjectile>();
        projectile.tier = m_tier;
        projectile.transform.position = originTransform.position;
        projectile.transform.rotation = originTransform.rotation;
		if ( target != null )
		{
            projectile.motionType = Projectile.MotionType.Homing;
			projectile.Shoot(target, originTransform.forward, 9999, originTransform);
		}
        else 
        {
            projectile.motionType = Projectile.MotionType.Linear;
            projectile.ShootTowards(originTransform.forward, projectile.speed, 9999, originTransform);
        }
			
	}

	void CreatePool() {
        
        if ( m_powerLevel >= 1 && !string.IsNullOrEmpty(m_missilesProjectileName))
		    m_missilesPoolHandler = PoolManager.CreatePool(m_missilesProjectileName, "Game/Projectiles/", 2, true);
        if ( m_powerLevel >= 2 && !string.IsNullOrEmpty(m_bombProjectileName))
            m_bombsPoolHandler = PoolManager.CreatePool(m_bombProjectileName, "Game/Projectiles/", 1, true);
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
        PetProjectile projectile = go.GetComponent<PetProjectile>();
        projectile.tier = m_tier;
        projectile.transform.position = originTransform.position;
        projectile.transform.rotation = originTransform.rotation;
        projectile.ShootAtPosition(transform.position, Vector3.down, 9999, originTransform);
        
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

}
