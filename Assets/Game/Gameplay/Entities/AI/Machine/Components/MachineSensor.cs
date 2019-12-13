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

        private const int CollisionCheckPools = 20;
        private static uint NextCollisionCheckID = 0;


        private Transform m_enemy; //enemy should be a Machine.. but dragon doesn't have this component
		public Transform enemy { get { return m_enemy; } }

		private float m_enemyRadiusSqr;
		private RectAreaBounds m_enemyBounds;
		private float m_senseTimer;

        private uint m_collisionCheckPool; // each prey will detect collisions at different frames
        private bool m_cachedRaycast;

		private float m_sightRadiusIn 	= 0f;
		private float m_sightRadiusOut 	= 0f;
		private float m_maxRadiusIn 	= 0f;
		private float m_minRadiusIn 	= 0f;
		private float m_maxRadiusOut 	= 0f;
		private float m_minRadiusOut 	= 0f;
	
		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineSensor() {}

		public override void Init() {
            m_collisionCheckPool = NextCollisionCheckID % CollisionCheckPools;
            NextCollisionCheckID++;
            m_cachedRaycast = false;

            float radiusOffsetFactor = m_radiusOffset.GetRandom();

            m_senseTimer = 0.125f * (m_collisionCheckPool % 2);
			m_enemyRadiusSqr = 0f;

			UpdateDistances( radiusOffsetFactor );
			
		}

		protected void UpdateDistances( float radiusOffsetFactor )
		{
			m_sightRadiusIn 	= m_sightRadius * radiusOffsetFactor;
			m_maxRadiusIn 		= (m_maxRadius * radiusOffsetFactor);
			m_minRadiusIn 		= (m_minRadius * radiusOffsetFactor);

			m_sightRadiusOut 	= m_sightRadiusIn + m_hysteresisOffset;
			m_maxRadiusOut 		= m_maxRadiusIn + m_hysteresisOffset;
			m_minRadiusOut 		= m_minRadiusIn + m_hysteresisOffset;
		}

		public void SetupEnemy(Transform _tr, float distanceSqr, RectAreaBounds _enemyBounds) {
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
            UnityEngine.Profiling.Profiler.BeginSample("[SENSOR]");
			if (m_machine.GetSignal(Signals.Type.Panic)) {
				m_machine.SetSignal(Signals.Type.Warning, true);
				m_machine.SetSignal(Signals.Type.Danger, true);
				m_machine.SetSignal(Signals.Type.Critical, 	true);
			} else if (m_enemy == null || !m_machine.GetSignal(Signals.Type.Alert) || m_machine.GetSignal(Signals.Type.InLove)) {
				m_machine.SetSignal(Signals.Type.Warning, false);
				m_machine.SetSignal(Signals.Type.Danger, false);
				m_machine.SetSignal(Signals.Type.Critical, 	false);

				m_senseTimer = 1f;
			} else {				
				m_senseTimer -= Time.deltaTime;
				if (m_senseTimer <= 0f) {         
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

				    if (sense) {
					    float fireRadius = 0f;
					    if (m_senseFire) {
						    if (InstanceManager.player.IsFuryOn() ) {
							    fireRadius = InstanceManager.player.breathBehaviour.actualLength;
						    }
					    }
                        					    
						float distanceSqr = DistanceSqrToEnemy();
						float sightRadiusIn =  m_sightRadiusIn + fireRadius;
						float sightRadiusOut =  m_sightRadiusOut + fireRadius;

						if (distanceSqr < sightRadiusIn * sightRadiusIn ) {
							isInsideSightArea = true;
							m_senseTimer = m_senseDelay.GetRandom();
						} else if (distanceSqr > sightRadiusOut * sightRadiusOut) {
							isInsideSightArea = false;
							m_senseTimer = 0f;
						}					    

					    if (isInsideSightArea) {
						    float maxRadiusIn  = m_maxRadiusIn + fireRadius;
						    float minRadiusIn  = m_minRadiusIn + fireRadius;
						    float maxRadiusOut = m_maxRadiusOut + fireRadius;
						    float minRadiusOut = m_minRadiusOut + fireRadius;

						    if (distanceSqr < float.Epsilon) {
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
                                if (m_collisionCheckPool == Time.frameCount % CollisionCheckPools) {
                                    m_cachedRaycast = Physics.Linecast(sensorPosition, m_enemy.position, GameConstants.Layers.GROUND);
                                }
                                    // Check line cast
                                if (m_cachedRaycast) {
								    isInsideSightArea = false;
								    isInsideMaxArea = false;
								    isInsideMinArea = false;
							    }
						    }
					    } else {
						    isInsideMaxArea = false;
						    isInsideMinArea = false;
					    }
				    } else {
					    isInsideSightArea = false;
					    isInsideMaxArea = false;
					    isInsideMinArea = false;
				    }

                    m_machine.SetSignal(Signals.Type.Warning, 	isInsideSightArea);
				    m_machine.SetSignal(Signals.Type.Danger, 	isInsideMaxArea);
				    m_machine.SetSignal(Signals.Type.Critical, 	isInsideMinArea);

                    m_senseTimer = 0.125f * (m_collisionCheckPool % 3);
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

		public float DistanceSqrToEnemy() {
			Vector2 vectorToPlayer = (Vector2)(m_enemy.position - sensorPosition);
			float distanceSqr = Mathf.Max(0f, vectorToPlayer.sqrMagnitude - m_enemyRadiusSqr);

			if (m_enemyBounds != null && (distanceSqr > m_minRadiusIn * m_minRadiusIn) ) {
				float distanceToBounds = m_enemyBounds.DistanceSqr( sensorPosition );
				distanceSqr = Mathf.Min( distanceSqr, distanceToBounds);
			}
			return distanceSqr;
		}

		// Debug
		public override void OnDrawGizmosSelected(Transform _go) {
#if UNITY_EDITOR
			UpdateDistances( m_radiusOffset.GetRandom() );
#endif

			float fireRadius = 0f;
			if (Application.isPlaying) {
				if (m_senseFire && InstanceManager.player != null) {					
					if (InstanceManager.player.IsFuryOn()) {
						fireRadius = InstanceManager.player.breathBehaviour.actualLength;
					}
				}
			}

			Vector3 pos = _go.position + (_go.rotation * m_sensorOffset);
			Gizmos.color = Colors.paleYellow;
			Gizmos.DrawWireSphere(pos, m_sightRadiusIn + fireRadius);
			Gizmos.DrawWireSphere(pos, m_sightRadiusOut + fireRadius);
			Gizmos.color = Colors.red;
			Gizmos.DrawWireSphere(pos, m_maxRadiusIn + fireRadius);
			Gizmos.DrawWireSphere(pos, m_maxRadiusOut + fireRadius);
			Gizmos.color = Colors.magenta;
			Gizmos.DrawWireSphere(pos, m_minRadiusIn + fireRadius);
			Gizmos.DrawWireSphere(pos, m_minRadiusOut + fireRadius);
		}
	}
}