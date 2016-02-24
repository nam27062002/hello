using UnityEngine;
using System.Collections;

public class ProjectileMotion : Initializable, MotionInterface 
{

	public enum Type
	{
		Arrow,
		Missile
	};

	public Type m_moveType;
	protected Orientation m_orientation;
	Vector3 m_direction;
	Vector3 m_forceVector;
	Vector3 m_position;

	// Use this for initialization
	void Start () 
	{
		m_orientation = GetComponent<Orientation>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch(m_moveType)
		{
			case Type.Arrow:
			{
				m_forceVector += Time.deltaTime * Physics.gravity;
				m_position = m_position + m_forceVector * Time.deltaTime;
				m_orientation.SetDirection( m_forceVector );

			}break;
		}
		transform.position = m_position;
	}

	public void Shoot( Vector3 _target )
	{
		switch( m_moveType )
		{
			case Type.Arrow:
			{
				m_position = transform.position;
				m_direction = _target - transform.position;
				float force = m_direction.magnitude * 2;
				m_direction.Normalize();
				m_forceVector = m_direction * Mathf.Min( force , 100 );
			}break;
		}
	}

	public override void Initialize()
	{

	}

	public Vector2 position 
	{ 
		get
		{
			return m_position;	
		}
	}
	public Vector2 direction 
	{ 
		get
		{
			return m_direction;
		}
	}

	public Vector2 velocity 
	{ 	
		get
		{
			return m_forceVector;
		}
	}

	public float maxSpeed 
	{ 
		get
		{
			return 0;
		}
	}

	public void SetSpeedMultiplier(float _value)
	{
		
	}
}
