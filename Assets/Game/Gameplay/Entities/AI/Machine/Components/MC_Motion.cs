using UnityEngine;
using System;

namespace AI {
	public abstract class MC_Motion : MachineComponent {

		public sealed override Type type { get { return Type.Motion; } }

		//--------------------------------------------------
		public enum State {
			Free = 0,
			Biting,
			Latching,
			Locked,
			Panic,
			FreeFall,
			StandUp,
            InLove
		};

		public enum UpVector {
			Up = 0,
			Down,
			Right,
			Left,
			Forward,
			Back
		};

		//--------------------------------------------------
		[SeparatorAttribute("Motion")]
		[SerializeField] protected float m_mass = 1f;
		[SerializeField] private UpVector m_defaultUpVector = UpVector.Up;
		[SerializeField] protected float m_orientationSpeed = 120f;
        [SerializeField] protected bool m_useAngularVelocity = false;


        //--------------------------------------------------		
        protected const float GRAVITY = 9.8f;
		private const float AIR_DENSITY = 1.293f;
		private const float DRAG = 1.3f;//human //0.47f;//sphere
                                        //--------------------------------------------------

        protected Transform m_machineTransform;
		private Transform m_eye; // for aiming purpose
		private Transform m_mouth;
		private Transform m_groundSensor;
		private PreyAnimationEvents m_animEvents;

		private bool m_hasEye;

		protected ViewControl 	m_viewControl;
		protected Rigidbody		m_rbody;
        private RigidbodyConstraints m_rbodyConstraints;

		public Quaternion orientation { 
			get { return m_rotation; }
			set { m_targetRotation = m_rotation = value;
				m_machineTransform.rotation = m_rotation; }
		}

		private float m_groundSensorOffset;
		public Vector3 position {
			get { return m_groundSensor.position; }
			set { m_machineTransform.position = value + (m_upVector * m_groundSensorOffset); }
		}

		private Vector3 m_lastPosition;
		public Vector3 lastPosition { get { return m_lastPosition; } }

		private Quaternion m_rotation;
		protected Quaternion m_targetRotation;

		protected Vector3 m_direction;
		public Vector3 direction { get { return m_direction; } }

		protected Vector3 m_upVector;
		public Vector3 upVector { get { return m_upVector; } set { m_upVector = value;} }

		protected Vector3 m_velocity;
		public Vector3 velocity 		{ get {  return m_velocity; } }

		public Vector3 angularVelocity { get {  if (m_rbody != null) return m_rbody.angularVelocity; return Vector3.zero; } }

		protected Vector3 m_externalVelocity;
		public Vector3 externalVelocity{ get { return m_externalVelocity; } set { m_externalVelocity = value; } }

		protected float m_terminalVelocity;
		private Vector3 m_acceleration;

		private Transform m_attackTarget = null;
		public Transform attackTarget { get{ return m_attackTarget; } set{ m_attackTarget = value; } }

		private float m_latchBlending = 0f;

		private float m_timer;

		protected DragonMotion m_dragon;

		private State m_state;
		private State m_nextState;

		//--------------------------------------------------

		public sealed override void Attach (IMachine _machine, IEntity _entity, Pilot _pilot) {
			base.Attach (_machine, _entity, _pilot);

			m_rbody = m_machine.GetComponent<Rigidbody>();
			if (m_rbody != null) { // entities should not interpolate
				m_rbody.interpolation = RigidbodyInterpolation.Interpolate;
                m_rbodyConstraints = m_rbody.constraints;
            }

			m_viewControl = m_machine.GetComponent<ViewControl>();

			m_machineTransform = m_machine.transform;

			m_eye = m_machineTransform.Find("eye");
			m_hasEye = m_eye != null;

			m_groundSensor = m_machineTransform.Find("groundSensor");
			if (m_groundSensor == null) {
				m_groundSensor = m_machineTransform;
			}

			m_animEvents = m_pilot.FindComponentRecursive<PreyAnimationEvents>();
			if (m_animEvents != null) { //TODO: should we remove this when the entity is disabled?
				m_animEvents.onStandUp += new PreyAnimationEvents.OnStandUpDelegate(OnStandUp);
			}

			m_mouth = m_machineTransform.FindTransformRecursive("Fire_Dummy");

			ExtendedAttach();
		}

