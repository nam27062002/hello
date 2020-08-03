using UnityEngine;
using System.Collections;

namespace AI {
	public class AirPilot : AIPilot {
		private static float DOT_END = -0.7f;
		private static float DOT_START = -0.99f;

		private const int CollisionCheckPools = 20;
		private static uint NextCollisionCheckID = 0;

		[SerializeField] private bool m_preciseDestArrival = false;

		[SerializeField] private float m_avoidDistanceAttenuation = 2f;
		[SerializeField] private bool m_avoidCollisions = false;
		public override bool avoidCollisions { get { return m_avoidCollisions; } set { m_avoidCollisions = value; } }

		[SerializeField] private bool m_avoidWater = false;
		public override bool avoidWater { get { return m_avoidWater; } set { m_avoidWater = value; } }

		[SerializeField] private float m_avoidCollisionsFactor = 10f;

		private uint m_collisionCheckPool; // each prey will detect collisions at different frames
		protected float m_collisionAvoidFactor;
		protected Vector3 m_collisionNormal;

		private bool m_perpendicularAvoid;
		private Vector3 m_lastImpulse;
		private Vector3 m_seek;


		protected virtual void Start() {
			m_perpendicularAvoid = false;
            
			m_lastImpulse = GameConstants.Vector3.zero;
			m_seek = GameConstants.Vector3.zero;
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

            m_collisionCheckPool = NextCollisionCheckID % CollisionCheckPools;
            NextCollisionCheckID++;

            m_collisionAvoidFactor = 0f;
			m_collisionNormal = GameConstants.Vector3.up;
		}

		public override void CustomUpdate() {
            UnityEngine.Profiling.Profiler.BeginSample("[AIR PILOT]");
            base.CustomUpdate();
                        
            // calculate impulse to reach our target
            m_lastImpulse = m_impulse;
			m_impulse = GameConstants.Vector3.zero;

			if (speed > 0.01f) {				
				if (IsActionPressed(Action.Latching)) {
					m_direction = m_targetRotation * GameConstants.Vector3.forward;
				} else {
                    Vector3 v = m_target - m_machine.position;
                    
					if (m_slowDown) { // this machine will slow down its movement when arriving to its detination
						v = v.normalized * Mathf.Min(moveSpeed, v.magnitude * 2);
						Util.MoveTowardsVector3WithDamping(ref m_seek, ref v, 32f * Time.deltaTime, 8.0f);
					} else {
						if (m_preciseDestArrival && (v.sqrMagnitude < moveSpeed * moveSpeed)) {
							v = v.normalized * Mathf.Max(moveSpeed * 0.25f, v.magnitude * 2);
						} else {
							v = v.normalized * moveSpeed;
						}
						m_seek = v;
					}
					#if UNITY_EDITOR
					Debug.DrawLine(m_machine.position, m_machine.position + m_seek, Color.green);
                    #endif


                    Vector3 flee = GameConstants.Vector3.zero;
					if (IsActionPressed(Action.Avoid)) {
						Transform enemy = m_machine.enemy;
						if (enemy != null) {
							v = m_machine.position - enemy.position;
							v.z = 0;

							float distSqr = v.sqrMagnitude;
							float distAttSqr = m_avoidDistanceAttenuation * m_avoidDistanceAttenuation;

							v.Normalize();
							if (distSqr > distAttSqr) {
								v *= distAttSqr / distSqr;
							} else {
								if (m_machine.GetSignal(Signals.Type.Critical)) {
									m_seek *= 0.25f;
								}
							}
							flee = v * speed;
							flee.z = 0;

							#if UNITY_EDITOR
							Debug.DrawLine(m_machine.position, m_machine.position + flee, Color.red);
							#endif
						}
					}


                    // add seek and flee vectors
                    if (IsActionPressed(Action.Avoid)) {
                        float dot = Vector3.Dot(m_seek, flee);
                        m_perpendicularAvoid = dot < 0f;

                        if (m_perpendicularAvoid) {
							m_impulse.x = -flee.y;
							m_impulse.y = flee.x;
							m_impulse.z = flee.z;
						} else {
							m_impulse = m_seek + flee;
						}
					} else {
						m_impulse = m_seek;
					}

                    // check near collisions 7
                    if (m_avoidCollisions || m_avoidWater) {
						AvoidCollisions();
					}

                    if (IsActionPressed(Action.Avoid)) {
						float lerpFactor = (IsActionPressed(Action.Boost))? 4f : 2f;
						m_impulse = Vector3.Lerp(m_lastImpulse, Vector3.ClampMagnitude(m_impulse, speed), Time.smoothDeltaTime * lerpFactor);
					} else {
						m_impulse = Vector3.ClampMagnitude(m_impulse, speed);
					}

                    m_impulse += m_externalImpulse;

					if (!m_directionForced && !m_stunned) {// behaviours are overriding the actual direction of this machine
						m_direction = m_impulse.normalized;
					}
					#if UNITY_EDITOR
					Debug.DrawLine(m_machine.position, m_machine.position + m_impulse, Color.white);
                    #endif
                }
			} else {
				m_seek = GameConstants.Vector3.zero;
			}

			m_externalImpulse = GameConstants.Vector3.zero;
            UnityEngine.Profiling.Profiler.EndSample();
        }

		private void AvoidCollisions() {
			// 1- ray cast in the same direction where we are flying
			if (m_collisionCheckPool == Time.frameCount % CollisionCheckPools) {
				RaycastHit ground;

				bool isInsideWater = false;
				float distanceCheck = 5f;
                int layerMask = GameConstants.Layers.GROUND_PREYCOL;

				if (m_avoidWater) {
					if (m_avoidCollisions) {
                        layerMask = GameConstants.Layers.GROUND_PREYCOL_WATER;
					} else {
                        layerMask = GameConstants.Layers.WATER;
					}
					isInsideWater = m_machine.GetSignal(Signals.Type.InWater);
				}

				if (isInsideWater) {
					m_collisionAvoidFactor = m_avoidCollisionsFactor * 1.5f;
					m_collisionNormal = GameConstants.Vector3.up;
					m_collisionNormal.z = 0f;
				} else if (Physics.Linecast(m_machine.position, m_machine.position + (m_direction * distanceCheck), out ground, layerMask)) {
					// 2- calc a big force to move away from the ground	
					m_collisionAvoidFactor = (distanceCheck / ground.distance) * m_avoidCollisionsFactor;
					m_collisionNormal = ground.normal;
					m_collisionNormal.z = 0f;
				} else {
					m_collisionAvoidFactor *= 0.5f;
				}
			}

			if (m_collisionAvoidFactor > 1f) {
                float dot = Vector3.Dot(m_impulse, m_collisionNormal);
                m_perpendicularAvoid = dot < 0f;

                if (m_perpendicularAvoid) {
					m_impulse.x = -m_impulse.y;
					m_impulse.y = m_impulse.x;					
				} else {
					m_impulse /= m_collisionAvoidFactor;
					m_impulse += (m_collisionNormal * m_collisionAvoidFactor);
				}

				#if UNITY_EDITOR
				Debug.DrawLine(m_machine.position, m_machine.position + (m_collisionNormal * m_collisionAvoidFactor), Color.gray);
				#endif
			}
		}
	}
}