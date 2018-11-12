using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class DragonSonicDestroyArea : MonoBehaviour {

	private CircleArea2D m_circle;
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;
	public DragonTier m_tier = DragonTier.TIER_4;
	public IEntity.Type m_type = IEntity.Type.PLAYER;
	public float m_fireBoostMultiplier = 2;
	float m_extraRadius;
	DragonMotion m_motion;
	private float m_originalRadius;
	private bool m_active = false;
	protected Transform m_transform;
	protected DragonPlayer m_player;
	protected bool m_fire = false;
	protected DragonBreathBehaviour.Type m_fireType;

	// Use this for initialization
	void Start () {
		m_circle = GetComponent<CircleArea2D>();
		m_originalRadius = m_circle.radius;
		m_player = InstanceManager.player;
		m_motion = m_player.dragonMotion;
		m_extraRadius = 1;
		m_tier = m_player.data.tier;
		m_transform = transform;

		Messenger.AddListener<bool, DragonBreathBehaviour.Type> (MessengerEvents.FURY_RUSH_TOGGLED, OnFuryRushToggled);

	}

	void OnDestroy()
	{
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(MessengerEvents.FURY_RUSH_TOGGLED, OnFuryRushToggled);
	}
	
	// Update is called once per frame
	void Update () {

		if ( m_motion.state == DragonMotion.State.Extra_2 || (m_fire && m_motion.state == DragonMotion.State.Extra_1))
		{
			if (!m_active)
			{
				m_active = true;
			}
			m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2)m_circle.center, m_circle.radius, m_checkEntities);
			for (int i = 0; i < m_numCheckEntities; i++) 
			{
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
		else
		{
			if (m_active)
			{
				m_active = false;
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
		}
	}
}
