using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class OverrideEdibleValueData : StateComponentData {
			public DragonTier tier;
		}

		[CreateAssetMenu(menuName = "Behaviour/Override Edible Value")]
		public class OverrideEdibleValue : StateComponent {

			private OverrideEdibleValueData m_data;
			private DragonTier m_tierRestoreValue;
			private Entity m_entity;

			public override StateComponentData CreateData() {
				return new OverrideEdibleValueData();
			}

			public override System.Type GetDataType() {
				return typeof(OverrideEdibleValueData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<OverrideEdibleValueData>();

				m_entity = m_pilot.GetComponent<Entity>();
				m_tierRestoreValue = m_entity.edibleFromTier;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_entity.edibleFromTier = m_data.tier;
			}

			protected override void OnExit(State newState) {
				m_entity.edibleFromTier = m_tierRestoreValue;				
			}
		}
	}
}