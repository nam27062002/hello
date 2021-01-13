using UnityEngine;
using System;

namespace AI {
	[Serializable]
	public sealed class MC_MotionWallWalking : MC_Motion {

		//--------------------------------------------------
		private enum SubState {
			Idle = 0,
			Move
		};


		//--------------------------------------------------
		[SeparatorAttribute("Orientation")]
		[SerializeField] private bool m_limitHorizontalRotation = false;
		[SerializeField] private float m_faceLeftAngle = -90f;
		[SerializeField] private float m_faceRightAngle = 90f;


		//--------------------------------------------------
		private Vector3 m_upVectorSave;
		private Vector3 m_groundNormalSave;
		private Vector3 m_groundDirectionSave;

		private Vector3 m_groundNormal;
		private Vector3 m_groundDirection;
		public Vector3 groundDirection { get { return m_groundDirection; } }

		private Vector3 m_gravity;

		private RaycastHit[] m_raycastHits;
        private RaycastHit[] m_hitResults;
        private bool[] m_hasHit;

		private bool m_onGround;
		private float m_heightFromGround;

		private bool m_checkCollisions;
		public bool checkCollisions { set { 
				if (m_checkCollisions != value) {
					if (value) {
						m_groundNormal = m_groundNormalSave;
						m_upVector = m_upVectorSave;
						m_groundDirection = m_groundDirectionSave;
					} else {
						m_groundNormalSave = m_groundNormal;
						m_groundNormal = Vector3.up;

						m_upVectorSave = m_upVector;
						m_upVector = Vector3.up;

						m_groundDirectionSave = m_groundDirection;
						m_groundDirection = Vector3.Cross(Vector3.back, m_upVector);
					}
					m_checkCollisions = value;
				}
			}
		}

		private SubState m_subState;
		private SubState m_nextSubState;


		//--------------------------------------------------
		protected override void ExtendedAttach() {
			m_raycastHits = new RaycastHit[255];
            m_hitResults = new RaycastHit[4];
            m_hasHit = new bool[4];
		}
		
		protected override void ExtendedInit() {
			m_onGround = false;
			m_checkCollisions = true;

			//find the nearest wall and use that up vector
			FindUpVector();

			if (!m_onGround) {
				m_upVector = GameConstants.Vector3.up;
				FreeFall();
			}

			m_groundDirection = Vector3.Cross(GameConstants.Vector3.back, m_upVector);

			m_gravity = GameConstants.Vector3.zero;

			m_subState = SubState.Idle;
			m_nextSubState = SubState.Idle;
		}

		protected override void ExtendedUpdate() {
			if (m_nextSubState != m_subState) {
				ChangeState();
			}

			m_direction = m_pilot.direction;

			switch (m_subState) {
				case SubState.Idle:
					if (m_pilot.speed > 0.01f) {
						m_nextSubState = SubState.Move;
					}
					break;

				case SubState.Move:
					if (m_pilot.speed <= 0.01f) {
						m_nextSubState = SubState.Idle;
					}
					break;
			}

			#if UNITY_EDITOR
			Debug.DrawRay(position, m_groundNormal, Color.red, 1f);
			Debug.DrawRay(position, m_upVector, Color.green, 1f);
			#endif
		}

		protected override void ExtendedFixedUpdate() {
			Vector3 gravityDir = -m_groundNormal;

			if (m_checkCollisions) {
				m_gravity += gravityDir * GRAVITY * Time.fixedDeltaTime;
			} else {
				m_gravity = GameConstants.Vector3.zero;
			}

			if (m_subState == SubState.Idle) {
				m_rbody.velocity = m_gravity;
			} else {
				if (m_mass != 1f) {
					Vector3 impulse = (m_pilot.impulse - m_velocity);
					impulse /= m_mass;
					m_velocity = Vector3.ClampMagnitude(m_velocity + impulse, m_pilot.speed);
				} else {
					m_velocity = m_pilot.impulse;
				}

				m_rbody.velocity = m_velocity + m_externalVelocity + m_gravity;
			}
		}

		protected override void OnFreeFall() { }
		protected override void ExtendedUpdateFreeFall() {			
			if (m_onGround) {
				m_machine.SetSignal(Signals.Type.FallDown, false);
				FindUpVector();
				m_nextSubState = SubState.Idle;
			}
		}
        
        protected override void FaceDragon() {
            m_direction = m_dragon.position - m_machine.position;
            m_direction.Normalize();
            m_direction = m_direction - Vector3.Dot(m_direction ,m_groundNormal) * m_groundNormal ;
            m_direction.Normalize();
            
        }

