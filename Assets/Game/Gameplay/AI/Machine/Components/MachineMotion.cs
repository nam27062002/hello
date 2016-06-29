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

		private Transform m_eye; // for aiming purpose

		private Quaternion m_rotation;
		private Quaternion m_targetRotation;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineMotion() {}

		public override void Init() {
			m_groundMask = 1 << LayerMask.NameToLayer("Ground");

			m_eye = m_machine.transform.FindChild("eye");

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

				//Aiming!!
				if (m_eye != null) {
					UpdateAim();
				}

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

		private void UpdateAim() {
			if (m_pilot.IsActionPressed(Pilot.Action.Aim)) {
				Machine target = m_machine.enemy;
				if (target != null) {
					Vector3 targetDir = target.position - m_eye.position;

					targetDir.Normalize();
					Vector3 cross = Vector3.Cross(targetDir, Vector3.right);
					float aim = cross.z * -1;

					//between aim [0.9 - 1 - 0.9] we'll rotate the model
					//for testing purpose, it'll go from 90 to 270 degrees and back. Aim value 1 is 180 degrees of rotation
					float absAim = Mathf.Abs(aim);

					float angleSide = 0f;
					if (targetDir.x < 0) {
						angleSide = 180f;
					}
					float angle = angleSide;

					if (absAim >= 0.6f) {
						angle = (((absAim - 0.6f) / (1f - 0.6f)) * (90f - angleSide)) + angleSide;
					}

					Debug.Log(aim + "  -  " + angle);

					// face target
					m_targetRotation = Quaternion.Euler(0, angle, 0);

					// blend between attack directions
					m_viewControl.Aim(aim);
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