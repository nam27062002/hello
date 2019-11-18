using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionGround : MC_Motion {
		//--------------------------------------------------
		private static float FREE_FALL_THRESHOLD = 0.35f;

		//--------------------------------------------------
		private enum SubState {
			Idle = 0,
			Move,
			Jump_Start,
			Jump_Up,
			Jump_Down
		};


		//--------------------------------------------------
		[SeparatorAttribute("Orientation")]
		[SerializeField] private bool m_faceDirection = false;
		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField] private float m_faceLeftAngle = -90f;
		[SerializeField] private float m_faceRightAngle = 90f;


		//--------------------------------------------------
		private Vector3 m_groundNormal;
		private Vector3 m_groundDirection;
		public Vector3 groundDirection { get { return m_groundDirection; } }

		private Vector3 m_gravity;

		private RaycastHit[] m_raycastHits;

		private bool m_onGround;
		public bool onGround{ get{return m_onGround;} }
		private float m_heightFromGround;

		private float m_jumpStartY;
		private float m_jumpUpDistance;

		private float m_fallTimer;

		private SubState m_subState;
		private SubState m_nextSubState;


		//--------------------------------------------------
		protected override void ExtendedAttach() {
			m_raycastHits = new RaycastHit[255];
		}

		protected override void ExtendedInit() {
			m_onGround = false;

			GetGroundNormal(0.3f);
			Ray ray = new Ray();
			ray.origin = position + m_upVector * 0.1f;
			ray.direction = -m_groundNormal;

			int hits = Physics.RaycastNonAlloc(ray, m_raycastHits, 5f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE);
            for (int i = 0; i < hits; ++i) {
                if (!m_raycastHits[i].collider.isTrigger) {
                    position = m_raycastHits[0].point;
                    m_heightFromGround = 0f;
                    m_viewControl.Height(0f);
                    m_onGround = true;
                }
            }

			m_gravity = GameConstants.Vector3.zero;
			m_fallTimer = FREE_FALL_THRESHOLD;

			m_subState = SubState.Idle;
			m_nextSubState = SubState.Idle;
		}

		protected override void ExtendedUpdate() {
			if (m_nextSubState != m_subState) {
				ChangeState();
			}

			if (m_faceDirection) {
				m_direction = m_velocity.normalized;
			} else {
				m_direction = m_pilot.direction;

				if (!m_pilot.IsActionPressed(Pilot.Action.Stop)) {
					m_direction = (m_direction.x >= 0)? GameConstants.Vector3.right : GameConstants.Vector3.left;
				}
			}

			switch (m_subState) {
				case SubState.Idle:
					if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
						m_nextSubState = SubState.Jump_Start;
					} else if (m_pilot.speed > 0.01f) {						
						m_nextSubState = SubState.Move;
					}
					break;

				case SubState.Move:					
					if (m_pilot.IsActionPressed(Pilot.Action.Jump)) {
						m_nextSubState = SubState.Jump_Start;
					} else if (m_pilot.speed <= 0.01f) {
						m_nextSubState = SubState.Idle;
					}
					break;

				case SubState.Jump_Start:
					break;

				case SubState.Jump_Up:
					if (m_velocity.y < 0f) {
						m_nextSubState = SubState.Jump_Down;
					}
					break;

				case SubState.Jump_Down:
					if (m_onGround) {
						m_pilot.ReleaseAction(Pilot.Action.Jump);
						m_nextSubState = SubState.Idle;
					} else {
						float jumpDownDistance = m_jumpStartY - m_machine.position.y;
						if (jumpDownDistance > m_jumpUpDistance * 1.25f) {
							// force to start a free fall
							FreeFall();
						}
					}
					break;
			}

			if (m_subState <= SubState.Move) {
				if (!m_onGround) {
					m_fallTimer -= Time.deltaTime;
					if (m_fallTimer <= 0f) {						
						FreeFall();
						m_fallTimer = FREE_FALL_THRESHOLD;
					}
				}

				//m_onGround = false;
			}
		}

		protected override void ExtendedFixedUpdate() {
			Vector3 gv = GameConstants.Vector3.down * GRAVITY * Time.fixedDeltaTime;

			if (m_subState >= SubState.Jump_Start && m_subState <= SubState.Jump_Down) {
				// ----------------------------- gravity :3
				m_velocity += gv;
				//m_rbody.angularVelocity = GameConstants.Vector3.zero;
				m_rbody.velocity = m_velocity;
			} else {
				if (m_groundDirection.y < -0.25f && m_direction.x > 0f
				||  m_groundDirection.y > 0.25f && m_direction.x < 0f) {
					gv *= 25f * Mathf.Abs(m_groundDirection.y);
				}

				m_gravity += gv;

				if (m_subState == SubState.Idle) {
					m_rbody.velocity = Vector3.ClampMagnitude(m_gravity, m_terminalVelocity);
				} else {
					if (m_mass != 1f) {
						Vector3 impulse = (m_pilot.impulse - m_velocity);
						impulse /= m_mass;
						m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
					} else {
						m_velocity = m_pilot.impulse;
					}

					//lets clamp velocity while entity is turning around
					/*float angle = AngleBetweenRotTargetRot();
					float factor = (((angle - 180f) / (0f - 180f)) * (1f - 0.25f)) + 0.25f;
					m_velocity *= factor;*/

					m_rbody.angularVelocity = GameConstants.Vector3.zero;
					m_rbody.velocity = Vector3.ClampMagnitude(m_velocity + m_externalVelocity + m_gravity, m_terminalVelocity);
				}
			}
		}

		protected override void OnFreeFall() { 
			m_velocity *= 0.5f;
		}

		protected override void ExtendedUpdateFreeFall() {
			if (m_faceDirection) {
				m_direction = m_velocity.normalized;
				UpdateOrientation();
			}

			if (m_onGround) {
				m_fallTimer = FREE_FALL_THRESHOLD;
				m_pilot.ReleaseAction(Pilot.Action.Jump);
				m_machine.SetSignal(Signals.Type.FallDown, false);
				m_viewControl.Height(0f);
				m_nextSubState = SubState.Idle;
			}
		}
        
        protected override void FaceDragon() {
            m_direction = m_dragon.position - m_machine.position;
            m_direction.y = 0;
            m_direction.Normalize();
        }
        

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + GameConstants.Vector3.back * 0.1f, m_upVector);

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	 m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_upVector) * m_targetRotation; 
				else if (m_direction.x > 0f) m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_upVector) * m_targetRotation; 
			}
		}

		protected override void OnSetVelocity() {
			if (m_subState == SubState.Jump_Start) {				
				m_nextSubState = SubState.Jump_Up;
			}
		}

		private Vector3 GetGroundNormal(float _onGroundHeight) {
			Vector3 normal = GameConstants.Vector3.up;
			Vector3 hitPos = position;
			Vector3 pos = position + (m_upVector * 3f);

			Ray ray = new Ray();
			ray.origin = pos;
			ray.direction = -m_upVector;

            m_heightFromGround = 100f;

            int hits = Physics.RaycastNonAlloc(ray, m_raycastHits, 6f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE);
            for (int i = 0; i < hits; ++i) {
                if (!m_raycastHits[i].collider.isTrigger) {
                    normal = m_raycastHits[0].normal;
                    hitPos = m_raycastHits[0].point;
                    m_heightFromGround = m_raycastHits[0].distance - 3f;
                }
            }

			if (m_heightFromGround < 0.3f) {
				m_gravity = GameConstants.Vector3.zero;
			}

			m_onGround = m_heightFromGround < _onGroundHeight;
			m_groundNormal = normal;

			m_groundDirection = Vector3.Cross(GameConstants.Vector3.back, m_groundNormal);

			m_viewControl.Height(m_heightFromGround);

			return hitPos;
		}

		private void ChangeState() {
			// leave current state
			switch (m_subState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					Stop();
					break;

				case SubState.Jump_Start:
					break;

				case SubState.Jump_Up:
					break;

				case SubState.Jump_Down:
					m_heightFromGround = 0f;
					m_viewControl.Height(0f);
					m_onGround = true;
					m_viewControl.Jumping(false);
					Stop();
					break;
			}

			// enter next state
			switch (m_nextSubState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					break;

				case SubState.Jump_Start:
					m_jumpStartY = m_machine.position.y;
					m_jumpUpDistance = 0f;

					m_heightFromGround = 0f;
					m_viewControl.Height(0f);
					m_onGround = true;
					m_viewControl.Jumping(true);
					Stop();
					break;

				case SubState.Jump_Up:
					break;

				case SubState.Jump_Down:					
					m_jumpUpDistance = m_machine.position.y - m_jumpStartY;
					m_jumpStartY = m_machine.position.y;
					break;
			}

			m_subState = m_nextSubState;
		}

		//--------------------------------------------------
		//--------------------------------------------------
		public override void OnCollisionGroundEnter(Collision _collision) {
			OnCollisionGroundStay(_collision);
		}

		public override void OnCollisionGroundStay(Collision _collision) {
            ContactPoint[] _contacts = _collision.contacts;
            int _count = _contacts.Length;
            for (int i = 0; i < _count; i++) {
				Vector3 hitPoint = _contacts[i].point;
				float error = (hitPoint - position).sqrMagnitude;

				if (error <= 0.3f) {					
					m_groundNormal = _contacts[i].normal;
					m_groundDirection = Vector3.Cross(GameConstants.Vector3.back, m_groundNormal);

					m_gravity = GameConstants.Vector3.zero;
					m_fallTimer = FREE_FALL_THRESHOLD;

					m_heightFromGround = 0f;
					m_viewControl.Height(0f);

					m_onGround = true;
					break;
				}
			}
			/*
			m_heightFromGround = 100f;
			m_viewControl.Height(100f);*/
		}

		public override void OnCollisionGroundExit(Collision _collision) {
			m_onGround = false;
			m_heightFromGround = 100f;
			m_viewControl.Height(100f);
		}
	}
}