		protected override void UpdateOrientation() {
			m_targetRotation = Quaternion.LookRotation(m_direction + GameConstants.Vector3.back * 0.1f, m_groundNormal);

			if (m_limitHorizontalRotation) {
				if (m_direction.x < 0f) 	 m_targetRotation = Quaternion.AngleAxis(m_faceLeftAngle, m_groundNormal) * m_targetRotation; 
				else if (m_direction.x > 0f) m_targetRotation = Quaternion.AngleAxis(m_faceRightAngle, m_groundNormal) * m_targetRotation; 
			}
		}

		private Vector3 GetGroundNormal() {	
			Vector3 hitPos = position;

			if (m_checkCollisions) {
				Vector3 normal = m_upVector;
				Vector3 pos = position + (m_upVector * 2f);

				if (m_subState == SubState.Move) {
					// we'll check forward
					Vector3 dir = m_direction;
					dir.z = 0f;
					pos += dir * 0.15f;
				}

				Ray ray = new Ray();
				ray.origin = pos;
				ray.direction = -m_upVector;

				int hitCount = Physics.RaycastNonAlloc(ray, m_raycastHits, 6f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE);
				if (hitCount > 0) {
					RaycastHit hit = m_raycastHits[0];
					normal = (hit.normal * 0.75f) + (m_groundNormal * 0.25f);
					hitPos = hit.point;
					m_heightFromGround = hit.distance - 3f;
				} else {
					m_heightFromGround = 100f;
				}

				if (m_heightFromGround < 0.3f) {
					m_gravity = GameConstants.Vector3.zero;
				}

				m_onGround = m_heightFromGround < 0.3f;
				m_groundNormal = normal;
				m_upVector = normal;

				m_groundDirection = Vector3.Cross(GameConstants.Vector3.back, m_upVector);
			}

			return hitPos;
		}

		private void FindUpVector() {			
			Ray ray = new Ray();
			ray.origin = position;

            for (int i = 0; i < 4; i++) {
                m_hasHit[i] = false;
            }

			//down
			ray.direction = GameConstants.Vector3.down;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[0] = m_raycastHits[0]; m_hasHit[0] = true; }

			//up
			ray.direction = GameConstants.Vector3.up;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[1] = m_raycastHits[0]; m_hasHit[1] = true; }

			//right
			ray.direction = GameConstants.Vector3.right;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[2] = m_raycastHits[0]; m_hasHit[2] = true; }

			//left
			ray.direction = GameConstants.Vector3.left;
            if (Physics.RaycastNonAlloc(ray, m_raycastHits, 10f, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) > 0) { m_hitResults[3] = m_raycastHits[0]; m_hasHit[3] = true; }

			float d = 99999f;
			for (int i = 0; i < 4; i++) {
                if (m_hasHit[i]) {
                    if (m_hitResults[i].distance < d) {
                        d = m_hitResults[i].distance;

                        m_upVector = m_hitResults[i].normal;
                        m_groundNormal = m_hitResults[i].normal;
                        position = m_hitResults[i].point;

						m_heightFromGround = 0f;
						m_onGround = true;
					}
				}
			}
		}

		private void ChangeState() {
			switch (m_subState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					Stop();
					break;
			}

			switch (m_nextSubState) {
				case SubState.Idle:
					break;

				case SubState.Move:
					break;
			}

			m_subState = m_nextSubState;
		}

		//--------------------------------------------------
		//--------------------------------------------------		
		protected override void OnSetVelocity() {}

		public override void OnCollisionGroundEnter(Collision _collision) {
			OnCollisionGroundStay(_collision);
		}

		public override void OnCollisionGroundStay(Collision _collision) {
			Vector3 groundNormal = GameConstants.Vector3.zero;
            ContactPoint[] _contacts = _collision.contacts;
            int _count = _contacts.Length;
			for (int i = 0; i < _count; i++) {
				groundNormal += _contacts[i].normal;
			}
			groundNormal.Normalize();
			m_groundNormal = m_groundNormal * 0.25f + groundNormal * 0.75f;
			m_groundNormal.Normalize();
			m_groundDirection = Vector3.Cross(GameConstants.Vector3.back, m_groundNormal);

			m_gravity = GameConstants.Vector3.zero;

			m_heightFromGround = 0f;
			m_viewControl.Height(0f);

			m_viewControl.UpsideDown(m_groundNormal.y < -0.5);

			m_onGround = true;
		}

		public override void OnCollisionGroundExit(Collision _collision) {
			m_onGround = false;

			m_heightFromGround = 100f;
			m_viewControl.Height(100f);
		}
	}
}