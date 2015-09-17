using UnityEngine;
using System.Collections;

public class BirdBehaviour : Initializable {

	private enum State {
		Idle = 0,
		Flee,
		Pursuit,
		Attack
	};


	[Header("Movement")]
	[SerializeField] private bool m_faceDirection;
	[SerializeField] private float m_steerForce;
	[SerializeField] private float m_mass;

	[Header("Seek Target")]
	[SerializeField] private float m_speed;
	[SerializeField] private float m_slowingRadius;

	[Header("Flee from Player")]
	[SerializeField][Range(0,2)] private float m_fleeForce;
	public float fleeForce { get { return m_fleeForce; } }
	[SerializeField] private float m_sensorMinRadius;
	public float sensorMinRadius { get { return m_sensorMinRadius; } set { m_sensorMinRadius = value; } }
	[SerializeField] private float m_sensorMaxRadius;
	public float sensorMaxRadius { get { return m_sensorMaxRadius; } set { m_sensorMaxRadius = value; } }
	[SerializeField][Range(45,360)] private float m_sensorAngle;
	public float sensorAngle { get { return m_sensorAngle; } }

	[Header("Wander")]
	[SerializeField] private float m_changeTargetTime; // amount of seconds before changing objective (position)

	[Header("Flock")]
	[SerializeField] private Range m_avoidRadiusRange;

	[Header("Attack")]
	[SerializeField] private bool m_canAttack = false;
	[SerializeField] private float m_damage;
	[SerializeField] private float m_attackTime;
	[SerializeField] private float m_attackDistance;

	//
	// Lets' Wander around and alone ;P
	//

	private float m_maxSpeed;

	private Vector2 m_position; // we move on 2D space
	private Vector2 m_velocity;
	private float m_posZ;

	private Vector2 m_direction;
	public Vector2 direction { get { return m_direction; } }

	private Vector2 m_target;

	private DragonMotion m_player; // some birds will flee from the player

	private FlockController m_flock; // turn into flock controller
	private float m_avoidRadius;

	private float m_timer;
	private float m_attackTimer;

	private State m_state;


	// Use this for initialization
	void Start() {
	
		m_player = InstanceManager.player.GetComponent<DragonMotion>();
		m_mass = Mathf.Max(1f, m_mass);				
		m_target = m_position = transform.position;
		m_direction = Vector2.right;

		m_posZ = Random.Range(-150, 150);

		m_maxSpeed = m_speed;

		m_avoidRadius = m_avoidRadiusRange.GetRandom();

		//start at random anim position
		Animator animator = transform.FindChild("view").GetComponent<Animator>();
		animator.Play("fly", 0, Random.Range(0f, 1f));

		m_state = State.Idle;
	}

	void OnDisable() {
		AttachFlock(null);
	}
	
	public override void Initialize() {
		
		m_target = m_position = transform.position;
		
		//start at random anim position
		Animator animator = transform.FindChild("view").GetComponent<Animator>();
		animator.Play("fly", 0, Random.Range(0f, 1f));
	}

	public override void SetAreaBounds(AreaBounds _area) {

		base.SetAreaBounds(_area);
		m_target = _area.RandomInside();
	}

	public void AttachFlock(FlockController _flock) {

		if (m_flock != null) {
			m_flock.Remove(gameObject);
		}

		m_flock = _flock;

		if (m_flock != null) {
			m_flock.Add(gameObject);
		}
	}

