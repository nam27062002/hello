using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class LatchData : StateComponentData {
			public Range stunTime = new Range( 4,6 );
			public float damage = 1;
			public float duration = 2;

		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Latch On Player")]
		public class LatchOnPlayer : StateComponent {

			[StateTransitionTrigger]
			private static string OnBiteFail = "onLatchFail";
			[StateTransitionTrigger]
			private static string OnEndLatching = "onEndLatching";

			private EatBehaviour m_eatBehaviour;

			private LatchData m_data;
			private object[] m_transitionParam;

			private Transform m_parent;
			private float m_timer;

			private Transform m_holdTransform;
			private Transform m_originalParent;
			private Machine m_myMachine;

			//--------------------------------------------------------
			public override StateComponentData CreateData() {
				return new LatchData();
			}

			public override System.Type GetDataType() {
				return typeof(LatchData);
			}

			protected override void OnInitialise() {

				m_myMachine = m_pilot.GetComponent<AI.Machine>();
				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();
				m_eatBehaviour.enabled = false;
				m_eatBehaviour.onBiteKill += OnBiteKillEvent;
				m_eatBehaviour.onEndLatching += OnEndLatchingEvent;

				m_data = m_pilot.GetComponentData<LatchData>();
				m_eatBehaviour.holdDamage = m_data.damage;
				m_eatBehaviour.holdDuration = m_data.duration;

				m_transitionParam = new object[1];
				m_transitionParam[0] = m_data.stunTime.GetRandom(); // retreat time

				base.OnInitialise();
			}

			protected override void OnEnter(State oldState, object[] param) {

				base.OnEnter(oldState, param);
				// Get Target!
				m_eatBehaviour.StartAttackTarget( InstanceManager.player.transform);
				m_eatBehaviour.enabled = true;
				m_eatBehaviour.holdDamage = m_data.damage;
				m_eatBehaviour.holdDuration = m_data.duration;

				m_holdTransform = null;
				m_machine.SetSignal(Signals.Type.Latching, true);
			}

			protected override void OnExit(State _newState) {
				if ( m_eatBehaviour.IsLatching() )
					m_eatBehaviour.EndHold();
				m_eatBehaviour.enabled = false;
				m_holdTransform = null;
				m_pilot.ReleaseAction(Pilot.Action.Latching);
				m_machine.SetSignal(Signals.Type.Latching, false);
			}

			void OnBiteKillEvent()
			{
				if ( !m_eatBehaviour.IsLatching() )
				{
					Transition(OnBiteFail);
				}
				else
				{
					m_originalParent = m_pilot.transform.parent;
					m_pilot.transform.parent = m_holdTransform;

					m_pilot.PressAction(Pilot.Action.Latching);
					m_holdTransform = m_eatBehaviour.holdTransform;
					m_pilot.GoTo( m_holdTransform.position );
					m_pilot.RotateTo( m_holdTransform.rotation );
				}
			}

			protected override void OnUpdate()
			{
				base.OnUpdate();
				if (m_holdTransform)
				{
					if (m_myMachine.IsDead() || m_myMachine.IsDying())	{
						if ( m_eatBehaviour.IsLatching() )
							m_eatBehaviour.EndHold();
						OnEndLatchingEvent();
					}else{	
						m_pilot.GoTo( m_holdTransform.position );
						m_pilot.RotateTo( m_holdTransform.rotation );
					}
				}
			}

			void OnEndLatchingEvent()
			{	
				m_pilot.transform.parent = m_originalParent;
				Transition(OnEndLatching, m_transitionParam);
			}
		}
	}
}
