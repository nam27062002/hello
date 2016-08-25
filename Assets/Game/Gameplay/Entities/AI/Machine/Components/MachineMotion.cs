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

		[SerializeField] private bool m_useGravity = false;
		public bool useGravity { get { return m_useGravity; } set { m_useGravity = value; } }
		[SerializeField] private bool m_walkOnWalls = false;
		[SerializeField] private float m_mass = 1f;

		[SeparatorAttribute]
		[SerializeField] private UpVector m_defaultUpVector = UpVector.Up;
		[SerializeField] private float m_orientationSpeed = 120f;
		[SerializeField] private bool m_faceDirection = true;
		public bool faceDirection { get { return m_faceDirection; } set { m_faceDirection = value; } }
		[SerializeField][HideInInspector] private bool m_rollRotation = false;

		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField][HideInInspector] private float m_faceLeftAngle = -90f;
		[SerializeField][HideInInspector] private float m_faceRightAngle = 90f;
	
		[SerializeField] private bool m_limitVerticalRotation = false;
		[SerializeField][HideInInspector] private float m_faceUpAngle = 320f;
		[SerializeField][HideInInspector] private float m_faceDownAngle = 40f;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public bool checkCollisions { set { m_walkOnWalls = value; } }
		
		public Vector3 position { get { return m_machine.transform.position; } set { m_machine.transform.position = value; } }

		private float m_zOffset; // if we use different rails for machines
		public float zOffset { set { m_zOffset = value; } }

		private Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		private Vector3 m_upVector;
		public Vector3 upVector { get { return m_upVector; } set { m_upVector = value;} }

		private Vector3 m_collisionNormal;
		private bool m_isGrounded;

		private Vector3 m_velocity;
		private Vector3 m_acceleration;

		private Collider m_collider;
		private Rigidbody m_rbody;
		private ViewControl m_viewControl;
		private Transform m_eye; // for aiming purpose

		private Quaternion m_rotation;
		private Quaternion m_targetRotation;


		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineMotion() {}

		public override void Init() {
			m_groundMask = LayerMask.GetMask("Ground", "GroundVisible");

			m_collider = m_machine.transform.FindComponentRecursive<Collider>();
			m_rbody = m_machine.GetComponent<Rigidbody>();
			m_viewControl = m_machine.GetComponent<ViewControl>();
			m_eye = m_machine.transform.FindChild("eye");

			m_isGrounded = false;

			if (m_walkOnWalls) {
				m_useGravity = true;

				//find the nearest wall and use that up vector
				RaycastHit[] hit = new RaycastHit[4];
				bool[] hasHit = new bool[4];

				hasHit[0] = Physics.Linecast(position, position + Vector3.down * 5f, out hit[0], m_groundMask);
				hasHit[1] = Physics.Linecast(position, position + Vector3.up * 5f,	 out hit[1], m_groundMask);
				hasHit[2] = Physics.Linecast(position, position + Vector3.right * 5f,out hit[2], m_groundMask);
				hasHit[3] = Physics.Linecast(position, position + Vector3.left * 5f, out hit[3], m_groundMask);

				float d = 99999f;
				for (int i = 0; i < 4; i++) {
					if (hasHit[i]) {
						if (hit[i].distance < d) {
							d = hit[i].distance;
							m_upVector = hit[i].normal;
						}
					}
				}
			} else {
				switch (m_defaultUpVector) {
					case UpVector.Up: 		m_upVector = Vector3.up; 		break;
					case UpVector.Down: 	m_upVector = Vector3.down; 		break;
					case UpVector.Right: 	m_upVector = Vector3.right; 	break;
					case UpVector.Left: 	m_upVector = Vector3.left; 		break;
					case UpVector.Forward: 	m_upVector = Vector3.forward; 	break;
					case UpVector.Back: 	m_upVector = Vector3.back;		break;
				}
			}

			m_velocity = Vector3.zero;
			m_acceleration = Vector3.zero;
			m_collisionNormal = Vector3.up;
			m_direction = (UnityEngine.Random.Range(0f, 1f) < 0.6f)? Vector3.right : Vector3.left;

			if (m_mass < 0f) {
				m_mass = 0f;
			}

			if (m_useGravity) {
				// teleport to ground
				GetCollisionNormal();
				RaycastHit hit;
				bool hasHit = Physics.Raycast(position + m_upVector * 0.1f, -m_collisionNormal, out hit, 5f, m_groundMask);
				if (hasHit) {
					m_machine.position = hit.point;
				}
			}

			m_rotation = Quaternion.LookRotation(m_direction, m_upVector);
			m_targetRotation = m_rotation;

			m_machine.transform.rotation = m_rotation;
		}

		public void SetVelocity(Vector3 _v) {
			m_velocity = _v;
		}

		public void Stop() {
			m_velocity = Vector3.zero;
			m_rbody.velocity = Vector3.zero;
			m_rbody.angularVelocity = Vector3.zero;
		}

		public override void Update() {
			if (m_machine.GetSignal(Signals.Type.Biting)) {
				Stop();
				m_rotation = m_machine.transform.rotation;
				return;
			}

			if (m_machine.GetSignal(Signals.Type.Panic)) {
				Stop();
				m_rotation = m_machine.transform.rotation;
				m_viewControl.Panic(true, m_machine.GetSignal(Signals.Type.Burning));
				return;
			} else {
				m_viewControl.Panic(false, m_machine.GetSignal(Signals.Type.Burning));
			}
				
			if (m_pilot != null) {
				m_direction = m_pilot.direction;

				if (m_useGravity) {
					GetCollisionNormal();

					Vector3 forceGravity = Vector3.zero;
					if (m_walkOnWalls) 	forceGravity =  -m_collisionNormal * 9.8f * m_mass;
					else 				forceGravity =  Vector3.down * 9.8f * m_mass;

					bool isGrounded = IsGrounded();

					if (m_isGrounded != isGrounded) {
						if (isGrounded) {
							m_velocity = Vector3.zero; // reset velocity when reaching ground
						}
						m_isGrounded = isGrounded;
					}

					if (m_isGrounded || m_walkOnWalls) {						
						UpdateVelocity();
						m_rbody.velocity = m_velocity + (forceGravity / m_mass) * Time.deltaTime;
					} else {
						// free fall, drag, friction and stuff
						const float airDensity = 1.293f;
						const float drag = 1.3f;//human //0.47f;//sphere
						float area = Mathf.PI * Mathf.Pow(m_collider.bounds.extents.x, 2f);
						float terminalVelocity = Mathf.Sqrt((2f * m_mass * 9.8f) * (airDensity * area * drag));

						Vector3 forceDrag = -m_velocity.normalized * 0.25f * airDensity * drag * area * Mathf.Pow(m_velocity.magnitude, 2f) / m_mass;
						m_acceleration = (forceGravity + forceDrag);

						m_velocity += Vector3.ClampMagnitude(m_acceleration * Time.deltaTime, terminalVelocity);
						m_rbody.velocity = m_velocity;
					}
				} else {
					UpdateVelocity();
					m_rbody.velocity = m_velocity;
				}

				Debug.DrawLine(position, position + m_rbody.velocity, Color.yellow);

				// machine should face the same direction it is moving
				UpdateOrientation();

				//Aiming!!
				if (m_eye != null) {
					UpdateAim();
				}
				m_rotation = Quaternion.RotateTowards(m_rotation, m_targetRotation, Time.deltaTime * m_orientationSpeed);

				m_viewControl.RotationLayer(ref m_rotation, ref m_targetRotation);
				m_machine.transform.rotation = m_rotation;

				// View updates
				UpdateAttack();

				m_viewControl.NavigationLayer(m_pilot.impulse);

				if (m_pilot.speed > 0.01f) {
					m_viewControl.Move(m_pilot.speed);//m_pilot.impulse.magnitude); //???
				} else {
					m_viewControl.Move(0f);
				}

				m_viewControl.Boost(m_pilot.IsActionPressed(Pilot.Action.Boost));
				m_viewControl.Scared(m_pilot.IsActionPressed(Pilot.Action.Scared));
			}

			m_viewControl.Falling(m_machine.GetSignal(Signals.Type.FallDown));
		}

		private void UpdateVelocity() {
			// "Physics" updates
			Vector3 impulse = (m_pilot.impulse - m_velocity);
			impulse /= m_mass; //mass
			m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
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
				m_targetRotation = Quaternion.LookRotation(m_direction, m_upVector);

				if (m_rollRotation) {
					float angle = Vector3.Angle(Vector3.right, m_direction);

					if (angle > 10f && angle < 90f) {
						angle = Mathf.Min(35f, angle);
					} else if (angle > 90f && angle < 170f) {
						angle = Mathf.Min(35f, 180f - angle);
					} else {
						angle = 0f;
					}

					if (m_direction.x < 0f && m_direction.z > 0f
					||  m_direction.x > 0f && m_direction.z < 0f) {
						angle *= -1;
					}
				
					m_targetRotation = Quaternion.AngleAxis(angle, m_direction) * m_targetRotation;
				}

			} else {
				if (m_pilot.speed > 0.01f) {
					m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
				}
				m_targetRotation = Quaternion.LookRotation(m_direction, m_upVector);

			}

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_upVector) * m_targetRotation; 
				else if (m_direction.x > 0f)m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_upVector) * m_targetRotation; 
			}

			if (m_limitVerticalRotation) {
				Vector3 euler = m_targetRotation.eulerAngles;
				if (m_direction.y > 0.25f) 			euler.x = Mathf.Max(m_faceUpAngle, euler.x);
				else if (m_direction.y < -0.25f) 	euler.x = Mathf.Min(m_faceDownAngle, euler.x);
				m_targetRotation = Quaternion.Euler(euler);
			}
		}

		private bool IsGrounded() {
			RaycastHit hit;
			bool hasHit = Physics.Raycast(position + m_upVector * 0.1f, -m_collisionNormal, out hit, 5f, m_groundMask);

			if (hasHit) {
				m_machine.SetSignal(Signals.Type.FallDown, hit.distance > 2f);
				m_viewControl.Height(hit.distance);
			} else {
				m_machine.SetSignal(Signals.Type.FallDown, true);
				m_viewControl.Height(100f);
			}
			
			return hasHit && hit.distance <= 0.3f;
		}

		private bool GetCollisionNormal() {			
			Vector3 normal = Vector3.up;
			Vector3 up = m_upVector;

			Vector3 start = position + (up * 2f);
			Vector3 end = position - (up * 2f);
		
			RaycastHit hit;
			bool hasHit = Physics.Linecast(start, end, out hit, m_groundMask);
			Debug.DrawLine(start, end, Color.black);

			if (hasHit) {
				normal = hit.normal;
			}

			if (m_walkOnWalls) {
				if (!hasHit) {
					start = position - (up * 2f);
					end = position + (up * 2f);
					hasHit = Physics.Linecast(start, end, out hit, m_groundMask);
				}

				if (hasHit) {
					normal = hit.normal;
				}

				// check forward to find change on the ground beforehand
				RaycastHit hitForward;
				Vector3 upVector = normal;
				start = position + m_direction + (normal * 2f);
				end = position + m_direction - (normal * 2f);

				Debug.DrawLine(start, end, Color.magenta);
				if (hasHit && Physics.Linecast(start, end, out hitForward, m_groundMask)) {
					upVector = (normal * 0.25f) + (hitForward.normal * 0.75f);
					m_direction = (hitForward.point - hit.point).normalized; // outdate direction using the two hits
				}
			
				m_upVector = upVector.normalized;
				Debug.DrawLine(position, position + m_upVector, Color.cyan);
			}

			m_collisionNormal = normal;
			return hasHit;
		}
	}
}