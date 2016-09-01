using UnityEngine;
using System.Collections;

public class ProjectileMotion : Initializable, MotionInterface 
{

	public enum Type
	{
		Arrow,
		Missile,
		Spear
	};

	public Type m_moveType;
	public float m_arrowSpeed = 1;
	public float m_arrowMaxDuration = 5;
	private float m_duration = 0;
	Vector3 m_direction;
	Vector3 m_forceVector;
	Vector3 m_position;

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch(m_moveType)
		{
			case Type.Arrow:
			{
				m_position = m_position + m_direction * m_arrowSpeed * Time.deltaTime;
				m_duration -= Time.deltaTime;
				if ( m_duration <= 0 )
				{
					ProjectileBehaviour pb = GetComponent<ProjectileBehaviour>();
					if (pb != null)
						pb.Explode(false);
				}
			}break;
			case Type.Spear:
			{
				m_forceVector += Time.deltaTime * Physics.gravity;
				m_position = m_position + m_forceVector * Time.deltaTime;

				float angle = Mathf.Atan2(m_forceVector.y, m_forceVector.x) * Mathf.Rad2Deg;
				Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(-angle, Vector3.left);
				Vector3 eulerRot = targetRotation.eulerAngles;		
				transform.rotation = Quaternion.Euler(eulerRot);

			}break;
		}
		transform.position = m_position;
	}

	public void Shoot( Vector3 _target )
	{
		_target.z = 0;
		switch( m_moveType )
		{
			case Type.Spear:
			{
				float randomSize = 2.5f;
				_target.x += Random.Range( -randomSize, randomSize );
				_target.y += Random.Range( 0, randomSize );

				m_position = transform.position;
				m_direction = _target - transform.position;
				float force = m_direction.magnitude * 2;
				m_direction.Normalize();
				m_forceVector = m_direction * Mathf.Min( force , 100 );
			}break;

			case Type.Arrow:
			{
				m_position = transform.position;
				m_direction = _target - transform.position;
				m_direction.Normalize();
				m_duration = m_arrowMaxDuration;

				Vector3 newDir = Vector3.RotateTowards(Vector3.forward, -m_direction, 2f*Mathf.PI, 0.0f);
				transform.rotation = Quaternion.AngleAxis(90f, m_direction) * Quaternion.LookRotation(newDir);
			}break;
		}
	}

	public override void Initialize()
	{

	}

	public Vector3 position 
	{ 
		get
		{
			return m_position;	
		}
		set
		{
			m_position = value;
		}
	}
	public Vector3 direction 
	{ 
		get
		{
			return m_direction;
		}
	}

	public Vector3 velocity 
	{ 	
		get
		{
			return m_forceVector;
		}
	}

	public Vector3 angularVelocity
	{
		get
		{
			return Vector3.zero;
		}
	}

	public float maxSpeed 
	{ 
		get
		{
			return 0;
		}
	}

}
