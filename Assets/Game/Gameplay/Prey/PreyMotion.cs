using UnityEngine;
using System.Collections;


/// <summary>
/// Prey motion. Movement and animation control layer.
/// </summary>
[DisallowMultipleComponent]
public class PreyMotion : Initializable {

	// Attributes	
	[SerializeField] private bool m_faceDirection;	
	[SerializeField] private float m_steerForce;	
	[SerializeField] private float m_maxSpeed;
	[SerializeField] private float m_mass;
	[SerializeField] private float m_slowingRadius;
	[SerializeField] private Range m_flockAvoidRadiusRange;

	private FlockController m_flock; // turn into flock controller
	private float m_flockAvoidRadius;
	
	private float m_posZ;
	private Vector2 m_position; // we move on 2D space
	private Vector2 m_lastPosition;
	private Vector2 m_velocity;
	private Vector2 m_direction;
	private Vector2 m_steering;
	private float   m_currentSpeed;
	
	private int m_groundMask;	
	private Transform m_groundSensor;

	//Debug
	Color m_seekColor 		= Color.green;
	Color m_fleeColor 		= Color.red;
	Color m_flockColor 		= Color.yellow;
	Color m_velocityColor	= Color.white;
	//

	// Properties
	public Vector2 position 	{ get { return m_position; } }
	public Vector2 direction 	{ get { return m_direction; } }
	public Vector2 velocity		{ get { return m_velocity; } }
	public float   speed		{ get { return m_currentSpeed; } }

	// ----------------------------------------------------------------------------- //

	// Methods
	
	void Awake() {
		m_posZ = Random.Range(-1, 1);
		m_groundMask = 1 << LayerMask.NameToLayer("Ground");
		m_groundSensor = transform.FindChild("ground_sensor");
	}

	// Use this for initialization
	public override void Initialize() {		
		if (m_groundSensor) {
			m_lastPosition = m_position = m_groundSensor.transform.position;
		} else {
			m_lastPosition = m_position = transform.position;
		}
		
		m_steering = Vector3.zero;
		m_velocity = Vector3.zero;
		m_direction = (Random.Range(0f, 1f) < 0.5f)? Vector3.right : Vector3.left;
		m_currentSpeed = 0;
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
		return m_flock.target;
	}

	public void Seek(Vector2 _target) {
		Vector2 desiredVelocity = _target - m_position;
		float distanceSqr = desiredVelocity.sqrMagnitude;
		float slowingRadiusSqr = m_slowingRadius * m_slowingRadius;
		
		desiredVelocity.Normalize();
				
		desiredVelocity *= m_maxSpeed;
		if (distanceSqr < slowingRadiusSqr) {
			desiredVelocity *= (distanceSqr / slowingRadiusSqr);
		}
		
		desiredVelocity -= m_velocity;		
		m_steering += desiredVelocity;
		
		Debug.DrawLine(m_position, m_position + desiredVelocity, m_seekColor);
	}
	
	public void Flee(Vector2 _from) {
		Vector2 desiredVelocity = m_position - _from;
		desiredVelocity = (desiredVelocity - m_velocity);		
		m_steering += desiredVelocity;		
		
		Debug.DrawLine(m_position, m_position + desiredVelocity, m_fleeColor);
	}

	public void Pursuit(Vector2 _target, Vector2 _velocity, float _maxSpeed) {
		float distance = (m_position - _target).magnitude;
		float t = 2f * (distance / _maxSpeed); // amount of time in the future

		Seek(_target + _velocity * t); // future position
	}

	public void Evade(Vector2 _target, Vector2 _velocity, float _maxSpeed) {		
		float distance = (m_position - _target).magnitude;
		float t = 2f * (distance / _maxSpeed); // amount of time in the future
				
		Flee(_target + _velocity * t); // future position
	}

	public void ApplySteering() {

		if (m_flock != null) {
			FlockSeparation();
		}

		UpdateVelocity();
		UpdatePosition();

		if (m_groundSensor != null) {
			UpdateCollisions();
		}

		UpdateOrientation();		
		ApplyPosition();

		m_steering = Vector2.zero;
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

	// ------------------------------------------------------------------------------------------------------------------------------ //

	private  void FlockSeparation() {
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
		
		Debug.DrawLine(m_position, m_position + avoid, m_flockColor);
		
		m_steering += avoid;
	}

	private void UpdateVelocity() {
		
		m_steering = Vector2.ClampMagnitude(m_steering, m_steerForce);
		m_steering = m_steering / m_mass;
		
		m_velocity = Vector2.ClampMagnitude(m_velocity + m_steering, m_maxSpeed);
		
		if (m_velocity != Vector2.zero) {
			m_direction = m_velocity.normalized;
		}

		m_currentSpeed = m_velocity.magnitude;
				
		Debug.DrawLine(m_position, m_position + m_velocity, m_velocityColor);
	}

	private void UpdatePosition() {
		
		m_lastPosition = m_position;
		m_position = m_position + (m_velocity * Time.fixedDeltaTime);
	}
	
	private void ApplyPosition() {
		transform.position = new Vector3(m_position.x, m_position.y, m_area.bounds.center.z + m_posZ);
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
	
	private void UpdateCollisions() {
		
		// teleport to ground
		RaycastHit ground;
		Vector3 testPosition = m_lastPosition + Vector2.up * 5f;

		if (Physics.Linecast(testPosition, testPosition + Vector3.down * 15f, out ground, m_groundMask)) {
			m_position.y = ground.point.y;
			m_position.y += (transform.position.y - m_groundSensor.transform.position.y);
			m_velocity.y = 0;
			m_currentSpeed = Mathf.Abs(m_velocity.x);
		}
	}
}
