using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class LatchData : StateComponentData {
			public Range retreatTime = new Range( 4,6 );
			public float damage = 1;
			public float duration = 2;

		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Latch On Player")]
		public class LatchOnPlayer : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onBiteFail = UnityEngine.Animator.StringToHash("onLatchFail");
			[StateTransitionTrigger]
			private static readonly int onEndLatching = UnityEngine.Animator.StringToHash("onEndLatching");

			private EatBehaviour m_eatBehaviour;

			private LatchData m_data;

			private Transform m_parent;
			private float m_timer;

			private Transform m_holdTransform;
			private Transform m_originalParent;

			//--------------------------------------------------------
			public override StateComponentData CreateData() {
				return new LatchData();
			}

			public override System.Type GetDataType() {
				return typeof(LatchData);
			}

			protected override void OnInitialise() {
				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();
				m_data = m_pilot.GetComponentData<LatchData>();
				m_eatBehaviour.holdDamage = m_data.damage;
				m_eatBehaviour.holdDuration = m_data.duration;
				m_eatBehaviour.onEndLatching += OnEndLatchingEvent;
				base.OnInitialise();
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				// Get Target!
				m_holdTransform = m_eatBehaviour.holdTransform;

				m_originalParent = m_pilot.transform.parent;
				m_pilot.transform.parent = m_holdTransform;

				m_pilot.PressAction(Pilot.Action.Latching);
				m_pilot.GoTo( m_holdTransform.position );
				m_pilot.RotateTo( m_holdTransform.rotation );

				m_machine.SetSignal(Signals.Type.Latching, true);
				m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State _newState) {
				m_holdTransform = null;
				if ( m_eatBehaviour.IsLatching() )
					m_eatBehaviour.EndHold();
				if ( m_eatBehaviour.GetAttackTarget() != null )
					m_eatBehaviour.StopAttackTarget();

				m_pilot.transform.parent = m_originalParent;
				m_eatBehaviour.enabled = false;

				m_machine.SetSignal(Signals.Type.Latching, false);
				m_pilot.ReleaseAction(Pilot.Action.Latching);
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}

			protected override void OnUpdate()
			{
				base.OnUpdate();
				if (m_holdTransform)
				{
					if (m_machine.IsDead() || m_machine.IsDying())	{
						if ( m_eatBehaviour.IsLatching() )
							m_eatBehaviour.EndHold();
						// OnEndLatchingEvent();
					}else{	
						m_pilot.GoTo( m_holdTransform.position );
						m_pilot.RotateTo( m_holdTransform.rotation );
					}
				}
			}

			void OnEndLatchingEvent() {
				if ( m_holdTransform )	
				{
					m_eatBehaviour.enabled = false;
					m_holdTransform = null;
					m_machine.DisableSensor(m_data.retreatTime.GetRandom());
					Transition(onEndLatching);
				}
			}
		}
	}
}
