using UnityEngine;
using System.Collections;

namespace AI {
	public abstract class Pilot : MonoBehaviour {

		private static float DOT_END = -0.7f;
		private static float DOT_START = -0.99f;

		//TODO: tornar a crear la interficie??
		public enum Action {
			Boost = 0,
			Bite,
			Fire,
			Avoid,
			Pursuit,

			Count
		}

		//----------------------------------------------------------------------------------------------------------------

		[SerializeField] private float m_avoidDistanceAttenuation = 2f;

		protected Machine m_machine;

		protected bool[] m_actions;

		protected Vector3 m_target;

		protected float m_speed;
		public float speed { get { return m_speed; } }

		protected Vector3 m_impulse;
		public Vector3 impulse { get { return m_impulse; } }

		protected Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		private bool m_perpendicularAvoid;

		//----------------------------------------------------------------------------------------------------------------

		void Start() {
			m_speed = 0;
			m_impulse = Vector3.zero;

			m_target = transform.position;

			m_actions = new bool[(int)Action.Count];
			m_machine = GetComponent<Machine>();

			m_perpendicularAvoid = false;
		}

		public bool IsActionPressed(Pilot.Action _action) {
			return false;
		}

		public void SetSpeed(float _speed) {
			m_speed = _speed;
		}

		public void GoTo(Vector3 _target) {
			m_target = _target;
		}

		public void Avoid(bool _enable) {
			m_actions[(int)Action.Avoid] = _enable;
		}

		public void Pursuit(bool _enable) {
			m_actions[(int)Action.Pursuit] = _enable;
		}

		protected virtual void Update() {
			// calculate impulse to reach our target
			m_impulse = Vector3.zero;

			if (m_speed > 0) {
				Vector3 seek = Vector3.zero;
				Vector3 flee = Vector3.zero;

				Vector3 v = m_target - transform.position;
				Util.MoveTowardsVector3WithDamping(ref seek, ref v, m_speed, 32f * Time.deltaTime);
				Debug.DrawLine(transform.position, transform.position + seek, Color.green);

				if (m_actions[(int)Action.Avoid]) {
					Machine enemy = m_machine.enemy;
					if (enemy != null) {
						v = transform.position - enemy.position;
						float distSqr = v.sqrMagnitude;
						if (distSqr > 0) {
							v.Normalize();
							v *= (m_avoidDistanceAttenuation * m_avoidDistanceAttenuation) / distSqr;
						}
						flee = v;
						flee.z = 0;

						Debug.DrawLine(transform.position, transform.position + flee, Color.red);
					}
				}

				float dot = Vector3.Dot(seek.normalized, flee.normalized);

				if (dot <= DOT_START) {
					m_perpendicularAvoid = true;
				} else if (dot > DOT_END) {
					m_perpendicularAvoid = false;
				}

				if (m_perpendicularAvoid) {
					m_impulse.Set(-flee.y, flee.x, flee.z);
					m_impulse = m_impulse.normalized * (Mathf.Max(seek.magnitude, flee.magnitude));
				} else {
					m_impulse = seek + flee;
				}

				m_direction = m_impulse.normalized;
				m_impulse = m_direction * seek.magnitude;//mVector3.ClampMagnitude(m_impulse, m_speed);

				Debug.DrawLine(transform.position, transform.position + m_impulse, Color.white);

			}
		}

		void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(m_target, 0.25f);
		}
	}
}