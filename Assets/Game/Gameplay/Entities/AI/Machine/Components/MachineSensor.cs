using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public class MachineSensor : MachineComponent {

		public override Type type { get { return Type.Sensor_Player; } }

		[SerializeField] private bool m_senseFire = true;
		[SerializeField] private float m_sightRadius;
		[SerializeField] private float m_maxRadius;
		[SerializeField] private float m_minRadius;
		[SerializeField] private float m_hysteresisOffset = 0f;
		[SerializeField] private bool m_senseAbove = true;
		[SerializeField] private bool m_senseBelow = true;
		[SerializeField] private Vector3 m_sensorOffset = Vector3.zero;
		public Vector3 sensorPosition { get { return m_machine.transform.position + (m_machine.transform.rotation * m_sensorOffset); } }
		[SerializeField] private Range m_radiusOffset = new Range(0.9f, 1.1f);
		[SerializeField] private Range m_senseDelay = new Range(0.25f, 1.25f);

		private Transform m_enemy; //enemy should be a Machine.. but dragon doesn't have this component
		public Transform enemy { 
			get { return m_enemy; } 
			// set { m_enemy = value; }
		}

		private float m_radiusOffsetFactor = 1f;
		private float m_enemyRadiusSqr;
		private RectAreaBounds m_enemyBounds;
		private float m_senseTimer;

		private static int s_groundMask;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineSensor() {}

		public override void Init() {
			m_senseTimer = 0f;
			m_enemyRadiusSqr = 0f;

			/*
			if (InstanceManager.player != null) {
				SetupEnemy(InstanceManager.player.transform, InstanceManager.player.dragonEatBehaviour.eatDistanceSqr);
			}
			*/
			m_radiusOffsetFactor = m_radiusOffset.GetRandom();

			s_groundMask = LayerMask.GetMask("Ground", "GroundVisible");			
		}

		public void SetupEnemy( Transform _tr, float distanceSqr, RectAreaBounds _enemyBounds )
		{
			m_enemy = _tr;
			m_enemyRadiusSqr = distanceSqr;
			m_enemyBounds = _enemyBounds;
		}

		public void Disable(float _seconds) {
			if (_seconds > 0f) {
				m_senseTimer = _seconds;

				m_machine.SetSignal(Signals.Type.Warning, false);
				m_machine.SetSignal(Signals.Type.Danger, false);
				m_machine.SetSignal(Signals.Type.Critical, 	false);
			}
		}

		public override void Update() {
			bool isFalling = m_machine.GetSignal(Signals.Type.FallDown) && !m_pilot.IsActionPressed(Pilot.Action.Jump);
			if (m_machine.GetSignal(Signals.Type.Panic)) {
				m_machine.SetSignal(Signals.Type.Warning, true);
				m_machine.SetSignal(Signals.Type.Danger, true);
				m_machine.SetSignal(Signals.Type.Critical, 	true);
			} else if (m_enemy == null || !m_machine.GetSignal(Signals.Type.Alert)) {
				m_machine.SetSignal(Signals.Type.Warning, false);
				m_machine.SetSignal(Signals.Type.Danger, false);
				m_machine.SetSignal(Signals.Type.Critical, 	false);

				m_senseTimer = 1f;
			} else {
				float distanceSqr = 0f;
				bool isInsideSightArea = m_machine.GetSignal(Signals.Type.Warning);
				bool isInsideMaxArea = m_machine.GetSignal(Signals.Type.Danger);
				bool isInsideMinArea = m_machine.GetSignal(Signals.Type.Critical);
				bool sense = false;

				if (m_senseAbove && m_senseBelow) {
					sense = true;
				} else if (m_senseAbove) {
					sense = m_enemy.position.y > sensorPosition.y;
				} else if (m_senseBelow) {
					sense = m_enemy.position.y < sensorPosition.y;								
				}

				float fireRadius = 0f;
				if (m_senseFire) {
					if (InstanceManager.player.IsFuryOn() || InstanceManager.player.IsSuperFuryOn()) {
						fireRadius = InstanceManager.player.breathBehaviour.actualLength;
					}
				}

				m_senseTimer -= Time.deltaTime;
				if (m_senseTimer <= 0) {
					float sightRadiusIn = fireRadius + (m_sightRadius * m_radiusOffsetFactor);
					float sightRadiusOut = sightRadiusIn + m_hysteresisOffset;

					distanceSqr = DistanceSqrToEnemy();

					if (distanceSqr < sightRadiusIn * sightRadiusIn ) {
						isInsideSightArea = true;
						m_senseTimer = m_senseDelay.GetRandom();
					} else if (distanceSqr > sightRadiusOut * sightRadiusOut) {
						isInsideSightArea = false;
						m_senseTimer = 0f;
					}
				}

				if (isInsideSightArea) {
					if (sense) {
						float maxRadiusIn = fireRadius + (m_maxRadius * m_radiusOffsetFactor);
						float minRadiusIn = fireRadius + (m_minRadius * m_radiusOffsetFactor);
						float maxRadiusOut = maxRadiusIn + m_hysteresisOffset;
						float minRadiusOut = minRadiusIn + m_hysteresisOffset;

						if (distanceSqr == 0f) {
							distanceSqr = DistanceSqrToEnemy();
						}

						if (distanceSqr < m_maxRadius * maxRadiusIn) {
							// check if the dragon is inside the sense zone
							if (distanceSqr < minRadiusIn * minRadiusIn) {
								isInsideMinArea = true;
							} else if (distanceSqr > minRadiusOut * minRadiusOut) {
								isInsideMinArea = false;
							}
							isInsideMaxArea = true;
						} else if (distanceSqr > maxRadiusOut * maxRadiusOut) {
							isInsideMaxArea = false;
						}

						if (isInsideMinArea || isInsideMaxArea) {
							// Check line cast
							if (Physics.Linecast(sensorPosition, m_enemy.position, s_groundMask)) {
								isInsideSightArea = false;
								isInsideMinArea = false;
								isInsideMaxArea = false;
							}
						}
					}
				}

				m_machine.SetSignal(Signals.Type.Warning, 	isInsideSightArea);
				m_machine.SetSignal(Signals.Type.Danger, 	isInsideMaxArea);
				m_machine.SetSignal(Signals.Type.Critical, 	isInsideMinArea);
			}				
		}

		public float DistanceSqrToEnemy()
		{
			Vector2 vectorToPlayer = (Vector2)(m_enemy.position - sensorPosition);
			float distanceSqr = Mathf.Max(0f, vectorToPlayer.sqrMagnitude - m_enemyRadiusSqr);
			float minDistance = (m_minRadius * m_radiusOffsetFactor);
			if (m_enemyBounds != null && (distanceSqr > minDistance * minDistance) )
			{
				float distanceToBounds = m_enemyBounds.DistanceSqr( sensorPosition );
				distanceSqr = Mathf.Min( distanceSqr, distanceToBounds);
			}
			return distanceSqr;
		}

		// Debug
		public override void OnDrawGizmosSelected(Transform _go) {
			float sightRadiusIn = (m_sightRadius * m_radiusOffsetFactor);
			float maxRadiusIn   = (m_maxRadius * m_radiusOffsetFactor);
			float minRadiusIn   = (m_minRadius * m_radiusOffsetFactor);
			float sightRadiusOut= sightRadiusIn + m_hysteresisOffset;
			float maxRadiusOut	= maxRadiusIn + m_hysteresisOffset;
			float minRadiusOut	= minRadiusIn + m_hysteresisOffset;

			if (Application.isPlaying) {
				if (m_senseFire && InstanceManager.player != null) {
					float fireRadius = 0f;
					if (InstanceManager.player.IsFuryOn() || InstanceManager.player.IsSuperFuryOn()) {
						fireRadius = InstanceManager.player.breathBehaviour.actualLength;
					}
					sightRadiusIn += fireRadius;
					maxRadiusIn += fireRadius;
					minRadiusIn += fireRadius;
				}
			}

			Vector3 pos = _go.position + (_go.rotation * m_sensorOffset);
			Gizmos.color = Colors.paleYellow;
			Gizmos.DrawWireSphere(pos, sightRadiusIn);
			Gizmos.DrawWireSphere(pos, sightRadiusOut);
			Gizmos.color = Colors.red;
			Gizmos.DrawWireSphere(pos, maxRadiusIn);
			Gizmos.DrawWireSphere(pos, maxRadiusOut);
			Gizmos.color = Colors.magenta;
			Gizmos.DrawWireSphere(pos, minRadiusIn);
			Gizmos.DrawWireSphere(pos, minRadiusOut);
		}
	}
}