using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class OnGroundData : StateComponentData {
			public float standUpTime;
		}

		[CreateAssetMenu(menuName = "Behaviour/On Ground")]
		public class OnGround : StateComponent {
			
			[StateTransitionTrigger]
			private static readonly int onRecover = UnityEngine.Animator.StringToHash("onRecover");


            private OnGroundData m_data;
			private PreyAnimationEvents m_animEvents;

			private float m_timer;
			private float m_timeOut;


			public override StateComponentData CreateData() {
				return new OnGroundData();
			}

			public override System.Type GetDataType() {
				return typeof(OnGroundData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<OnGroundData>();
				m_animEvents = m_pilot.FindComponentRecursive<PreyAnimationEvents>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {				
				m_timer = m_data.standUpTime;
				m_machine.DisableSensor(m_timer);
				m_pilot.Stop();

				if (m_animEvents != null) {
					m_animEvents.onStandUp += new PreyAnimationEvents.OnStandUpDelegate(OnStandUp);
				}

				m_timeOut = 3f;
			}

			protected override void OnExit(State _newState) {
				if (m_animEvents != null) {
					m_animEvents.onStandUp -= new PreyAnimationEvents.OnStandUpDelegate(OnStandUp);
				}
			}

			protected override void OnUpdate() {
				//
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					if (m_machine.GetSignal(Signals.Type.InWater)) {
						m_machine.Drown();
					}
				}

				m_timeOut -= Time.deltaTime;
				if (m_timeOut <= 0f) {
					OnStandUp();
				}
			}

			private void OnStandUp() {
				Transition(onRecover);
			}
		}
	}
}