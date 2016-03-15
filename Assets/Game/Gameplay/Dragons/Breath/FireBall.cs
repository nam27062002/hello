using UnityEngine;
using System.Collections;

public class FireBall : MonoBehaviour 
{
	Vector3 m_direction;
	float m_damage;
	CircleArea2D m_area;
	float m_timer;

	public float m_speed;
	public float m_maxTime;
	public GameObject m_explosionParticle;

	// Use this for initialization
	void Start () 
	{
		m_area = GetComponent<CircleArea2D>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		m_timer += Time.deltaTime;
		if ( m_timer >= m_maxTime )
		{
			Explode();
		}
		else
		{
			transform.position += m_direction * m_speed * Time.deltaTime;
			Entity[] preys = EntityManager.instance.GetEntitiesInRange2D( m_area.center, m_area.radius);
			/*
			for (int i = 0; i < preys.Length; i++) {
				InflammableBehaviour entity =  preys[i].GetComponent<InflammableBehaviour>();
				if (entity != null) 
				{
					entity.Burn(m_damage * Time.deltaTime, transform);
				}
			}
			*/
			if ( preys.Length > 0 )
				Explode();
		}
	}

	public void Shoot( Vector3 _direction, float _damage)
	{
		m_direction = _direction;
		m_damage = _damage;
		m_timer = 0;
	}

	void OnCollisionEnter( Collision _collision )
	{
		// if the collision is ground -> Explode!!
		if ( _collision.gameObject.layer == LayerMask.NameToLayer("Ground") )
		{
			Explode();
		}
	}

	void OnTriggerEnter( Collider _other)
	{
		if ( _other.gameObject.layer == LayerMask.NameToLayer("Ground"))
			Explode();
	}

	void Explode()
	{
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D( m_area.center, m_area.radius * 3);
		for (int i = 0; i < preys.Length; i++) 
		{
			InflammableBehaviour entity =  preys[i].GetComponent<InflammableBehaviour>();
			if (entity != null) 
			{
				entity.Burn(m_damage, transform);
			}
		}

		ParticleManager.Spawn("PF_Explosion", transform.position);

		PoolManager.ReturnInstance( gameObject );
	}
}
