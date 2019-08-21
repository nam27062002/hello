using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PursuitEntityTargetData : StateComponentData {
			public float speed = 1.5f;
			public string attackPoint;
			public float chaseTimeout;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit Entity Target")]
		public class PursuitEntityTarget : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onCollisionDetected = UnityEngine.Animator.StringToHash("onCollisionDetected");

			[StateTransitionTrigger]
			private static readonly int onEnemyOutOfSight = UnityEngine.Animator.StringToHash("onEnemyOutOfSight");

			[StateTransitionTrigger]
			private static readonly int onChaseTimeOut = UnityEngine.Animator.StringToHash("onChaseTimeout");


			protected PursuitEntityTargetData m_data;
			protected Transform m_target;
			protected AI.IMachine m_targetMachine;
			protected Entity m_targetEntity;
			protected MachineEatBehaviour m_eatBehaviour;

			protected float m_timer;


			public override StateComponentData CreateData() {
				return new PursuitEntityTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PursuitEntityTargetData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PursuitEntityTargetData>();
				m_eatBehaviour = m_pilot.GetComponent<MachineEatBehaviour>();

				m_machine.SetSignal(Signals.Type.Alert, true);
				m_target = null;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(false);

				m_target = null;
				m_targetMachine = null;
				m_targetEntity = null;

				if (param != null && param.Length > 0) {
					m_target = param[0] as Transform;
					if (m_target) {
						m_targetEntity = m_target.GetComponent<Entity>();
						m_targetMachine = m_targetEntity.machine;
					}
				}

				if (m_target == null && m_machine.enemy != null) {
					m_target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
					if (m_target == null) {
						m_target = m_machine.enemy;
					}

					m_targetEntity = m_machine.enemy.GetComponent<Entity>();
					m_targetMachine = m_machine.enemy.GetComponent<IMachine>();
				}

				m_eatBehaviour.enabled = true;

				m_timer = 0;
			}

			protected override void OnExit(State _newState) {				
				m_target = null;
				m_targetMachine = null;
				m_targetEntity = null;	

				m_eatBehaviour.enabled = false;
			}

			protected override void OnUpdate() {
				if (m_targetMachine != null) {
					if (!m_targetMachine.CanBeBitten()) {
						m_target = null;
						m_targetMachine = null;
						m_targetEntity = null;
					}
				}

				// if collides with ground then -> recover/loose sight
				if (m_machine.GetSignal(Signals.Type.Collision)) {
					object[] param = m_machine.GetSignalParams(Signals.Type.Collision);
					if (param.Length > 0) {
						Collision collision = param[0] as Collision;
						if (collision != null) {
							if (collision.collider.gameObject.layer == LayerMask.NameToLayer("ground")) {	
								// We go back
								Transition(onCollisionDetected);
								return;
							}
						}
					}
				}
									
				if (m_target != null && m_target.gameObject.activeInHierarchy) {
					// if eating move forward only
					if (m_eatBehaviour != null && m_eatBehaviour.IsEating()) {
						m_pilot.SlowDown(true);
					} else {
						m_pilot.SlowDown(false);

						m_timer += Time.deltaTime;
						if (m_timer >= m_data.chaseTimeout) {							
							Transition(onChaseTimeOut);
						} else {
							// if not eating check chase timeout
							Vector3 pos;
							// Chase
							if (m_targetEntity != null) {
								pos = m_targetEntity.circleArea.center;						
							} else {
								pos = m_target.position;
							}
							float magnitude = (pos - m_pilot.transform.position).sqrMagnitude;
							if (magnitude < m_data.speed * 0.25f) // !!!
								magnitude = m_data.speed * 0.25f;
							m_pilot.SetMoveSpeed(Mathf.Min(m_data.speed, magnitude));
							m_pilot.GoTo(pos);
						}
					}
				} else {					
					Transition(onEnemyOutOfSight);
				}
			}
		}
	}
}