		public sealed override void Init() {
			switch (m_defaultUpVector) {
				case UpVector.Up: 		m_upVector = GameConstants.Vector3.up; 		break;
				case UpVector.Down: 	m_upVector = GameConstants.Vector3.down; 		break;
				case UpVector.Right: 	m_upVector = GameConstants.Vector3.right; 	break;
				case UpVector.Left: 	m_upVector = GameConstants.Vector3.left; 		break;
				case UpVector.Forward: 	m_upVector = GameConstants.Vector3.forward; 	break;
				case UpVector.Back: 	m_upVector = GameConstants.Vector3.back;		break;
			}

			m_groundSensorOffset = (m_machineTransform.position - m_groundSensor.position).magnitude;

			m_direction = GameConstants.Vector3.back;
			m_velocity = GameConstants.Vector3.zero;
			m_acceleration = GameConstants.Vector3.zero;

			if (m_mass < 1f) {
				m_mass = 1f;
			}

			m_terminalVelocity = Mathf.Sqrt((2f * m_mass * GRAVITY) * (AIR_DENSITY * 1f * DRAG));

			m_rbody.isKinematic = false;
			m_rbody.detectCollisions = true;

			//----------------------------------------------------------------------------------
			ExtendedInit();

			ExtendedUpdate();
			UpdateOrientation();
			m_rotation = m_targetRotation;
			m_machineTransform.rotation = m_rotation;

			//----------------------------------------------------------------------------------
			m_dragon = InstanceManager.player.dragonMotion;

			//----------------------------------------------------------------------------------
			m_state = State.Free;
			m_nextState = State.Free;
		}

		public void SetVelocity(Vector3 _v) {
			m_velocity = _v;
			OnSetVelocity();
		}

		public void Stop() {
			if (m_state != State.FreeFall) {
				m_velocity = GameConstants.Vector3.zero;
				m_rbody.velocity = GameConstants.Vector3.zero;
				m_rbody.angularVelocity = GameConstants.Vector3.zero;

				m_viewControl.Move(0f);
			}
		}

		public sealed override void Update() {	
			if (m_nextState != m_state) {
				ChangeState();
			}

			switch (m_state) {
				case State.Free:
					if (!m_viewControl.isHitAnimOn()) {
						ExtendedUpdate();
						UpdateOrientation();

						m_viewControl.Move(m_pilot.speed);

						UpdateAttack();

						if (m_viewControl.hasNavigationLayer) {
							m_viewControl.NavigationLayer(m_direction + GameConstants.Vector3.back * 0.1f);
						}
					}
					break;

				case State.Biting:
					m_rotation = m_machineTransform.rotation;
					break;

				case State.Latching:
					break;

				case State.Locked:
					m_direction = m_dragon.position - m_machine.position;
					m_direction.y = 0f;
					m_direction.Normalize();

					UpdateAttack();

					m_direction = Vector3.Cross(m_direction + GameConstants.Vector3.back * 0.1f, GameConstants.Vector3.down);

					m_upVector = m_machineTransform.parent.up;

					m_targetRotation = Quaternion.LookRotation(Vector3.Cross(m_direction, m_upVector), m_upVector);
					break;

				case State.Panic:
					m_rotation = m_machineTransform.rotation;
					break;

				case State.FreeFall:
					ExtendedUpdateFreeFall();
					break;
				case State.StandUp:
					m_timer -= Time.deltaTime;
					if (m_timer <= 0f) {
						OnStandUp();
					}
					break;
                case State.InLove:
                    FaceDragon();
                    UpdateOrientation();
                    m_viewControl.StopAttack();
                    break;
			}

            // Check if targeting to bend through that direction
            if (m_attackTarget) {
				Vector3 dir = m_attackTarget.position - position;
				dir.Normalize();
				m_viewControl.NavigationLayer(dir + GameConstants.Vector3.back * 0.1f);	
			} else {
				m_viewControl.NavigationLayer(m_direction + GameConstants.Vector3.back * 0.1f);	
			}

			m_viewControl.RotationLayer(ref m_rotation, ref m_targetRotation);

			m_viewControl.Boost(m_pilot.IsActionPressed(Pilot.Action.Boost));


			//----------------------------------------------------------------------------------
			// Debug
			//----------------------------------------------------------------------------------
			#if UNITY_EDITOR
			Debug.DrawLine(position, position + m_rbody.velocity, Color.yellow);
			#endif
			//----------------------------------------------------------------------------------

			CheckState();
		}

