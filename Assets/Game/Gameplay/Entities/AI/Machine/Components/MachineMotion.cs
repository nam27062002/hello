using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public class MachineMotion : MachineComponent {

		public override Type type { get { return Type.Motion; } }


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
		[SerializeField] private bool m_useDragonStyleRotation = false;
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
		
		public Vector3 position { 
			get { 
				if (m_groundSensor != null) {
					return m_groundSensor.position;
				} else {
					return m_machineTransform.position; 
				}
			} 

			set { 
				if (m_groundSensor != null) {
					m_machineTransform.position = value + (m_machineTransform.position - m_groundSensor.position); 
				} else {
					m_machineTransform.position = value; 
				}
			} 
		}

		private Transform m_machineTransform;

		private Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		private Vector3 m_groundDirection;
		public Vector3 groundDirection { get { return m_groundDirection; } }

		private Vector3 m_upVector;
		public Vector3 upVector { get { return m_upVector; } set { m_upVector = value;} }

		private Vector3 m_collisionNormal;
		private float m_fallingFromY;
		private bool m_isGrounded;
		private bool m_isColliderOnGround;
		private bool m_isJumping;
		private bool m_jumpVelocityApplied;
		private bool m_isFallingDown;
		private float m_heightFromGround;

		private float m_lastFallDistance;
		public float lastFallDistance { get { return m_lastFallDistance; } }

		private Vector3 m_velocity;
		public Vector3 velocity { get{  return m_velocity; } }
		public Vector3 angularVelocity { get{  if (m_rbody != null)return m_rbody.angularVelocity;return Vector3.zero; } }
		private Vector3 m_acceleration;

		private Rigidbody m_rbody;
		public bool isKinematic { get { return m_rbody.isKinematic; } set { m_rbody.isKinematic = value; } }

		private ViewControl m_viewControl;
		private Transform m_eye; // for aiming purpose
		private Transform m_mouth;
		private Transform m_groundSensor;

		private Quaternion m_rotation;
		private Quaternion m_targetRotation;

		private float m_latchBlending = 0f;

		private Transform m_attackTarget = null;
		public Transform attackTarget { get{ return m_attackTarget; } set{ m_attackTarget = value; } }

		private Vector3 m_externalVelocity;
		public Vector3 externalVelocity{ get{ return m_externalVelocity; } set{ m_externalVelocity = value; } }


		//gravity stuff
		private const float Air_Density = 1.293f;
		private const float Drag = 1.3f;//human //0.47f;//sphere
		private float m_terminalVelocity;

		//--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		public MachineMotion() {}

		public override void Attach (IMachine _machine, IEntity _entity, Pilot _pilot)
		{
			base.Attach (_machine, _entity, _pilot);
			m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "Obstacle", "PreyOnlyCollisions");
			m_rbody = m_machine.GetComponent<Rigidbody>();
			if ( m_rbody != null )
			{
				m_rbody.interpolation = RigidbodyInterpolation.None;
			}
			m_viewControl = m_machine.GetComponent<ViewControl>();

			m_machineTransform = m_machine.transform;
			m_eye = m_machineTransform.FindChild("eye");
			m_groundSensor = m_machineTransform.FindChild("groundSensor");

			m_mouth = null;
		}

		public override void Init() {
			m_isGrounded = false;
			m_isColliderOnGround = false;
			m_heightFromGround = 100f;

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
			m_direction = Vector3.forward; //(UnityEngine.Random.Range(0f, 1f) < 0.6f)? Vector3.right : Vector3.left;
			m_groundDirection = Vector3.right;

			if (m_mass < 0f) {
				m_mass = 0f;
			}

			if (m_useGravity /*&& !m_walkOnWalls*/) {
				// teleport to ground
				GetCollisionNormal();
				RaycastHit hit;
				bool hasHit = Physics.Raycast(position + m_upVector * 0.1f, -m_collisionNormal, out hit, 5f, m_groundMask);
				if (hasHit) {
					position = hit.point;
					m_heightFromGround = 0f;
				}
			} 

			m_terminalVelocity = Mathf.Sqrt((2f * m_mass * 9.8f) * (Air_Density * 1f * Drag));

			m_rotation = Quaternion.LookRotation(m_direction, m_upVector);
			m_targetRotation = m_rotation;

			m_machineTransform.rotation = m_rotation;

			m_isJumping = false;
			m_jumpVelocityApplied = false;
			m_isFallingDown = false;
			m_fallingFromY = -99999f;
			m_lastFallDistance = 0f;

			//----------------------------------------------------------------------------------
			m_mouth = m_machineTransform.FindTransformRecursive("Fire_Dummy");
		}

		public void LockInCage() {
			m_rbody.isKinematic = true;
			m_rbody.detectCollisions = false;
		}

		public void UnlockFromCage() {
			m_rbody.isKinematic = false;
			m_rbody.detectCollisions = true;
		}

		public void SetVelocity(Vector3 _v) {
			if (m_isJumping) {
				m_jumpVelocityApplied = true;
			}
			m_velocity = _v;
		}

		public void Stop() {
			if (!(m_machine.GetSignal(Signals.Type.FallDown) || m_pilot.IsActionPressed(Pilot.Action.Jump))) {
				m_velocity = Vector3.zero;
				m_rbody.velocity = Vector3.zero;
				m_rbody.angularVelocity = Vector3.zero;
			}
		}

		public void OnCollisionGroundEnter() {
			if (m_useGravity) {
				if (!m_isGrounded) {
					Stop();
				}
				m_isColliderOnGround = true;
			}
		}

		public void OnCollisionGroundExit() {
			if (m_useGravity) {				
				m_isColliderOnGround = false;
			}
		}


		public override void Update() {
			if (m_machine.GetSignal(Signals.Type.Latched)) {
				m_fallingFromY = -99999f;
			}

			if (m_pilot.IsActionPressed(Pilot.Action.Stop)) {
				Stop();
			}

			if (m_machine.GetSignal(Signals.Type.Biting)) {
				Stop();
				m_rotation = m_machineTransform.rotation;
				return;
			} else if (m_machine.GetSignal(Signals.Type.Latching)) {
				Stop();
				// m_latchBlending += Time.deltaTime;
				// Vector3 mouthOffset = (position - m_mouth.position);
				// position = Vector3.Lerp(position, m_pilot.target + mouthOffset, m_latchBlending);
				m_viewControl.Move(0);
				return;	
			}

			if (m_machine.GetSignal(Signals.Type.Panic)) {
				Stop();
				m_rotation = m_machineTransform.rotation;
				m_viewControl.Panic(true, m_machine.GetSignal(Signals.Type.Burning));
				return;
			} else {
				m_viewControl.Panic(false, m_machine.GetSignal(Signals.Type.Burning));
			}


			if (!m_machine.GetSignal(Signals.Type.LockedInCage)) {
				//--------------
				//ground, gravity and free falls
				if (m_useGravity) {
					bool isJumpingAlt = m_pilot.IsActionPressed(Pilot.Action.Jump);
					if (m_isJumping != isJumpingAlt) {
						if (isJumpingAlt) {
							m_fallingFromY = m_machineTransform.position.y;
						}
						m_isJumping = isJumpingAlt;
					}

					m_isFallingDown = !m_isJumping && m_machine.GetSignal(Signals.Type.FallDown);
					m_viewControl.Falling(m_isFallingDown);
					m_viewControl.Jumping(m_isJumping);

					GetCollisionNormal();
					GetHeightFromGround();

					m_isGrounded = m_isColliderOnGround || m_heightFromGround < 0.3f;

					if (m_isJumping) {
						if (m_jumpVelocityApplied) {
							if (m_velocity.y < 0f && m_isGrounded) { 
								m_pilot.ReleaseAction(Pilot.Action.Jump);
								m_jumpVelocityApplied = false;
								m_fallingFromY = -99999f;
							}
						}
					} else {
						m_jumpVelocityApplied = false;
						if (m_isFallingDown) {				
							if (m_fallingFromY < m_machineTransform.position.y)
								m_fallingFromY = m_machineTransform.position.y;

							if (m_isGrounded) {
								// check if it has to die > 10 units of distance?
								float dy = Mathf.Abs(m_machineTransform.position.y - m_fallingFromY);
								m_lastFallDistance = dy;

								m_machine.SetSignal(Signals.Type.FallDown, false);

								if (dy > 10f) {
									m_machine.SetSignal(Signals.Type.Destroyed, true);
								}
								m_fallingFromY = -99999f;
							}
						} else {
							if (!m_isGrounded && m_heightFromGround > 1f) {
								m_machine.SetSignal(Signals.Type.FallDown, true);
								m_fallingFromY = m_machineTransform.position.y;
							}
						}
					}
				}
			}

			//--------------
			// machine should face the same direction it is moving
			UpdateOrientation();

			//Aiming!!
			if (m_eye != null) {
				UpdateAim();
			}

			m_rotation = Quaternion.RotateTowards(m_rotation, m_targetRotation, Time.deltaTime * m_orientationSpeed);
			m_machineTransform.rotation = m_rotation;

			m_viewControl.RotationLayer(ref m_rotation, ref m_targetRotation);


			// View updates
			UpdateAttack();

			// Check if targeting to bend through that direction
			if (m_attackTarget) {
				Vector3 dir = m_attackTarget.position - position;
				dir.Normalize();
				m_viewControl.NavigationLayer(dir);	
			} else {
				m_viewControl.NavigationLayer(m_pilot.impulse);	
			}

			if (m_machine.GetSignal(Signals.Type.LockedInCage) || m_pilot.speed <= 0.01f) {
				m_viewControl.Move(0f);
			} else {
				m_viewControl.Move(m_velocity.magnitude);
			}

			m_viewControl.Boost(m_pilot.IsActionPressed(Pilot.Action.Boost));
			m_viewControl.Scared(m_pilot.IsActionPressed(Pilot.Action.Scared));

			//------
			Debug.DrawLine(position, position + m_rbody.velocity, Color.yellow);


			//
			Debug.DrawLine(position + upVector, position + upVector + m_groundDirection, Colors.darkGreen);
			Debug.DrawLine(position + (upVector * 1.5f) + m_groundDirection, position + (upVector * -1.5f) + m_groundDirection, Colors.darkGreen);

		}

		public override void FixedUpdate() {
			Signals.Type test = Signals.Type.Biting | Signals.Type.Latching | Signals.Type.Panic | Signals.Type.Latched | Signals.Type.LockedInCage;
			if (m_machine.GetSignal(test)) {	
				return;
			}

			if (m_rbody.isKinematic) {
				return;
			}
				
			if (m_pilot != null) {
				m_direction = m_pilot.direction;

				if (m_useGravity) {
					if (m_isGrounded && (m_isFallingDown || m_isJumping)) {
						Stop();
					}

					Vector3 forceGravity = Vector3.zero;
					if (m_walkOnWalls) 	forceGravity =  -m_collisionNormal * 9.8f * m_mass;
					else 				forceGravity =  Vector3.down * 9.8f * m_mass;

					if (m_isJumping) {
						m_velocity += (forceGravity) * Time.fixedDeltaTime;
						m_rbody.velocity = m_velocity;
					} else if (m_isGrounded || m_walkOnWalls) {
						UpdateVelocity();
						m_rbody.velocity = m_velocity + ((forceGravity * 3f) / m_mass) * Time.fixedDeltaTime + m_externalVelocity;
					} else {
						// free fall
						Vector3 forceDrag = -m_velocity.normalized * 0.25f * Air_Density * Drag * 1f * Mathf.Pow(m_velocity.magnitude, 2f) / m_mass;
						m_acceleration = (forceGravity + forceDrag) * 1.5f;

						float terminalVelocity = m_terminalVelocity;
						if (m_machine.GetSignal(Signals.Type.InWater)) {
							terminalVelocity *= 0.5f;
						}

						m_velocity += m_acceleration * Time.fixedDeltaTime;
						m_velocity = Vector3.ClampMagnitude(m_velocity, terminalVelocity) + m_externalVelocity;
						m_rbody.velocity = m_velocity;
					}
				} else {
					UpdateVelocity();
					m_rbody.velocity = m_velocity + m_externalVelocity;
				}
			}
		}

		public void LateUpdate() {
			if (m_machine.GetSignal(Signals.Type.Latching)) {
				m_latchBlending += Time.deltaTime;
				Vector3 mouthOffset = (position - m_mouth.position);
				position = Vector3.Lerp(position, m_pilot.target + mouthOffset, m_latchBlending);
			} else {
				m_latchBlending = 0;
			}

		}
		private void UpdateVelocity() {
			// "Physics" updates
			Vector3 impulse = (m_pilot.impulse - m_velocity);
			impulse /= m_mass; //mass

			if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
				m_velocity += impulse;
			} else {
				m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
			}
		}


		private void UpdateAttack() {
			if (m_pilot.IsActionPressed(Pilot.Action.Attack) && m_viewControl.canAttack()) {
				// start attack!
				m_viewControl.Attack(m_machine.GetSignal(Signals.Type.Melee), m_machine.GetSignal(Signals.Type.Ranged));
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
					targetDir.z = 0f;

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
					m_pilot.SetDirection(m_targetRotation * Vector3.forward, true);

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
			} else if (m_machine.GetSignal(Signals.Type.FallDown)) {
				m_targetRotation = Quaternion.LookRotation(m_direction, m_collisionNormal);
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
			} else if (m_useDragonStyleRotation && m_pilot.speed > 0.01f) {
				float angle = m_direction.ToAngleDegrees();
				float roll = angle;
				float pitch = angle;
				float yaw = 0;

				Quaternion qRoll = Quaternion.Euler(0.0f, 0.0f, roll);
				Quaternion qYaw = Quaternion.Euler(0.0f, yaw, 0.0f);
				Quaternion qPitch = Quaternion.Euler(pitch, 0.0f, 0.0f);
				m_targetRotation = qYaw * qRoll * qPitch;
				Vector3 eulerRot = m_targetRotation.eulerAngles;
				if (m_limitVerticalRotation)
				{
					// top cap
					if (eulerRot.z > m_faceUpAngle && eulerRot.z < 180 - m_faceUpAngle) 
					{
						eulerRot.z = m_faceUpAngle;
					}
					// bottom cap
					else if ( eulerRot.z > 180 + m_faceDownAngle && eulerRot.z < 360-m_faceDownAngle )
					{
						eulerRot.z = -m_faceDownAngle;
					}
				}
				m_targetRotation = Quaternion.Euler(eulerRot) * Quaternion.Euler(0,90.0f,0);

			} else {
				if (m_pilot.speed > 0.01f) {
					m_direction = (m_direction.x >= 0)? Vector3.right : Vector3.left;
				}
				m_targetRotation = Quaternion.LookRotation(m_direction + Vector3.back * 0.1f, m_upVector);

			}

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_upVector) * m_targetRotation; 
				else if (m_direction.x > 0f)m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_upVector) * m_targetRotation; 
			}

			if (m_limitVerticalRotation && !m_useDragonStyleRotation) {
				Vector3 euler = m_targetRotation.eulerAngles;
				if (m_direction.y > 0.25f) 			euler.x = Mathf.Max(m_faceUpAngle, euler.x);
				else if (m_direction.y < -0.25f) 	euler.x = Mathf.Min(m_faceDownAngle, euler.x);
				m_targetRotation = Quaternion.Euler(euler);
			}
		}

		private void GetHeightFromGround() {
			if (m_isColliderOnGround) {
				m_heightFromGround = 0f;
			} else {
				RaycastHit hit;
				bool hasHit = Physics.Raycast(position + m_upVector * 0.1f, -m_collisionNormal, out hit, 5f, m_groundMask);

				if (hasHit) {
					m_heightFromGround = hit.distance;
				} else {
					m_heightFromGround = 100f;
				}
			}

			m_viewControl.Height(m_heightFromGround);
		}

		private bool GetCollisionNormal() {			
			Vector3 normal = Vector3.up;

			bool hasHit = CheckCollision(m_upVector, ref normal, ref m_groundDirection);

			if (m_walkOnWalls) {
				if (!hasHit) {
					hasHit = CheckCollision(m_upVector * -1, ref normal, ref m_groundDirection);
				}

				m_upVector = normal;
				Debug.DrawLine(position, position + m_upVector, Color.cyan);
			} else {
				if (m_groundDirection.y > 0.5f || m_groundDirection.y < -0.5f) {
					m_groundDirection = Vector3.right;
				}
			}

			m_collisionNormal = normal;
			//m_direction = m_groundDirection;
			return hasHit;
		}

		private bool CheckCollision(Vector3 _up, ref Vector3 _normal, ref Vector3 _direction) {
			Vector3 pos = position;			
			Vector3 start = pos + (_up * 3f);
			Vector3 end = pos - (_up * 3f);

			// first cast
			RaycastHit hit;
			bool hasHit = Physics.Linecast(start, end, out hit, m_groundMask);
			Debug.DrawLine(start, end, Color.black);

			if (hasHit) {
				_normal = hit.normal;
			
				// cast forward
				RaycastHit hitForward;
				Vector3 dir = m_direction;
				dir.z = 0f;

				start = pos + (dir * 0.5f) + (_normal * 3f);
				end = pos + (dir * 0.5f) - (_normal * 3f);

				Debug.DrawLine(start, end, Color.magenta);
				if (hasHit && Physics.Linecast(start, end, out hitForward, m_groundMask)) {					
					_normal = (_normal * 0.5f) + (hitForward.normal * 0.75f);
					_normal.Normalize();
				}

				_direction = Vector3.Cross(Vector3.back, _normal);

				return true;
			}

			_normal = Vector3.up;
			_direction = Vector3.right;

			return false;
		}
	}
}