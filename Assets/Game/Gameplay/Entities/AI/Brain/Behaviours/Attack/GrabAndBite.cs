using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class GrabAndBiteData : StateComponentData {
			public float time = 0f;
			public float damagePerSecond = 0f;
			public float retreatTime = 0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Grab and Bite")]
		public class GrabAndBite : StateComponent {

			[StateTransitionTrigger]
			private static string OnRelease = "onRelease";


			private GrabAndBiteData m_data;
			private object[] m_transitionParam;

			private DragonHealthBehaviour m_dragon;

			private Transform m_parent;
			private float m_timer;


			//--------------------------------------------------------
			public override StateComponentData CreateData() {
				return new GrabAndBiteData();
			}

			public override System.Type GetDataType() {
				return typeof(GrabAndBiteData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<GrabAndBiteData>();

				m_transitionParam = new object[1];
				m_transitionParam[0] = m_data.retreatTime; // retreat time
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_parent = m_machine.transform.parent;

				if (m_machine.enemy != null) {
					m_machine.transform.parent = m_machine.enemy.transform;
				}

				m_dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();

				m_timer = m_data.time;

				m_machine.SetSignal(Signals.Type.Biting, true);
			}

			protected override void OnExit(State _newState) {
				m_machine.transform.parent = m_parent;
				m_machine.position = m_machine.transform.position;

				m_machine.SetSignal(Signals.Type.Biting, false);
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0 || m_machine.enemy == null) {
					Transition(OnRelease, m_transitionParam);
				} else {
					m_dragon.ReceiveDamage(m_data.damagePerSecond * Time.deltaTime, DamageType.NORMAL, m_machine.transform);
				}
			}
		}
	}
}
