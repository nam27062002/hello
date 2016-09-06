using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PursuitToHoldData : StateComponentData {
			public float speed;
			public float arrivalRadius = 1f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit To Hold")]
		public class PursuitToHold : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

			protected PursuitToHoldData m_data;

			protected AI.Machine m_targetMachine;
			protected Entity m_targetEntity;
			protected DragonPlayer m_player;


			private object[] m_transitionParam;

			public override StateComponentData CreateData() {
				return new PursuitToHoldData();
			}

			public override System.Type GetDataType() {
				return typeof(PursuitToHoldData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PursuitToHoldData>();

				m_machine.SetSignal(Signals.Type.Alert, true);
				m_transitionParam = new object[1];
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(false);

				m_targetMachine = null;
				m_targetEntity = null;
				m_player = null;

				Transform toLookAt = null;

				if ( param != null && param.Length > 0 )
				{
					toLookAt = param[0] as Transform;
					m_targetMachine = toLookAt.GetComponent<Machine>();
					m_targetEntity = toLookAt.GetComponent<Entity>();
					m_player = toLookAt.GetComponent<DragonPlayer>();
				}
				else
				{
					m_player = InstanceManager.player;
				}

			}

			protected override void OnUpdate() {	
				if (m_targetMachine != null) {
					if ( m_targetMachine.IsDead() || m_targetMachine.IsDying()) {
						m_targetMachine = null;
						m_targetEntity = null;
						Transition(OnEnemyOutOfSight);
					}
				} else if ( m_player != null ){
					if ( !m_player.IsAlive() || m_player.BeingLatchedOn() )
					{
						Transition(OnEnemyOutOfSight);
					}
				}

				Transform m_target = null;
				if ( m_targetMachine != null)
					m_target = SearchClosestHoldPoint( m_targetMachine.holdPreyPoints );
				else
					m_target = SearchClosestHoldPoint( m_player.holdPreyPoints );

				float m = (m_machine.position - m_target.position).sqrMagnitude;
				if (m < m_data.arrivalRadius * m_data.arrivalRadius) {
					m_transitionParam[0] = m_target;
					Transition(OnEnemyInRange, m_transitionParam);
				} else {
					if ( m_targetEntity != null )
						m_pilot.GoTo(m_targetEntity.circleArea.center);	
					else
						m_pilot.GoTo(m_target.position);
				}
									
				
			}

			virtual protected Transform SearchClosestHoldPoint( List<Transform> holdPreyPoints )
			{
				float distance = float.MaxValue;
				List<Transform> points = holdPreyPoints;
				Transform holdTransform = null;
				for( int i = 0; i<points.Count; i++ )
				{
					if ( Vector3.SqrMagnitude( m_machine.position - points[i].position) < distance )
					{
						distance = Vector3.SqrMagnitude( m_machine.position - points[i].position);
						holdTransform = points[i];
					}
				}
				return holdTransform;
			}


		}
	}
}