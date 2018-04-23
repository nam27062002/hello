using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMissileLauncher : MonoBehaviour {

	public float m_fireRate;
	private float m_timer;

	DragonBoostBehaviour m_playerBoost;
	private PoolHandler m_poolHandler;
	private GameObject m_projectile;
	public List<Transform> m_firePositions = new List<Transform>();
	private int m_fireAnchorIndex = 0;
	public string m_projectileName;
	private float m_rangeSize;

	// Use this for initialization
	void Start () {
		m_playerBoost = InstanceManager.player.dragonBoostBehaviour;
		m_rangeSize = InstanceManager.player.data.scale * 10;
		CreatePool();
		Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, CreatePool);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, CreatePool);
	}
	
	// Update is called once per frame
	void Update () {
		if ( m_playerBoost.IsBoostActive() )
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
		if (m_projectile == null) {
			m_projectile = m_poolHandler.GetInstance();

			if (m_projectile != null) {
				
				Transform originTransform = m_firePositions[m_fireAnchorIndex];

				// Search target!
				Transform target = null;
				Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(originTransform.position, m_rangeSize);
				for (int i = 0; i < preys.Length && target == null; i++) 
				{
					// if (preys[i].IsBurnable(m_fireTier)) 
					{
						target = preys[i].transform;
					}
				}

				if ( target != null )
				{
					IProjectile projectile = m_projectile.GetComponent<IProjectile>();
					projectile.AttachTo(originTransform);	
					projectile.Shoot(target, transform.forward, 9999, originTransform);
					m_fireAnchorIndex++;
					if ( m_fireAnchorIndex >= m_firePositions.Count )
						m_fireAnchorIndex = 0;
				}
			} else {
				Debug.LogError("Projectile not available");
			}
			m_projectile = null;
		}
	}

	void CreatePool() {
		m_poolHandler = PoolManager.CreatePool(m_projectileName, "Game/Projectiles/", 2, true);
	}

}
