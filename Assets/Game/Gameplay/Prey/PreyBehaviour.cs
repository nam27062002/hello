﻿using UnityEngine;

[DisallowMultipleComponent]
public class PreyBehaviour : Initializable {
	
	[SerializeField] private bool m_faceDirection;

	[SerializeField] private float m_steerForce;

	[SerializeField] protected float m_maxSpeed;
	public float maxSpeed { get { return m_maxSpeed; } }

	[SerializeField] protected float m_mass;
	public float mass { get { return m_mass; } }

	protected Vector2 m_positionLast;
	protected Vector2 m_position; // we move on 2D space
	public Vector2 position { get { return m_position; } }

	protected Vector2 m_velocity;
	public Vector2 velocity { get { return m_velocity; } }

	protected Vector2 m_direction;
	public Vector2 direction { get { return m_direction; } }


	protected bool playerDetected { get { return m_sensor && m_sensor.alert; } }


	private float m_posZ;

	protected Seek 		m_seek;
	protected Flee 		m_flee;
	protected Flock 	m_flock;
	protected Wander	m_wander;
	protected Pursuit 	m_pursuit;
	protected SensePlayer m_sensor;
	
	protected Animator m_animator;

	void Awake() {
		
		m_posZ = Random.Range(-150, 150);

		m_seek = GetComponent<Seek>();
		m_flee = GetComponent<Flee>();
		m_flock = GetComponent<Flock>();
		m_wander = GetComponent<Wander>();
		m_pursuit = GetComponent<Pursuit>();

		m_sensor = GetComponent<SensePlayer>();

		m_animator = transform.FindChild("view").GetComponent<Animator>();
	}
	
	
	public override void Initialize() {
		
		m_positionLast = m_position = transform.position;

		//start at random anim position - Move to Bird Behaviour
		//Animator animator = transform.FindChild("view").GetComponent<Animator>();
		//animator.Play("fly", 0, Random.Range(0f, 1f));
	}


	/// <summary>
	/// Update at fixed time intervals. It also performs a default movement behaviour based on the available components.
	/// Override it for more complex behaviours.
	/// </summary>
	protected virtual void FixedUpdate() {

		Vector2 steering = Vector2.zero;

		if (m_seek) {

			Vector2 target = Vector2.zero;

			if (m_pursuit && playerDetected) {

				DragonPlayer player = InstanceManager.player;
				target = m_pursuit.GetTarget(player.transform.position, player.GetVelocity(), player.GetMaxSpeed());

			} else if (m_flock && m_flock.HasController()) {

				target = m_flock.GetTarget();

			} else if (m_wander) {

				target = m_wander.GetTarget();
			}

			steering += m_seek.GetForce(target);
		}

		if (playerDetected) {

			if (m_flee) {

				DragonPlayer player = InstanceManager.player;
				steering += m_flee.GetForce(player.transform.position);
			} 
		}

		if (m_flock && m_flock.HasController()) {

			steering += m_flock.GetForce();
		}
				
		UpdateVelocity(steering);
		UpdatePosition();
		UpdateOrientation();

		ApplyPosition();
	}

	protected void UpdateVelocity(Vector2 _steering) {

		_steering = Vector2.ClampMagnitude(_steering, m_steerForce);
		_steering = _steering / m_mass;
		
		m_velocity = Vector2.ClampMagnitude(m_velocity + _steering, m_maxSpeed);

		if (m_velocity != Vector2.zero) {
			m_direction = m_velocity.normalized;
		}
	}

	protected void UpdatePosition() {

		m_positionLast = m_position;
		m_position = m_position + m_velocity;
	}

	protected void ApplyPosition() {
		transform.position = new Vector3(m_position.x, m_position.y, m_posZ);
	}

	protected void UpdateOrientation() {
		
		float rotationSpeed = 2f;	// [AOC] Deg/sec?
		Quaternion targetDir;
		
		if (m_faceDirection && m_velocity.sqrMagnitude > 0.1f) {
			// rotate the model so it can fully face the current direction
			float angle = Mathf.Atan2(m_direction.y, m_direction.x) * Mathf.Rad2Deg;			
			targetDir = Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.AngleAxis(-angle, Vector3.left);		
			
		} else {
			// Rotate so it faces the right direction (replaces 2D sprite flip)
			float angleY = 0f;
			
			if (m_direction.x < 0f) {
				angleY = 180f;
			}
			
			targetDir = Quaternion.Euler(0, angleY, 0);
		}
		
		transform.localRotation = Quaternion.Slerp(transform.localRotation, targetDir, Time.deltaTime * rotationSpeed);
	}
}
