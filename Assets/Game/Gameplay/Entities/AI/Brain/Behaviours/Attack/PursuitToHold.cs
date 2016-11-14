﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class PursuitToHoldData : StateComponentData {
			public float speed;
			public float arrivalRadius = 1f;
			public Range timeout = new Range(4,6);
			public Range onFailShutdown = new Range(4,6);
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit To Hold")]
		public class PursuitToHold : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			[StateTransitionTrigger]
			private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

			[StateTransitionTrigger]
			private static string OnPursuitTimeOut = "onPursuitTimeOut";

			protected PursuitToHoldData m_data;

			protected AI.Machine m_targetMachine;
			protected Entity m_targetEntity;
			protected DragonPlayer m_player;
			protected float m_timer;
			protected float m_timeOut;

			private EatBehaviour m_eatBehaviour;

			private bool m_enemyInRange = false;



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

				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();
				m_eatBehaviour.enabled = false;
				m_eatBehaviour.onJawsClosed += OnBiteKillEvent;
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

				m_eatBehaviour.enabled = true;
				m_enemyInRange = false;
				m_timer = 0;
				m_timeOut = m_data.timeout.GetRandom();

				//m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State _newState) {
				//m_pilot.ReleaseAction(Pilot.Action.Button_A);
				m_enemyInRange = false;
			}

			void OnBiteKillEvent()
			{
				if ( m_eatBehaviour.IsLatching() )
				{
					m_enemyInRange = true;

				}
			}

			protected override void OnUpdate() {
				if (m_enemyInRange)	{
					Transition(OnEnemyInRange);
					m_enemyInRange = false;
					return;
				}
				if (m_targetMachine != null) {
					if ( m_targetMachine.IsDead() || m_targetMachine.IsDying()) {
						m_targetMachine = null;
						m_targetEntity = null;
						m_transitionParam[0] = m_data.onFailShutdown.GetRandom();
						m_eatBehaviour.enabled = false;
						Transition(OnEnemyOutOfSight, m_transitionParam);
					}
				} else if ( m_player != null ){

					bool canLatch = false;
					if ( m_eatBehaviour.canMultipleLatchOnPlayer && InstanceManager.player.HasFreeHoldPoint())
						canLatch = true;
					else
						canLatch = !InstanceManager.player.BeingLatchedOn();

					if ( !m_player.IsAlive() || !canLatch )
					{
						m_transitionParam[0] = m_data.onFailShutdown.GetRandom();
						m_eatBehaviour.enabled = false;
						Transition(OnEnemyOutOfSight, m_transitionParam);
					}
				}



				Transform m_target = null;
				if ( m_targetMachine != null)
					m_target = SearchClosestHoldPoint( m_targetMachine.holdPreyPoints );
				else
					m_target = SearchClosestHoldPoint( m_player.holdPreyPoints );


				if ( m_target == null )	{
					m_transitionParam[0] = m_data.onFailShutdown.GetRandom();
					m_eatBehaviour.enabled = false;
					Transition( OnPursuitTimeOut, m_transitionParam );
				}

				float m = (m_machine.position - m_target.position).sqrMagnitude;
				if (m < m_data.arrivalRadius * m_data.arrivalRadius) {
				//	m_transitionParam[0] = m_target;
				//	Transition(OnEnemyInRange, m_transitionParam);
				} else {
					m_timer += Time.deltaTime;
					if ( m_timer > m_timeOut )
					{
						m_transitionParam[0] = m_data.onFailShutdown.GetRandom();
						m_eatBehaviour.enabled = false;
						Transition( OnPursuitTimeOut, m_transitionParam );
					}
					else
					{
						if ( m_targetEntity != null )
							m_pilot.GoTo(m_targetEntity.circleArea.center);	
						else
							m_pilot.GoTo(m_target.position);
					}
				}
									
				
			}

			virtual protected Transform SearchClosestHoldPoint( HoldPreyPoint[] holdPreyPoints )
			{
				float distance = float.MaxValue;
				Transform holdTransform = null;
				for( int i = 0; i<holdPreyPoints.Length; i++ )
				{
					HoldPreyPoint point = holdPreyPoints[i];
					if ( !point.holded && Vector3.SqrMagnitude( m_machine.position - point.transform.position) < distance )
					{
						distance = Vector3.SqrMagnitude( m_machine.position - point.transform.position);
						holdTransform = point.transform;
					}
				}
				return holdTransform;
			}


		}
	}
}