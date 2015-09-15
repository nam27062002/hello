using UnityEngine;
using System.Collections;

public class SensePlayer : Steering {
	
	[SerializeField] private float m_sensorMinRadius;
	public float sensorMinRadius { get { return m_sensorMinRadius; } set { m_sensorMinRadius = value; } }

	[SerializeField] private float m_sensorMaxRadius;
	public float sensorMaxRadius { get { return m_sensorMaxRadius; } set { m_sensorMaxRadius = value; } }

	[SerializeField][Range(45,360)] private float m_sensorAngle;
	public float sensorAngle { get { return m_sensorAngle; } }


	private bool m_alert;
	public bool alert { get { return m_alert; } }


	void OnEnable() {

		m_alert = false;
	}

	// Update is called once per frame
	void Update () {
	
		DragonPlayer player = InstanceManager.player;

		if (m_prey.area == null || m_prey.area.Contains(player.transform.position)) {

			Vector2 vectorToPlayer = (Vector2)player.transform.position - m_prey.position;
			float distanceSqr = vectorToPlayer.sqrMagnitude;

			if (distanceSqr < m_sensorMaxRadius * m_sensorMaxRadius) {
				// check if the dragon is inside the sense zone
				if (distanceSqr < m_sensorMinRadius * m_sensorMinRadius) {
					// Check if this entity can see the player
					float angle = Vector2.Angle(m_prey.direction, vectorToPlayer); // angle between them: from 0 to 180

					if (angle <= m_sensorAngle * 0.5f) {
						m_alert = true;
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
