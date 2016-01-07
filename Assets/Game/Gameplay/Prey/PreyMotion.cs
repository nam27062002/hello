using UnityEngine;
using System.Collections;


/// <summary>
/// Prey motion. Movement and animation control layer.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PreyOrientation))]
public class PreyMotion : Initializable {
	//---------------------------------------------------------------
	// Constants
	//---------------------------------------------------------------
	protected struct Forces {
		public const int Seek = 0;
		public const int Flee = 1;
		public const int Flock = 2;
		public const int Collision = 3;

		public const int Count = 4;
	};

	private const int CollisionCheckPools = 4;

	//---------------------------------------------------------------
	// Attributes
	//---------------------------------------------------------------
[Header("Movement")]
	[SerializeField] private Range m_zOffset = new Range(-1f, 1f);
	[SerializeField] private Range m_flockAvoidRadiusRange;

[Header("Force management")]
		[CommentAttribute("Max magnitude of the steering force vector (velocity).")]
	[SerializeField] protected float m_steerForce;
		[CommentAttribute("The steering vector is divided by mass.")]
	[SerializeField] protected float m_mass = 1f;
		[CommentAttribute("Distance can reduce the effect of evasive behaviours.")]
	[SerializeField] private float m_distanceAttenuation = 5f;

[Header("Speed variations")]
	[SerializeField] private float m_maxSpeed;
	[SerializeField] private float m_maxRunSpeed;
	[SerializeField] private float m_slowingRadius;


	// --------------------------------------------------------------- //

	private FlockController m_flock; // turn into flock controller
	private float m_flockAvoidRadius;
	
	private float m_posZ;
	protected Vector2 m_position; // we move on 2D space
	protected Vector2 m_lastPosition;

	protected Vector2 m_velocity;
	protected Vector2 m_direction;

	protected Vector2[] m_steeringForces;
	protected Vector2   m_steering;

	protected float m_currentMaxSpeed;
	protected float m_currentSpeed;

	protected float m_lastSeekDistanceSqr;
		
	protected int m_groundMask;	
	protected Transform m_groundSensor;
	protected float m_collisionAvoidFactor;
	protected Vector2 m_collisionNormal;

	protected PreyOrientation m_orientation;
	protected SpawnBehaviour m_spawn;
	protected Animator m_animator;

	private int m_collisionCheckPool; // each prey will detect collisions at different frames

	//Debug
	protected Color[] m_steeringColors;
	//

	// Properties
	public Vector2 position 		{ get { return m_position; } set { m_position = value; } }
	public Vector2 direction 		{ get { return m_direction; } set { m_direction = value.normalized; m_orientation.SetDirection(m_direction); } }
	public Vector2 velocity			{ get { return m_velocity; } set { m_velocity = value; } }
	public float   speed			{ get { return m_currentSpeed; } }
	public float   slowingRadius	{ get { return m_slowingRadius; } }
	public float   lastSeekDistanceSqr { get { return m_lastSeekDistanceSqr; } }
	//
	// ----------------------------------------------------------------------------- //

	// Methods
	
	void Awake() {
		m_posZ = m_zOffset.GetRandom();
		m_groundMask = 1 << LayerMask.NameToLayer("Ground");
		m_groundSensor = transform.FindChild("ground_sensor");

		m_orientation = GetComponent<PreyOrientation>();
		m_spawn = GetComponent<SpawnBehaviour>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_steeringForces = new Vector2[Forces.Count];

		m_steeringColors = new Color[Forces.Count];
		m_steeringColors[Forces.Seek] = Color.green;
		m_steeringColors[Forces.Flee] = Color.red;
		m_steeringColors[Forces.Flock] = Color.blue;
		m_steeringColors[Forces.Collision] = Color.magenta;
	}

	private void ResetForces() {
		for (int i = 0; i < Forces.Count; i++) {
			m_steeringForces[i] = Vector2.zero;	
		}
		m_currentMaxSpeed = m_maxSpeed;
	}

