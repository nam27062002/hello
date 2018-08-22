using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMissileLauncher : MonoBehaviour {

	public float m_fireRate;
	private float m_timer;

	DragonBoostBehaviour m_playerBoost;
	DragonMotion m_playerMotion;
	private PoolHandler m_poolHandler;
	public List<Transform> m_firePositions = new List<Transform>();
	private int m_fireAnchorIndex = 0;
	public string m_projectileName;
	private float m_rangeSize;
	public float m_dragonRangeMultiplier = 10;
	// Use this for initialization
	void Start () {
		m_playerBoost = InstanceManager.player.dragonBoostBehaviour;
		m_playerMotion = InstanceManager.player.dragonMotion;
		m_rangeSize = InstanceManager.player.data.scale * m_dragonRangeMultiplier;
		CreatePool();
		Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, CreatePool);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, CreatePool);
	}
	
	// Update is called once per frame
	void Update () {
		if ( m_playerBoost.IsBoostActive() && !m_playerMotion.IsInsideWater() )
		{
			m_timer -= Time.deltaTime;
			if ( m_timer <= 0 )
			{
				Fire();
				m_timer = m_fireRate;
			}
		}
	}

	private void Fire()
	{
		// Fire!!
		Transform originTransform = m_firePositions[m_fireAnchorIndex];

		// Search target!
		Transform alternateTarget = null;
		Transform target = null;
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(originTransform.position, m_rangeSize);
		for (int i = 0; i < preys.Length && target == null; i++) 
		{
			// if (preys[i].IsBurnable(m_fireTier)) 
			{
				if ( alternateTarget == null ){
					alternateTarget = preys[i].transform;
				}else if ( Vector3.Dot( originTransform.forward, preys[i].transform.position - originTransform.position) > 0 ){
					target = preys[i].transform;
				}
			}
		}
		if ( target == null && alternateTarget != null )
			target = alternateTarget;

		if ( target != null )
		{
			GameObject go = m_poolHandler.GetInstance();
			IProjectile projectile = go.GetComponent<IProjectile>();
			projectile.AttachTo(originTransform);	
			projectile.Shoot(target, transform.forward, 9999, originTransform);
			m_fireAnchorIndex++;
			if ( m_fireAnchorIndex >= m_firePositions.Count )
				m_fireAnchorIndex = 0;
		}
			
	}

	void CreatePool() {
		m_poolHandler = PoolManager.CreatePool(m_projectileName, "Game/Projectiles/", 2, true);
	}

}
