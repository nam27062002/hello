using UnityEngine;
using System.Collections;

namespace AI {
	public class AirPilot : AIPilot {
		private static float DOT_END = -0.7f;
		private static float DOT_START = -0.99f;

		private const int CollisionCheckPools = 4;
		private static uint NextCollisionCheckID = 0;


		[SerializeField] private float m_avoidDistanceAttenuation = 2f;
		[SerializeField] private bool m_avoidCollisions = false;
		public override bool avoidCollisions { get { return m_avoidCollisions; } set { m_avoidCollisions = value; } }

		private uint m_collisionCheckPool; // each prey will detect collisions at different frames
		protected float m_collisionAvoidFactor;
		protected Vector3 m_collisionNormal;

		private bool m_perpendicularAvoid;
		private Vector3 m_lastImpulse;



		protected virtual void Start() {
			m_perpendicularAvoid = false;

			m_collisionCheckPool = NextCollisionCheckID % CollisionCheckPools;
			NextCollisionCheckID++;
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			m_collisionAvoidFactor = 0f;
			m_collisionNormal = Vector3.up;
		}

		protected override void Update() {
			base.Update();

			// calculate impulse to reach our target
			m_lastImpulse = m_impulse;
			m_impulse = Vector3.zero;

			if (speed > 0.01f) {
				Vector3 seek = Vector3.zero;
				Vector3 flee = Vector3.zero;

				Vector3 v = m_target - m_machine.position;

				if (m_slowDown) { // this machine will slow down its movement when arriving to its detination
					Util.MoveTowardsVector3WithDamping(ref seek, ref v, moveSpeed, 32f * Time.deltaTime);
				} else {
					seek = v.normalized * moveSpeed;
				}
				Debug.DrawLine(m_machine.position, m_machine.position + seek, Color.green);

				if (IsActionPressed(Action.Avoid)) {
					Transform enemy = m_machine.enemy;
					if (enemy != null) {
						v = m_machine.position - enemy.position;
						v.z = 0;

						float distSqr = v.sqrMagnitude;
						if (distSqr > 0 && m_avoidDistanceAttenuation > 0) {
							v.Normalize();
							v *= (m_avoidDistanceAttenuation * m_avoidDistanceAttenuation) / distSqr;
						}
						flee = v;
						flee.z = 0;

						Debug.DrawLine(m_machine.position, m_machine.position + flee, Color.red);
					}
				}

				float dot = Vector3.Dot(seek.normalized, flee.normalized);
				float seekMagnitude = seek.magnitude;
				float fleeMagnitude = flee.magnitude;

				if (dot <= DOT_START) {
					m_perpendicularAvoid = true;
				} else if (dot > DOT_END) {
					m_perpendicularAvoid = false;
				}

				if (m_perpendicularAvoid) {
					m_impulse.Set(-flee.y, flee.x, flee.z);
				} else {
					m_impulse = seek + flee;
				}
				m_impulse = m_impulse.normalized * (Mathf.Max(seekMagnitude, fleeMagnitude));

				if (m_avoidCollisions) {
					AvoidCollisions();
					m_impulse = m_impulse.normalized * (Mathf.Max(seekMagnitude, fleeMagnitude));
				}

				m_impulse += m_externalImpulse;

				if (!m_directionForced) {// behaviours are overriding the actual direction of this machine
					if (m_impulse != Vector3.zero) {
						m_direction = m_impulse.normalized;
					}
				}

				float lerpFactor = (IsActionPressed(Action.Boost))? 4f : 2f;

				m_impulse = Vector3.Lerp(m_lastImpulse, Vector3.ClampMagnitude(m_impulse, speed), Time.smoothDeltaTime * lerpFactor);

				Debug.DrawLine(m_machine.position, m_machine.position + m_impulse, Color.white);
			}

			m_externalImpulse = Vector3.zero;
		}

		private void AvoidCollisions() {
			// 1- ray cast in the same direction where we are flying
			if (m_collisionCheckPool == Time.frameCount % CollisionCheckPools) {
				RaycastHit ground;

				float distanceCheck = 5f;

				if (Physics.Linecast(transform.position, transform.position + (m_direction * distanceCheck), out ground, m_groundMask)) {
					// 2- calc a big force to move away from the ground	
					m_collisionAvoidFactor = (distanceCheck / ground.distance) * 2f;
					m_collisionNormal = ground.normal;
					m_collisionNormal.z = 0f;
				} else {
					m_collisionAvoidFactor *= 0.75f;
				}
			}

			if (m_collisionAvoidFactor > 1f) {				
				m_impulse /= m_collisionAvoidFactor;
				m_impulse += (m_collisionNormal * m_collisionAvoidFactor);

				Debug.DrawLine(m_machine.position, m_machine.position + (m_collisionNormal * m_collisionAvoidFactor), Color.gray);
			}
		}
	}
}