	// Update is called once per frame
	void Update() {
	
		// Update next target position
		if (m_state == State.Pursuit) {

			m_target = Pursuit();

		} else if (m_state == State.Idle) {

			if (m_flock != null) {

				m_target = m_flock.target; // flock controller guides the group of birds

			} else if (m_area != null) {
				// Update Wander behaviour
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {

					m_target = m_area.RandomInside();
					m_timer = m_changeTargetTime;
				}
			}
		}

		if (m_fleeForce > 0 || m_canAttack) {
			Vector2 vectorToPlayer = (Vector2)m_player.transform.position - m_position;
			float distanceSqr = vectorToPlayer.sqrMagnitude;

			if (m_state == State.Pursuit && distanceSqr < m_attackDistance * m_attackDistance) {

				m_target = m_position;

				m_attackTimer -= Time.deltaTime;
				if (m_attackTimer <= 0) {
					m_player.OnImpact(transform.position, m_damage, 1, null);

					m_attackTimer = m_attackTime;
				}
			
			} else {
				if (distanceSqr < m_sensorMaxRadius * m_sensorMaxRadius) {
					// check if the dragon is inside the sense zone
					if (distanceSqr < m_sensorMinRadius * m_sensorMinRadius) {
						// Check if this entity can see the player
						float angle = Vector2.Angle(m_direction, vectorToPlayer); // angle between them: from 0 to 180

						if (angle <= m_sensorAngle * 0.5f) {

							if (m_canAttack)
								m_state = State.Pursuit;
							else
								m_state = State.Flee;

						}
					} 
				} else {
					// stop running away when we are outside the Max radius
					m_state = State.Idle;
				}
			}
		}
	}

	// Move towards target and attack or flee from player dragon
	void FixedUpdate() {
	
		Vector2 steering = Vector2.zero;

		steering += Seek();

		if (m_state == State.Flee) {
			steering += Flee();
			m_maxSpeed = Mathf.Lerp(m_maxSpeed, m_speed * m_fleeForce, 0.2f);
		} else {
			m_maxSpeed = Mathf.Lerp(m_maxSpeed, m_speed, 0.1f);
		}

		if (m_flock) {
			steering += AvoidFlock();
		}
		
		steering = Vector2.ClampMagnitude(steering, m_steerForce);
		steering = steering / m_mass;

		m_velocity = Vector2.ClampMagnitude(m_velocity + steering, m_maxSpeed);
		m_position = m_position + m_velocity;

		transform.position = new Vector3(m_position.x, m_position.y, m_posZ);

		if (m_velocity != Vector2.zero) {
			m_direction = m_velocity.normalized;
		}

		UpdateOrientation();
	}
	
	private void UpdateOrientation() {

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

	private Vector2 Seek() {

		Vector2 desiredVelocity = m_target - m_position;
		float distanceSqr = desiredVelocity.sqrMagnitude;
		float slowingRadiusSqr = m_slowingRadius * m_slowingRadius;

		desiredVelocity.Normalize();

		if (distanceSqr < slowingRadiusSqr) {
			desiredVelocity *= m_speed * (distanceSqr / slowingRadiusSqr);
		} else { 
			desiredVelocity *= m_speed;
		}

		desiredVelocity -= m_velocity;

		Debug.DrawLine(m_position, m_position + desiredVelocity);
		
		return desiredVelocity;
	}

	private Vector2 Flee() {

		Vector2 desiredVelocity = m_position - (Vector2)m_player.transform.position;
		desiredVelocity = (desiredVelocity - m_velocity);
		
		Debug.DrawLine(m_position, m_position + desiredVelocity);

		return desiredVelocity;
	}

	private Vector2 AvoidFlock() {

		Vector2 avoid = Vector2.zero;
		Vector2 direction = Vector2.zero;
		for (int i = 0; i < m_flock.entities.Length; i++) {

			GameObject entity = m_flock.entities[i];

			if (entity != null && entity != gameObject) {
				direction = m_position - (Vector2)entity.transform.position;
				float distance = direction.magnitude;

				if (distance < m_avoidRadius) {
					avoid += direction.normalized * (m_avoidRadius - distance);
				}
			}
		}

		Debug.DrawLine(m_position, m_position + avoid);

		return avoid;
	}

	private Vector2 Pursuit() {

		float distance = (m_position - (Vector2)m_player.transform.position).magnitude;
		float t = 2f * (distance / m_player.GetMaxSpeed()); // amount of time in the future

		Vector2 futurePosition = m_player.transform.position + m_player.GetVelocity() * t;

		return futurePosition;
	}

	//-----------------------------------------------
	// Debug
	//-----------------------------------------------
	void OnDrawGizmosSelected() {
		
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(m_target, 10);
	}
}
