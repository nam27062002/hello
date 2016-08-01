using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public class MachineSensor : MachineComponent {

		[SerializeField] private float m_minRadius;
		[SerializeField] private float m_maxRadius;
		[SerializeField][Range(45,360)] private float m_angle;
		[SerializeField][Range(0,360)] private float m_angleOffset;
		[SerializeField] private Vector3 m_sensorOffset = Vector3.zero;
		private Vector3 sensorPosition { get { return m_machine.transform.position + m_sensorOffset; } }
		[SerializeField] private Range m_senseDelay = new Range(0.25f, 1.25f);

		private Transform m_enemy; //enemy should be a Machine.. but dragon doesn't have this component
		public Transform enemy { get { return m_enemy; } }

		private float m_enemyRadiusSqr;
		private float m_senseTimer;

		private static int s_groundMask;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineSensor() {}

		public override void Init() {
			m_enemy = InstanceManager.player.transform;

			m_senseTimer = m_senseDelay.GetRandom();
			m_enemyRadiusSqr = 0;

			s_groundMask = LayerMask.GetMask("Ground", "GroundVisible");			
		}

		public override void Update() {
			if (m_enemy == null || !m_machine.GetSignal(Signals.Type.Alert) || m_machine.GetSignal(Signals.Type.Panic)) {
				m_machine.SetSignal(Signals.Type.Warning, false);
				m_machine.SetSignal(Signals.Type.Danger, false);
			} else {
				m_senseTimer -= Time.deltaTime;
				if (m_senseTimer <= 0) {
					bool isInsideMinArea = false;
					bool isInsideMaxArea = false;

					Vector2 vectorToPlayer = (Vector2)(m_enemy.position - sensorPosition);
					float distanceSqr = vectorToPlayer.sqrMagnitude - m_enemyRadiusSqr;

					if (distanceSqr < m_maxRadius * m_maxRadius) {
						// check if the dragon is inside the sense zone
						if (distanceSqr < m_minRadius * m_minRadius) {
							// Check if this entity can see the player
							if (m_angle == 360) {
								isInsideMinArea = true;
							} else {
								Vector2 direction = (m_machine.direction.x < 0)? Vector2.left : Vector2.right;
								float angle = Vector2.Angle(direction, vectorToPlayer); // angle between them: from 0 to 180

								Vector3 cross = Vector3.Cross(m_machine.direction, vectorToPlayer);					
								if (cross.z > 0) angle = 306 - angle;

								float sensorAngleFrom = m_angleOffset - (m_angle * 0.5f);
								if (sensorAngleFrom < 0) sensorAngleFrom += 360;

								float sensorAngleTo = m_angleOffset + (m_angle * 0.5f);
								if (sensorAngleTo > 360) sensorAngleTo -= 360;

								isInsideMinArea = angle >= sensorAngleFrom || angle <= sensorAngleTo;
							}
						}
						isInsideMaxArea = true;
					} 

					if (isInsideMinArea || isInsideMaxArea) {
						// Check line cast
						if (Physics.Linecast(sensorPosition, m_enemy.position, s_groundMask)) {
							isInsideMinArea = false;
							isInsideMaxArea = false;
						}
					}

					m_machine.SetSignal(Signals.Type.Warning, isInsideMaxArea);
					m_machine.SetSignal(Signals.Type.Danger, isInsideMinArea);

					m_senseTimer = m_senseDelay.GetRandom();
				}
			}				
		}

		// Debug
		public override void OnDrawGizmosSelected(Transform _go) {
			Vector3 pos = _go.position + m_sensorOffset;

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(pos, m_minRadius);
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(pos, m_maxRadius);

			float fromAngle = (-m_angle * 0.5f) + m_angleOffset;
			float toAngle = (m_angle * 0.5f) + m_angleOffset;
			Vector3 from = Vector3.right.RotateXYDegrees(fromAngle);
			Vector3 to = Vector3.right.RotateXYDegrees(toAngle);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(pos, pos + to * m_minRadius);
			Gizmos.DrawLine(pos, pos + from * m_minRadius);
		}
	}
}