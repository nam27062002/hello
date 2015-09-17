using UnityEngine;
using System.Collections;

public class FlockBehaviour : MonoBehaviour {
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[HideInInspector]
	public FlockController flock;

	[Header("Individual")]
	public float m_speed = 300f;
	public bool m_trackGround = true;
	public bool m_faceDirection = false;

	[Header("Flock")]
	public float m_followFactor = 0.5f;
	public float m_avoidFactor = 0.4f;
	public float m_avoidDistance = 50f;

	[Header("Flee")]
	public float m_fleeFactor = 0f;
	public float m_sensorAngleDeg = 360f;
	public float m_sensorDistance = 350f;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Vector3 m_impulse;
	private Vector3 m_pos;
	private Vector3 m_scale;
	private Quaternion m_rotation;

	private Transform m_player;
	private int m_frame;
	private int m_groundMask;
	

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {

	/*	m_pos = transform.position;
		m_scale = transform.localScale;
		m_rotation = transform.localRotation;

		m_frame = 0;
		m_player = GameObject.Find ("Player").transform;

		m_groundMask = 1 << LayerMask.NameToLayer("Ground");*/
	}
	
	void Update () {

	/*	if (flock != null) {

			// Follow the target
			Vector3 follow = flock.followPos - m_pos;
			follow.Normalize();
			follow *= m_speed * Time.deltaTime;

			// Avoid other entities from the same flock
			Vector3 avoid = Vector3.zero;
			Vector3 dist = Vector3.zero;
			foreach (GameObject obj in flock.entities) {

				if (obj != null && obj != this.gameObject) {

					dist = m_pos - obj.transform.position;
					float m = dist.magnitude;

					if (m < m_avoidDistance)
						avoid += dist.normalized * (m_avoidDistance - m);
				}
			}

			avoid.Normalize();
			avoid *= m_speed * Time.deltaTime;

			// Flee from player
			Vector3 flee = Vector3.zero;
			if (m_fleeFactor > 0) {
				flee = m_pos - m_player.position;
				float d = flee.magnitude;
				if (d < m_sensorDistance) {					
					
					// Check if this entity can see the player
					float dot = Vector3.Dot(m_impulse, m_player.position - m_pos);

					// normalize angle to [-1, 1] 
					float angle = m_sensorAngleDeg * 0.5f;
					angle = -((angle - 90f) / 90f);

					if (dot >= angle) {
						flee = flee.normalized * (m_speed - (d * 0.5f)) * m_fleeFactor * Time.deltaTime;
					} else {
						flee = Vector3.zero;
					}
				} else {
					flee = Vector3.zero;
				}
			}

			// Calculate impulse
			m_impulse = Vector3.Lerp(m_impulse, follow * m_followFactor + avoid * m_avoidFactor + flee * m_fleeFactor, 0.4f);

			// Move 
			m_pos += m_impulse;

			m_frame++;
			Vector3 dir = m_impulse.normalized;
			if (m_trackGround && m_frame > 2) {

				// Don't go into the ground
				RaycastHit ground;
				if (Physics.Linecast(m_pos, m_pos + dir * 50f, out ground, m_groundMask)) {
					m_pos = new Vector3(ground.point.x, ground.point.y, 0f) - dir * 50f;
				} else {
					m_frame = 0;
				}
			}

			m_pos.z = 0f;
			transform.position = m_pos;

			if (m_faceDirection) {

				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			} else {
				// Rotate so it faces the right direction (replaces 2D sprite flip)
				float fRotationSpeed = 2f;	// [AOC] Deg/sec?
				float fAngleY = 0f;

				if (m_impulse.x < 0f) {
					fAngleY = 180;
				}

				Quaternion q = Quaternion.Euler(0, fAngleY, 0);
				transform.localRotation = Quaternion.Slerp(transform.localRotation, q, Time.deltaTime * fRotationSpeed);
			}
		}*/
	}

	public void OnSpawn(Bounds bounds){

	/*	m_pos = bounds.center;
		m_pos.x  += Random.Range (-300f,300f);
		m_pos.y  += Random.Range (-300f,300f);
		m_pos.z = 0f;

		transform.position = m_pos;
		transform.localScale = m_scale;
		transform.localRotation = m_rotation;
		m_impulse = Vector3.zero;

		GetComponent<GameEntity>().RestoreHealth();*/
	}
}