	// Use this for initialization
	public override void Initialize() {		
		if (m_flock) {
			m_lastPosition = m_position = m_flock.GetTarget(m_spawn.index);
		} else if (m_groundSensor) {
			m_lastPosition = m_position = m_groundSensor.transform.position;
		} else {
			m_lastPosition = m_position = transform.position;
		}

		ResetForces();

		m_velocity = Vector2.zero;
		m_currentSpeed = 0;
		m_collisionAvoidFactor = 0;
		m_collisionNormal = Vector2.up;

		if (Random.Range(0f, 1f) < 0.5f) {
			m_direction = Vector2.right;
		} else {
			m_direction = Vector2.left;
		}
		m_orientation.SetDirection(m_direction);

		if (m_spawn != null) {
			m_collisionCheckPool = m_spawn.index % CollisionCheckPools;
		} else {
			m_collisionCheckPool = 0;
		}
	}

	void OnEnable() {		
		m_flockAvoidRadius = m_flockAvoidRadiusRange.GetRandom();
	}
	
	void OnDisable() {
		AttachFlock(null);
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

	public bool HasGroundSensor() { 
		return m_groundSensor != null;
	}

	public bool HasFlockController() {
		return m_flock != null;
	}
	
	public Vector2 GetFlockTarget() {
		if (m_spawn != null) {
			return m_flock.GetTarget(m_spawn.index);
		} else {
			return m_flock.GetTarget(0);
		}
	}

	public void Seek(Vector2 _target) {
		m_currentMaxSpeed = m_maxSpeed;
		DoSeek(_target);
	}

	public void RunTo(Vector2 _target) {
		m_currentMaxSpeed = m_maxRunSpeed;
		DoSeek(_target);
	}
	
	public void Flee(Vector2 _target) {
		m_currentMaxSpeed = m_maxRunSpeed;
		DoFlee(_target);
	}
	
	
	public void Pursuit(Vector2 _target, Vector2 _velocity, float _maxSpeed) {
		float distance = (m_position - _target).magnitude;
		float t = (distance / _maxSpeed); // amount of time in the future
		
		m_currentMaxSpeed = m_maxRunSpeed;
		
		DoSeek(_target + _velocity * t); // future position
	}
	
	public void Evade(Vector2 _target, Vector2 _velocity, float _maxSpeed) {		
		float distance = (m_position - _target).magnitude;
		float t = (distance / _maxSpeed); // amount of time in the future

		DoFlee(_target + _velocity * t); // future position
	}

	public void Stop() {
		if (direction.x < 0) {
			direction = Vector3.left;
		} else {
			direction = Vector3.right;
		}

		m_velocity = Vector3.zero;
	}

	// ------------------------------------------------------------------------------------ //
	private void ApplySteering() {
		if (m_flock != null) {
			FlockSeparation();
		}

		AvoidCollisions();

		UpdateSteering();
		UpdateVelocity();
		UpdatePosition();

		if (m_groundSensor != null) {
			UpdateCollisions();
		}

		ApplyPosition();

		ResetForces();
	}

	public Vector2 ProjectToGround(Vector2 _point) {
		RaycastHit ground;	
		Vector3 source = (Vector3)_point + Vector3.up * m_area.bounds.size.y;
		Vector3 target = source + Vector3.down * m_area.bounds.size.y * 2f;

		if (Physics.Linecast(source, target, out ground, m_groundMask)) {
			return (Vector2)ground.point;
		}

		return _point;
	}

	void FixedUpdate() {
		ApplySteering();
	}


	// ------------------------------------------------------------------------------------------------------------------------------ //
	private void DoSeek(Vector2 _target) {
		Vector2 desiredVelocity = _target - m_position;
		float distanceSqr = desiredVelocity.sqrMagnitude;
		float slowingRadiusSqr = m_slowingRadius * m_slowingRadius;
		
		desiredVelocity.Normalize();
		
		desiredVelocity *= m_currentMaxSpeed;
		if (distanceSqr < slowingRadiusSqr) {
			desiredVelocity *= (distanceSqr / slowingRadiusSqr);
		}

		// we'll keep the distance to our target for external components
		m_lastSeekDistanceSqr = distanceSqr;

		desiredVelocity -= m_velocity;
		m_steeringForces[Forces.Seek] += desiredVelocity;
	}
	
	private void DoFlee(Vector2 _from) {
		Vector2 desiredVelocity = m_position - _from;
		float distanceSqr = desiredVelocity.sqrMagnitude;

		desiredVelocity.Normalize();
		desiredVelocity *= m_currentMaxSpeed;

		if (distanceSqr > 0) {
			desiredVelocity *= (m_distanceAttenuation * m_distanceAttenuation) / distanceSqr;
		}

		desiredVelocity -= m_velocity;
		m_steeringForces[Forces.Flee] += desiredVelocity;
	}

	private void FlockSeparation() {
		Vector2 avoid = Vector2.zero;
		Vector2 direction = Vector2.zero;
		for (int i = 0; i < m_flock.entities.Length; i++) {			
			GameObject entity = m_flock.entities[i];
			
			if (entity != null && entity != gameObject) {
				direction = m_position - (Vector2)entity.transform.position;
				float distance = direction.magnitude;
				
				if (distance < m_flockAvoidRadius) {
					avoid += direction.normalized * (m_flockAvoidRadius - distance);
				}
			}
		}
		
		m_steeringForces[Forces.Flock] += avoid;
	}

	protected virtual void AvoidCollisions() {
		// 1- ray cast in the same direction where we are flying
		if (m_collisionCheckPool == Time.frameCount % CollisionCheckPools) {
			RaycastHit ground;

			float distanceCheck = 5f;
			Vector3 dir = (Vector3)m_direction;
			Debug.DrawLine(transform.position, transform.position + (dir * distanceCheck), Color.gray);

			if (Physics.Linecast(transform.position, transform.position + (dir * distanceCheck), out ground, m_groundMask)) {
				// 2- calc a big force to move away from the ground	
				m_collisionAvoidFactor = (distanceCheck / ground.distance) * 100f;
				m_collisionNormal = ground.normal;
			} else {
				m_collisionAvoidFactor *= 0.75f;
			}
		}

		if (m_collisionAvoidFactor > 1f) {
			for (int i = 0; i < Forces.Count; i++) {
				m_steeringForces[i] /= m_collisionAvoidFactor;
			}
			m_steeringForces[Forces.Collision] += (m_collisionNormal * m_collisionAvoidFactor);
		}
	}

	private void UpdateSteering() {
		for (int i = 0; i < Forces.Count; i++) {
			m_steering += m_steeringForces[i];
			Debug.DrawLine(m_position, m_position + m_steeringForces[i], m_steeringColors[i]);
		}
	}

	protected virtual void UpdateVelocity() {		
		m_steering = Vector2.ClampMagnitude(m_steering, m_steerForce);
		m_steering = m_steering / m_mass;
		
		m_velocity = Vector2.ClampMagnitude(m_velocity + m_steering, Mathf.Lerp(m_currentSpeed, m_currentMaxSpeed, 0.05f));
		
		if (m_velocity != Vector2.zero) {
			m_direction = m_velocity.normalized;
			m_orientation.SetDirection(m_direction);
		}

		m_currentSpeed = m_velocity.magnitude;
				
		Debug.DrawLine(m_position, m_position + m_velocity, Color.white);
	}

	protected virtual void UpdatePosition() {		
		m_lastPosition = m_position;
		m_position = m_position + (m_velocity * Time.fixedDeltaTime);
	}
	
	private void ApplyPosition() {
		float posZ = m_posZ;
		if (m_area != null) {
			posZ += m_area.bounds.center.z;
		}
		transform.position = new Vector3(m_position.x, m_position.y, posZ);
	}
	
	private void UpdateCollisions() {		
		// teleport to ground
		RaycastHit ground;
		Vector3 testPosition = m_groundSensor.position;

		if (Physics.Linecast(testPosition, testPosition + Vector3.down * (m_area.bounds.size.y + 5f), out ground, m_groundMask)) {
			m_position.y = ground.point.y;
			m_velocity.y = 0;
			m_currentSpeed = Mathf.Abs(m_velocity.x);
		}
	}
}
