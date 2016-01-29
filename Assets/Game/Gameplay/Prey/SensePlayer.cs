using UnityEngine;
using System.Collections;

[AddComponentMenu("Behaviour/Prey/Sense Player")]
public class SensePlayer : MonoBehaviour {
	
	[SerializeField] private float m_sensorMinRadius;
	public float sensorMinRadius { get { return m_sensorMinRadius; } set { m_sensorMinRadius = value; } }
	public float sensorMinRadiusSqr { get { return m_sensorMinRadius * m_sensorMinRadius; } }

	[SerializeField] private float m_sensorMaxRadius;
	public float sensorMaxRadius { get { return m_sensorMaxRadius; } set { m_sensorMaxRadius = value; } }
	public float sensorMaxRadiusSqr { get { return m_sensorMaxRadius * m_sensorMaxRadius; } }

	[SerializeField][Range(45,360)] private float m_sensorAngle;
	public float sensorAngle { get { return m_sensorAngle; } }

	[SerializeField][Range(0,360)] private float m_sensorAngleOffset;
	public float sensorAngleOffset { get { return m_sensorAngleOffset; } }


	private bool m_alert;
	public bool alert { get { return m_alert; } }

	private bool m_isInsideMinArea;
	public bool isInsideMinArea { get { return m_isInsideMinArea; } }

	private bool m_isInsideMaxArea;
	public bool isInsideMaxArea { get { return m_isInsideMaxArea; } }

	private float m_distanceSqr;
	public float distanceSqr { get { return m_distanceSqr; } }

	private PreyMotion m_motion;
	private DragonPlayer m_dragon;
	private Transform m_dragonMouth; 

	private float m_dragonRadiusSqr;

	private float m_shutdownTime;

	void Awake() {
		m_motion = GetComponent<PreyMotion>();
	}

	void Start() {
		m_dragon = InstanceManager.player;
		m_dragonMouth = m_dragon.GetComponent<DragonMotion>().tongue;

		m_dragonRadiusSqr = 0;
		Collider[] colliders = InstanceManager.player.GetComponents<Collider>();
		for (int i = 0; i < colliders.Length; i++) {
			if (m_dragonRadiusSqr > colliders[i].bounds.extents.x) {
				m_dragonRadiusSqr = colliders[i].bounds.extents.x;
			}
		}
		m_dragonRadiusSqr *= m_dragonRadiusSqr;
		
		m_alert = false;
		m_isInsideMinArea = false;
		m_isInsideMaxArea = false;
		m_distanceSqr = 0;

		m_shutdownTime = 0;
	}

	void OnEnable() {
		m_shutdownTime = 0;
	}

	void OnDisable() {		
		m_alert = false;
		m_isInsideMinArea = false;
		m_isInsideMaxArea = false;
	}

	public void Shutdown(float _time) {
		m_shutdownTime = _time;
		OnDisable();
	}

	// Update is called once per frame
	void Update () {
		if (m_shutdownTime > 0) {
			m_shutdownTime -= Time.deltaTime;
			if (m_shutdownTime <= 0) {
				m_shutdownTime = 0;
			}
		} else if (m_dragon.IsAlive()) {
			// we have too much erro if we only sense the dragon when it is inside the spawn area, it can be too small
			Vector2 vectorToPlayer = (Vector2)m_dragonMouth.position - m_motion.position;
			m_distanceSqr = vectorToPlayer.sqrMagnitude - m_dragonRadiusSqr;

			if (m_distanceSqr < m_sensorMaxRadius * m_sensorMaxRadius) {
				// check if the dragon is inside the sense zone
				if (m_distanceSqr < m_sensorMinRadius * m_sensorMinRadius) {
					// Check if this entity can see the player
					if (sensorAngle == 360) {
						m_alert = true;
						m_isInsideMinArea = true;
					} else {
						Vector2 direction = (m_motion.direction.x < 0)? Vector2.left : Vector2.right;
						float angle = Vector2.Angle(direction, vectorToPlayer); // angle between them: from 0 to 180

						Vector3 cross = Vector3.Cross(m_motion.direction, vectorToPlayer);					
						if (cross.z > 0) {
							angle = 306 - angle;
						}

						float sensorAngleFrom = sensorAngleOffset - (sensorAngle * 0.5f);
						if (sensorAngleFrom < 0) {
							sensorAngleFrom += 360;
						}

						float sensorAngleTo = sensorAngleOffset + (sensorAngle * 0.5f);
						if (sensorAngleTo > 360) {
							sensorAngleTo -= 360;
						}

						if (angle >= sensorAngleFrom || angle <= sensorAngleTo) {
							m_alert = true;
							m_isInsideMinArea = true;
						} else {
							m_isInsideMinArea = false;
						}
					}
				} else {
					m_isInsideMinArea = false;
				}
				m_isInsideMaxArea = true;
			} else {
				m_alert = false;
				m_isInsideMinArea = false;
				m_isInsideMaxArea = false;
			}
		} else {
			m_alert = false;
			m_isInsideMinArea = false;
			m_isInsideMaxArea = false;
		}
	}
}
