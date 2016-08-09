using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public class MachineMotion : MachineComponent {
		protected static int m_groundMask;

		private enum UpVector {
			Up = 0,
			Down,
			Right,
			Left,
			Forward,
			Back
		};

		[SerializeField] private bool m_stickToGround = false;
		public bool stickToGround { get { return m_stickToGround; } set { m_stickToGround = value; } }
		[SerializeField] private bool m_walkOnWalls = false;
		[SerializeField] private float m_mass = 1f;

		[SeparatorAttribute]
		[SerializeField] private UpVector m_defaultUpVector = UpVector.Up;
		[SerializeField] private float m_orientationSpeed = 120f;
		[SerializeField] private bool m_faceDirection = true;
		public bool faceDirection { get { return m_faceDirection; } set { m_faceDirection = value; } }
		[SerializeField][HideInInspector] private bool m_facePlayer = false;

		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField][HideInInspector] private float m_faceLeftAngle = -90f;
		[SerializeField][HideInInspector] private float m_faceRightAngle = 90f;
	
		[SerializeField] private bool m_limitVerticalRotation = false;
		[SerializeField][HideInInspector] private float m_faceUpAngle = 320f;
		[SerializeField][HideInInspector] private float m_faceDownAngle = 40f;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		private Vector3 m_position;
		public Vector3 position { get { return m_position; } set { m_machine.transform.position = m_position = value; } }

		private float m_zOffset; // if we use different rails for machines
		public float zOffset { set { m_zOffset = value; } }

		private Vector3 m_upVector;
		public Vector3 upVector { get { return m_upVector; } set { m_upVector = value;} }

		private Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		private Vector3 m_velocity;
		private Vector3 m_gravity;

		private ViewControl m_viewControl;
		private Transform m_eye; // for aiming purpose

		private Quaternion m_rotation;
		private Quaternion m_targetRotation;


		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineMotion() {}

		public override void Init() {
			m_groundMask = LayerMask.GetMask("Ground", "GroundVisible");

			m_viewControl = m_machine.GetComponent<ViewControl>();
			m_eye = m_machine.transform.FindChild("eye");

			m_position = m_machine.transform.position;
			m_rotation = m_machine.transform.rotation;
			m_targetRotation = m_rotation;

			if (m_walkOnWalls) m_stickToGround = true;

			switch (m_defaultUpVector) {
				case UpVector.Up: 		m_upVector = Vector3.up; 		break;
				case UpVector.Down: 	m_upVector = Vector3.down; 		break;
				case UpVector.Right: 	m_upVector = Vector3.right; 	break;
				case UpVector.Left: 	m_upVector = Vector3.left; 		break;
				case UpVector.Forward: 	m_upVector = Vector3.forward; 	break;
				case UpVector.Back: 	m_upVector = Vector3.back;		break;
			}

			m_velocity = Vector3.zero;
			m_gravity = Vector3.zero;
			if (m_mass < 0f) {
				m_mass = 0f;
			}
		}

		public override void Update() {
			if (m_machine.GetSignal(Signals.Type.Panic)) {
				m_viewControl.Panic(true, m_machine.GetSignal(Signals.Type.Burning));
				return;
			} else {
				m_viewControl.Panic(false, m_machine.GetSignal(Signals.Type.Burning));
			}

			if (m_pilot != null) {
				Vector3 impulse = (m_pilot.impulse - m_velocity);
				impulse /= m_mass; //mass
				m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
				m_direction = m_pilot.direction;

				m_viewControl.NavigationLayer(m_pilot.impulse);

				UpdateAttack();

				m_position += (m_velocity + m_gravity) * Time.deltaTime;
				if (m_pilot.speed > 0.01f) {
					m_viewControl.Move(m_pilot.impulse.magnitude); //???
				} else {
					m_viewControl.Move(0f);
				}

				m_viewControl.Boost(m_pilot.IsActionPressed(Pilot.Action.Boost));
				m_viewControl.Scared(m_pilot.IsActionPressed(Pilot.Action.Scared));

				if (m_stickToGround) {
					bool isOnCollider = CheckCollisions();
					if (!isOnCollider) {
						m_gravity.y -= Time.fixedTime * Time.fixedTime * 9.8f;
					} else {
						m_gravity = Vector3.zero;
					}
				}

				UpdateOrientation();

				Vector3 pos = m_position;
				pos.z += m_zOffset;
				m_machine.transform.position = pos;

				//Aiming!!
				if (m_eye != null) {
					UpdateAim();
				}

				// machine should face the same direction it is moving
				m_rotation = Quaternion.RotateTowards(m_rotation, m_targetRotation, Time.deltaTime * m_orientationSpeed);

				m_viewControl.RotationLayer(ref m_rotation, ref m_targetRotation);
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
				Transform target = m_machine.enemy;
				if (target != null) {
					Vector3 targetDir = target.position - m_eye.position;

					targetDir.Normalize();
					Vector3 cross = Vector3.Cross(targetDir, Vector3.right);
					float aim = cross.z * -1;

					//between aim [0.9 - 1 - 0.9] we'll rotate the model
					//for testing purpose, it'll go from 90 to 270 degrees and back. Aim value 1 is 180 degrees of rotation
					float absAim = Mathf.Abs(aim);

					float angleSide = 90f;
					if (targetDir.x < 0) {
						angleSide = 270f;
					}
					float angle = angleSide;

					if (absAim >= 0.6f) {
						angle = (((absAim - 0.6f) / (1f - 0.6f)) * (180f - angleSide)) + angleSide;
					}

					// face target
					m_targetRotation = Quaternion.Euler(0, angle, 0);

					// blend between attack directions
					m_viewControl.Aim(aim);
				}
			}
		}

		private void UpdateOrientation() {	
			if (m_walkOnWalls) {
				if (m_direction != Vector3.zero) {
					m_targetRotation = Quaternion.LookRotation(m_direction, m_upVector);
				}
			} else if (m_faceDirection && m_pilot.speed > 0.01f) {				
				if (m_facePlayer) {
					Vector3 right = Vector3.Cross(m_direction, m_upVector);
					Vector3 up = Vector3.Cross(right, m_direction);
					m_targetRotation = Quaternion.LookRotation(m_direction - (new Vector3(0, 0, 0.01f)), up); // Little hack to force the rotation to face user, 
				} else {
					m_targetRotation = Quaternion.LookRotation(m_direction, m_upVector);
				}

				float angle = 0f;
				if (m_direction.x > 0)			angle = Vector3.Angle(Vector3.right, m_direction);
				else if (m_direction.x < 0)		angle = Vector3.Angle(Vector3.left, m_direction);

				angle = Mathf.Min(35f, angle);

				m_targetRotation = Quaternion.AngleAxis(-angle, m_direction) * m_targetRotation;

			} else {
				if (m_pilot.speed > 0.01f) {
					m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
				}
				m_targetRotation = Quaternion.LookRotation(m_direction, m_upVector);

			}

			if (m_limitHorizontalRotation || m_limitVerticalRotation) {
				m_targetRotation = LimitRotation(m_targetRotation);
			}
		}


		private Quaternion LimitRotation(Quaternion _quat) {
			Vector3 euler = _quat.eulerAngles;

			if (m_limitHorizontalRotation) {
				if (euler.y > m_faceRightAngle) {
					euler.y = m_faceRightAngle;
				} else if (euler.y < m_faceLeftAngle) {
					euler.y = m_faceLeftAngle;
				}
			}

			if (m_limitVerticalRotation) {					
				if (m_direction.y > 0.25f) {
					euler.x = Mathf.Max(m_faceUpAngle, euler.x);
				} else if (m_direction.y < -0.25f) {
					euler.x = Mathf.Min(m_faceDownAngle, euler.x);
				}
			}

			return Quaternion.Euler(euler);
		}

		private bool CheckCollisions() {
			// teleport to ground
			Vector3 normal = Vector3.up;
			Vector3 up = m_upVector;

			Vector3 start = m_position + (up * 3f);
			Vector3 end = m_position - (up * 3f);

			RaycastHit hit;
			bool hasHit = Linecast(start, end, true, out hit);
			Debug.DrawLine(start, end, Color.black);

			if (m_walkOnWalls) {
				if (!hasHit) {
					start = m_position - (up * 3f);
					end = m_position + (up * 3f);
					hasHit = Linecast(start, end, true, out hit);
				}

				if (hasHit) {
					normal = hit.normal;
				}

				// check forward to find change on the ground beforehand
				RaycastHit hitForward;
				start = m_position + m_direction + (normal * 3f);
				end = m_position + m_direction - (normal * 3f);

				Debug.DrawLine(start, end, Color.magenta);
				if (hasHit && Linecast(start, end, false, out hitForward)) {
					normal = (normal * 0.25f) + (hitForward.normal * 0.75f);
					m_direction = (hitForward.point - hit.point).normalized; // outdate direction using the two hits
				}
			}

			m_upVector = normal.normalized;
			Debug.DrawLine(m_position, m_position + m_upVector, Color.cyan);

			return hasHit;
		}

		private bool Linecast(Vector3 _start, Vector3 _end, bool _updatePosition, out RaycastHit _hit) {
			if (Physics.Linecast(_start, _end, out _hit, m_groundMask)) {
				if (_updatePosition) {/*
					m_position.x = _hit.point.x;
					m_position.y = _hit.point.y;
					*/
					m_position.x = Mathf.Lerp(m_position.x, _hit.point.x, Time.deltaTime * 8f);
					m_position.y = Mathf.Lerp(m_position.y, _hit.point.y, Time.deltaTime * 8f);

				}
				return true;
			}
			return false;
		}
	}
}