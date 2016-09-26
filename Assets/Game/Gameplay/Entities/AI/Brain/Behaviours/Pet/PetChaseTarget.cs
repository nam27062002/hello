using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PetChaseTargetData : StateComponentData {
			public float speed;
			public string attackPoint;
			public float chaseTimeout;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Chase Target")]
		public class PetChaseTarget : StateComponent {

			[StateTransitionTrigger]
			private static string OnCollisionDetected = "onCollisionDetected";

			[StateTransitionTrigger]
			private static string OnChaseTimeOut = "onChaseTimeout";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

			protected PetChaseTargetData m_data;
			protected Transform m_target;
			protected AI.Machine m_targetMachine;
			protected Entity m_targetEntity;
			protected MachineEatBehaviour m_eatBehaviour;
			protected float m_timer;

			private object[] m_transitionParam;

			public override StateComponentData CreateData() {
				return new PetChaseTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetChaseTargetData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetChaseTargetData>();
				m_eatBehaviour = m_pilot.GetComponent<MachineEatBehaviour>();
				m_eatBehaviour.PauseEating();

				m_machine.SetSignal(Signals.Type.Alert, true);
				m_transitionParam = new object[1];
				m_target = null;

			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(true);

				m_target = null;
				m_targetMachine = null;
				m_targetEntity = null;

				if ( param != null && param.Length > 0 )
				{
					m_target = param[0] as Transform;
					if ( m_target )
						m_targetMachine = m_target.GetComponent<Machine>();
					if ( m_target )
						m_targetEntity = m_target.GetComponent<Entity>();
				}

				if (m_target == null && m_machine.enemy != null) {
					m_target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
					if (m_target == null) {
						m_target = m_machine.enemy;
					}

					m_targetMachine = m_machine.enemy.GetComponent<Machine>();
					m_targetEntity = m_machine.enemy.GetComponent<Entity>();
				}

				if ( m_targetMachine != null )
					m_targetMachine.isPetTarget = true;

				m_eatBehaviour.ResumeEating();
				m_timer = 0;
			}

			protected override void OnExit(State _newState)
			{
				if ( m_targetMachine != null )
					m_targetMachine.isPetTarget = false;
				m_eatBehaviour.PauseEating();

				m_target = null;
				m_targetMachine = null;
				m_targetEntity = null;	
			}

			protected override void OnUpdate() {	

				if (m_targetMachine != null) {
					if ( !m_targetMachine.CanBeBitten()) {
						m_target = null;
						m_targetMachine = null;
						m_targetEntity = null;
					}
				}

				// if collides with ground then -> recover/loose sight
				if (m_machine.GetSignal(Signals.Type.Collision))
				{
					object[] param = m_machine.GetSignalParams(Signals.Type.Collision);
					if ( param.Length > 0 )
					{
						Collision collision = param[0] as Collision;
						if ( collision != null )
						{
							if ( collision.collider.gameObject.layer == LayerMask.NameToLayer("ground") )
							{	
								// We go back
								Transition(OnCollisionDetected);
								return;
							}
						}
					}
				}
									
				if (m_target != null && m_target.gameObject.activeInHierarchy) {

					// if eating move forward only
					if ( m_eatBehaviour.IsEating() )
					{
						// m_pilot.GoTo( m_machine.transform.position + m_machine.transform.forward * m_data.speed * 0.5f);
					}
					else
					{
						// if not eating check chase timeout
						m_timer += Time.deltaTime;
						if ( m_timer >= m_data.chaseTimeout )
						{
							Transition(OnChaseTimeOut);
						}
						else
						{
							// Chase
							if (m_targetEntity != null) {
								m_pilot.GoTo(m_targetEntity.circleArea.center);	
							} else {
								m_pilot.GoTo(m_target.position);
							}
							
						}
					}

				} else {
					Transition(OnEnemyOutOfSight);
				}
			}
		}
	}
}