        public sealed override void FixedUpdate() {			
			switch (m_state) {
				case State.Free:
					ExtendedFixedUpdate();
					break;

				case State.FreeFall: {
						// free fall
						m_acceleration = GameConstants.Vector3.down * GRAVITY;

						float terminalVelocity = m_terminalVelocity;
						if (m_machine.GetSignal(Signals.Type.InWater)) {
							terminalVelocity *= 0.5f;
						}

						m_velocity += m_acceleration * Time.fixedDeltaTime;
						m_velocity = Vector3.ClampMagnitude(m_velocity, terminalVelocity) + m_externalVelocity;
						m_rbody.angularVelocity = GameConstants.Vector3.zero;
						m_rbody.velocity = m_velocity;
					} break;
			}

			if (m_useAngularVelocity) {
                m_rbody.angularVelocity = Util.GetAngularVelocityForRotationBlend(m_machineTransform.rotation, m_targetRotation, m_orientationSpeed);
            } else {           
                m_rotation = Quaternion.RotateTowards(m_rotation, m_targetRotation, Time.fixedDeltaTime * m_orientationSpeed);
                m_machineTransform.rotation = m_rotation;            
			}
		}

		public void LateUpdate() {
			if (m_state == State.Latching) {
				if (m_machine.GetSignal(Signals.Type.Latching)) {
					m_latchBlending += Time.deltaTime;
					Vector3 mouthOffset = (m_machineTransform.position - m_mouth.position);
					m_machineTransform.position = Vector3.Lerp(m_machineTransform.position, m_pilot.target + mouthOffset, m_latchBlending);
				}
			}

			m_lastPosition = position;
		}

		public void FreeFall() {
			if (m_state != State.FreeFall /*&& m_state != State.StandUp*/) {				
				m_viewControl.Height(100f);
				m_machine.SetSignal(Signals.Type.FallDown, true);
				m_nextState = State.FreeFall;
			}
		}

		protected float AngleBetweenRotTargetRot() {
			return Quaternion.Angle(m_rotation, m_targetRotation);
		}

		private void UpdateAttack() {
			if (m_hasEye && m_pilot.IsActionPressed(Pilot.Action.Aim)) {
				UpdateAim();
			}

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
			Transform target = m_machine.enemy;
			if (target != null) {
				Vector3 targetDir = target.position - m_eye.position;
				targetDir.z = 0f;

				targetDir.Normalize();
				Vector3 cross = Vector3.Cross(targetDir, GameConstants.Vector3.right);
				float aim = cross.z * -1;

				// Z
				float angleSide = 90f;
				if (targetDir.x < 0) {
					angleSide = 270f;
				}

				targetDir = target.position - m_eye.position;
				targetDir.y = 0f;
				targetDir.Normalize();

				cross = Vector3.Cross(targetDir, GameConstants.Vector3.right);
				float aimZ = cross.y * -1;
				float angleZ = (((aimZ - 0f) / (1f - 0f)) * (180f - angleSide)) + angleSide;

				// face target
				m_targetRotation = Quaternion.Euler(0, angleZ, 0);

				m_pilot.SetDirection(m_targetRotation * GameConstants.Vector3.forward, true);

				// blend between attack directions
				m_viewControl.Aim(aim);
			}
		}

