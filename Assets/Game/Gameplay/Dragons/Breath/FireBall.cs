using UnityEngine;
using System.Collections;

public class FireBall : MonoBehaviour 
{
	Vector3 m_direction;
	CircleArea2D m_area;
	float m_timer;

	public float m_speed;
	public float m_maxTime;
	public string m_explosionParticle;
	private DragonBreathBehaviour m_breath;
	private ParticleSystem[] m_PSbreath;

	// Use this for initialization
	void Start () 
	{
		m_area = GetComponent<CircleArea2D>();
		m_PSbreath = GetComponentsInChildren<ParticleSystem>();
	}

	void PlayBreath(bool value)
	{
		foreach (ParticleSystem ps in m_PSbreath)
		{
			if (value)
			{
				ps.Play();
			}
			else
			{
				ps.Stop();
			}
		}

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

			if ( preys.Length > 0 )
				Explode();
		}
	}

	public void SetBreath( DragonBreathBehaviour _breath )
	{
		m_breath = _breath;
	}

	public void Shoot( Vector3 _direction)
	{
		m_direction = _direction;
		m_timer = 0;
		PlayBreath (true);
	}

	void OnCollisionEnter( Collision _collision )
	{
		// if the collision is ground -> Explode!!
		if(((1 << _collision.gameObject.layer) & LayerMask.GetMask("Ground", "Water", "GroundVisible")) > 0)
			Explode();

//        Debug.Log("FireBall.OnCollisionEnter");
	}

	void OnTriggerEnter( Collider _other)
	{
		if(((1 << _other.gameObject.layer) & LayerMask.GetMask("Ground", "Water", "GroundVisible")) > 0)
			Explode();

//        Debug.Log("FireBall.OnTriggerEnter");
    }

    void Explode()
	{
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_area.center, m_area.radius * 3);
		for (int i = 0; i < preys.Length; i++) 
		{
			//if (CanBurn(preys[i]) || m_type == Type.Super) 
			{
				AI.IMachine machine =  preys[i].machine;
				if (machine != null) {
					machine.Burn(transform);
				}
			}
			/*
			if (!burned){
				// Show I cannot burn this entity!
			}
			*/
		}

        m_speed = 0.0f;
//		ParticleManager.Spawn(m_explosionParticle, transform.position);

//        PoolManager.ReturnInstance(gameObject);
		PlayBreath(false);
        StartCoroutine(DisableInTime());

	}
    IEnumerator DisableInTime()
    {
		yield return new WaitForSeconds(1.0f);
        PoolManager.ReturnInstance(gameObject);
    }
}
