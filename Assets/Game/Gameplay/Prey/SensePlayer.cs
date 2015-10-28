using UnityEngine;
using System.Collections;

[AddComponentMenu("Behaviour/Prey/Sense Player")]
public class SensePlayer : MonoBehaviour {
	
	[SerializeField] private float m_sensorMinRadius;
	public float sensorMinRadius { get { return m_sensorMinRadius; } set { m_sensorMinRadius = value; } }

	[SerializeField] private float m_sensorMaxRadius;
	public float sensorMaxRadius { get { return m_sensorMaxRadius; } set { m_sensorMaxRadius = value; } }

	[SerializeField][Range(45,360)] private float m_sensorAngle;
	public float sensorAngle { get { return m_sensorAngle; } }

	[SerializeField][Range(0,360)] private float m_sensorAngleOffset;
	public float sensorAngleOffset { get { return m_sensorAngleOffset; } }


	private bool m_alert;
	public bool alert { get { return m_alert; } }

	private float m_distanceSqr;
	public float distanceSqr { get { return m_distanceSqr; } }

	private PreyMotion m_motion;
	private Transform m_dragonMouth; 

	void Awake() {
		m_motion = GetComponent<PreyMotion>();
	}

	void OnEnable() {
		m_dragonMouth = InstanceManager.player.GetComponent<DragonMotion>().mouth;

		m_alert = false;
		m_distanceSqr = 0;
	}

	// Update is called once per frame
	void Update () {
		if (m_motion.area == null || m_motion.area.Contains(m_dragonMouth.position)) {

			Vector2 vectorToPlayer = (Vector2)m_dragonMouth.position - m_motion.position;
			m_distanceSqr = vectorToPlayer.sqrMagnitude;

			if (m_distanceSqr < m_sensorMaxRadius * m_sensorMaxRadius) {
				// check if the dragon is inside the sense zone
				if (m_distanceSqr < m_sensorMinRadius * m_sensorMinRadius) {
					// Check if this entity can see the player
					if (sensorAngle == 360) {
						m_alert = true;
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
						}
					}
				} 
			} else {
				m_alert = false;
			}
		} else {
			m_alert = false;
		}
	}
}
