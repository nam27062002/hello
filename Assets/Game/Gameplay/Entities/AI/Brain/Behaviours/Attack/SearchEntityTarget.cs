using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	
		[System.Serializable]
		public class SearchEntityData : StateComponentData {
			public float range = 5f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Search Entity Target")]
		public class SearchEntityTarget : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			public override StateComponentData CreateData() {
				return new SearchEntityData();
			}

			public override System.Type GetDataType() {
				return typeof(SearchEntityData);
			}

			protected SearchEntityData m_data;
			private float m_shutdownSensorTime;
			private float m_timer;
			private DragonTier m_eaterTier;
			private object[] m_transitionParam;

			private Entity[] m_checkEntities = new Entity[20];
			private int m_numCheckEntities = 0;

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<SearchEntityData>();
				m_timer = 0f;
				m_shutdownSensorTime = 0f;
				m_eaterTier = m_pilot.GetComponent<MachineEatBehaviour>().eaterTier;	// Temp
				m_transitionParam = new object[1];
				base.OnInitialise();
			}

			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {
				if (_param != null && _param.Length > 0) {
					m_shutdownSensorTime = (float)_param[0];
				} else {
					m_shutdownSensorTime = 0f;
				}

				if (m_shutdownSensorTime > 0f) {
					m_timer = m_shutdownSensorTime;
				} else {
					m_timer = 0f;
				}

				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
				} else {

					m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( m_machine.position , m_data.range, m_checkEntities);
					for (int e = 0; e < m_numCheckEntities; e++) 
					{
						Entity entity = m_checkEntities[e];
						if (entity.IsEdible() && entity.IsEdible( m_eaterTier ))
						{
							// Check if closed? Not for the moment
							m_transitionParam[0] = entity.transform;
							Transition( OnEnemyInRange, m_transitionParam);
							break;
						}
					}

				}
			}
		}
	}
}