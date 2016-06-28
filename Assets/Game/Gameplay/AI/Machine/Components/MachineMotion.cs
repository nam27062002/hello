using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public class MachineMotion : MachineComponent {
		protected static int m_groundMask;

		[SerializeField] private bool m_stickToGround = false;
		[SerializeField] private bool m_faceDirection = true;

		private Vector3 m_position;
		private Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		private Quaternion m_rotation;
		private Quaternion m_targetRotation;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineMotion() {}

		public override void Init() {
			m_groundMask = 1 << LayerMask.NameToLayer("Ground");

			m_position = m_machine.transform.position;
			m_rotation = m_machine.transform.rotation;
			m_targetRotation = m_rotation;

			m_direction = Vector3.right;
		}

		public override void Update() {
			if (m_pilot != null) {

				m_direction = m_pilot.direction;

				UpdateAttack();

				if (m_pilot.speed > 0.01f) {
					UpdateMovement();
				} else {
					// keep the entity facing left or right when not moving
					m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
					m_targetRotation = Quaternion.AngleAxis(270f, Vector3.up) * Quaternion.LookRotation(m_direction, Vector3.up);

					m_viewControl.Move(0f);
				}

				m_viewControl.Scared(m_pilot.IsActionPressed(Pilot.Action.Scared));

				if (m_stickToGround) {
					CheckCollisions();
				}
				m_machine.transform.position = m_position;

				// machine should face the same direction it is moving
				m_rotation = Quaternion.Lerp(m_rotation, m_targetRotation, Time.deltaTime * 2f);
				m_machine.transform.rotation = m_rotation;
			}
		}

		private void UpdateAttack() {
			if (m_pilot.IsActionPressed(Pilot.Action.Attack) && m_viewControl.canAttack()) {
				// start attack!
				m_viewControl.Attack();
			} else {
				if (m_viewControl.hasAttackEnded()) {					
					m_pilot.ReleaseAction(Pilot.Action.Attack);
				}

				if (!m_pilot.IsActionPressed(Pilot.Action.Attack)) {
					m_viewControl.StopAttack();
				}
			}
		}

		private void UpdateMovement() {	
			Vector3 right = Vector3.Cross(m_direction, Vector3.up);
			Vector3 up = Vector3.Cross(right, m_direction);

			if (m_faceDirection) {
				Quaternion rotation = Quaternion.AngleAxis(270f, up) * Quaternion.LookRotation(m_direction - (new Vector3(0, 0, 0.01f)), up); // Little hack to force the rotation to face user, 
				Vector3 eulerRotation = rotation.eulerAngles;																	   			  // if the machine move always in the same Z
				if (m_direction.y > 0) 		eulerRotation.z = Mathf.Min(40f, eulerRotation.z);
				else if (m_direction.y < 0)	eulerRotation.z = Mathf.Max(320f, eulerRotation.z);
				m_targetRotation = Quaternion.Euler(eulerRotation);	
			} else {
				m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
				m_targetRotation = Quaternion.AngleAxis(270f, Vector3.up) * Quaternion.LookRotation(m_direction, Vector3.up);
			}

			m_position += m_pilot.impulse * Time.deltaTime;

			m_viewControl.Move(m_pilot.impulse.magnitude);
		}

		private void CheckCollisions() {
			// teleport to ground
			RaycastHit ground;
			Vector3 testPosition = m_position + Vector3.up * 1f;

			if (Physics.Linecast(testPosition, testPosition + Vector3.down * 15f, out ground, m_groundMask)) {
				m_position.y = ground.point.y;
			}
		}
	}
}