		private void OnStandUp() {
			if (m_state == State.StandUp) {
				m_nextState = State.Free;
			}
		}

		private void CheckState() {
			if (m_state != State.StandUp) {
				bool canMove = !m_machine.GetSignal(Signals.Type.Biting | Signals.Type.Latching | Signals.Type.LockedInCage | Signals.Type.Panic | Signals.Type.FallDown | Signals.Type.InLove);

				if (canMove) {
					if (m_state == State.FreeFall) {
						m_nextState = State.StandUp;
					} else {
						m_nextState = State.Free;
					}
				} else {
					if 		(m_machine.GetSignal(Signals.Type.LockedInCage)) m_nextState = State.Locked;
					else if	(m_machine.GetSignal(Signals.Type.FallDown)) 	 m_nextState = State.FreeFall;
                    else if (m_machine.GetSignal(Signals.Type.InLove))       m_nextState = State.InLove;
					else if (m_machine.GetSignal(Signals.Type.Panic)) 		 m_nextState = State.Panic;
					else if (m_machine.GetSignal(Signals.Type.Biting)) 		 m_nextState = State.Biting;
					else if (m_machine.GetSignal(Signals.Type.Latching)) 	 m_nextState = State.Latching;
				}
			}
		}

		private void ChangeState() {
			// Leave current state
			switch (m_state) {
				case State.Free:					
					break;

				case State.Biting:
					break;

				case State.Latching:
					Stop();
					m_rbody.isKinematic = false;
					m_rbody.detectCollisions = true;	
					m_latchBlending = 0f;
					break;

				case State.Locked:
                    m_rbody.useGravity = false;
                    m_viewControl.Scared(false);
					OnCollisionGroundExit(null);
					break;

				case State.Panic:
					m_viewControl.Panic(false, m_machine.GetSignal(Signals.Type.Burning));					
					break;

				case State.FreeFall:			
					m_viewControl.Falling(false);
					break;
			}

			// change state
			m_state = m_nextState;

			// Enter next state
			switch (m_state) {
				case State.Free:
					break;

				case State.Biting:
					break;

				case State.Latching:
					m_rbody.isKinematic = true;
					m_rbody.detectCollisions = false;
					break;

				case State.Locked:
                    m_rbody.useGravity = true;
                    m_viewControl.Scared(true);
                    Stop();
                    break;

				case State.Panic:
					m_viewControl.Panic(true, m_machine.GetSignal(Signals.Type.Burning));
					break;

				case State.FreeFall:
					OnFreeFall();
					m_viewControl.Falling(true);
					break;

				case State.StandUp:
					Stop();
					m_timer = 2f; // fallback timer, if we don't have an event in our animations
					break;

                case State.InLove:
                    m_viewControl.StopAttack();
                    break;
            }
		}

		//--------------------------------------------------
		// Queries
		//--------------------------------------------------
		public bool IsInFreeFall() { return m_state == State.FreeFall; }


		//--------------------------------------------------
		// Extend functionality
		//--------------------------------------------------
		protected abstract void ExtendedAttach();
		protected abstract void ExtendedInit();

		protected abstract void ExtendedUpdate();
		protected abstract void ExtendedFixedUpdate();

		protected abstract void OnFreeFall();
		protected abstract void ExtendedUpdateFreeFall();
        
        protected abstract void FaceDragon();

		protected abstract void UpdateOrientation();
		protected abstract void OnSetVelocity();

		public abstract void OnCollisionGroundEnter(Collision _collision);
		public abstract void OnCollisionGroundStay(Collision _collision);
		public abstract void OnCollisionGroundExit(Collision _collision);
		//--------------------------------------------------
	}
}