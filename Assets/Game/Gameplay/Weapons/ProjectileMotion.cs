using UnityEngine;
using System.Collections;

public class ProjectileMotion : MonoBehaviour {
	public enum Type {
		Arrow,
		Missile,
		Spear,
		Bomb,
		FallingMine,
		PositionMissile	// Goes to position and explodes there
	};

	public Type m_moveType;
	public float m_arrowSpeed = 1;
	public float m_arrowMaxDuration = 5;
	private float m_duration = 0;
	Vector3 m_direction;
	Vector3 m_forceVector;
	Vector3 m_position;
	Vector3 m_target;
	public Vector3 target{
		get{ return m_target; }
		set{ m_target = value; }
	}
	Vector3 m_startPosition;

	
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
			case Type.FallingMine:
			{
			}break;
			case Type.PositionMissile:
			{
				m_duration -= Time.deltaTime;
				m_position = Vector3.Lerp( m_target, m_startPosition, m_duration / m_arrowMaxDuration);
				if ( m_duration <= 0 )
				{
					IProjectile pb = GetComponent<IProjectile>();
					if (pb != null)
						pb.Explode(false);
				}
			}break;
			case Type.Missile:
			{
				m_duration -= Time.deltaTime;
				if ( m_duration <= 0 )
				{
					IProjectile pb = GetComponent<IProjectile>();
					if (pb != null)
						pb.Explode(false);
				}
				else
				{
					m_direction = Vector3.Lerp(m_direction, (m_target - m_position).normalized, Time.deltaTime * 2);
					m_position += m_direction * m_arrowSpeed * Time.deltaTime;
				}
			}break;
		}
		transform.position = m_position;
	}

	void FixedUpdate()
	{
		switch( m_moveType )
		{
			case Type.FallingMine:
			{
				m_forceVector += Physics.gravity * Time.fixedDeltaTime;
				m_position += m_forceVector * Time.deltaTime;
			}break;
		}
	}

	public void Shoot( Vector3 _target )
	{
		_target.z = 0;
		m_startPosition = _target;
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
			case Type.FallingMine:
			{
				m_position = transform.position;
				m_forceVector = Vector3.zero;
			}break;
			case Type.PositionMissile:
			{
				m_position = transform.position;
				m_startPosition = m_position;
				m_target = _target;
				m_duration = (m_target - m_startPosition).magnitude / m_arrowSpeed;
				if ( m_duration <= 0 )
					m_duration = 0.1f;
				m_arrowMaxDuration = m_duration;
			}break;
			case Type.Missile:
			{
				m_position = transform.position;
				m_startPosition = m_position;
				m_target = _target;
				m_direction = transform.forward;
				m_duration = m_arrowMaxDuration;
			}break;
		}
	}
}
