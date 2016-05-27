using UnityEngine;

namespace AI {
	public class MachineMotion : MachineComponent {


		private Vector3 m_position;
		private Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		private Quaternion m_rotation;
		private Quaternion m_targetRotation;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineMotion() {}

		public override void Init() {
			m_position = m_machine.transform.position;
			m_rotation = m_machine.transform.rotation;
			m_targetRotation = m_rotation;

			m_direction = Vector3.right;
		}

		public override void Update() {
			if (m_pilot != null) {
				// time increment
				float dt = Time.deltaTime;

				// lets move!
				float speed = m_pilot.speed;

				if (speed > 0.01f) {
					m_direction = m_pilot.direction;

					Vector3 right = Vector3.Cross(m_direction, Vector3.up);
					Vector3 up = Vector3.Cross(right, m_direction);

					Quaternion rotation = Quaternion.AngleAxis(270f, up) * Quaternion.LookRotation(m_direction - (new Vector3(0,0,0.01f)), up); // Little hack to force the rotation to face user, 
					Vector3 eulerRotation = rotation.eulerAngles;																				// if the machine move always in the same Z
					if (m_direction.y > 0) 		eulerRotation.z = Mathf.Min(40f, eulerRotation.z);
					else if (m_direction.y < 0)	eulerRotation.z = Mathf.Max(320f, eulerRotation.z);
					m_targetRotation = Quaternion.Euler(eulerRotation);	

					m_position += m_pilot.impulse * dt;
					m_machine.transform.position = m_position;
				} else {
					// keep the entity facing left or right when not moving
					m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
					m_targetRotation = Quaternion.AngleAxis(270f, Vector3.up) * Quaternion.LookRotation(m_direction, Vector3.up);
				}

				// machine should face the same direction it is moving
				m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, dt * 2f);
				m_machine.transform.rotation = m_rotation;
			}
		}
	}
}