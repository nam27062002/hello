using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class AttackSelectorData : StateComponentData {
			public DragonTier tier;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Attack Selector")]
		public class AttackSelector : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemySmallerTier = "onEnemySmallerTier";

			[StateTransitionTrigger]
			private static string OnEnemyEqualTier = "onEnemyEqualTier";

			[StateTransitionTrigger]
			private static string OnEnemyBiggerTier = "onEnemyBiggerTier";

			private string m_selectedTransition = "";

			public override StateComponentData CreateData() {
				return new AttackSelectorData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackSelectorData);
			}

			protected override void OnInitialise() {
				AttackSelectorData data = m_pilot.GetComponentData<AttackSelectorData>();
				m_selectedTransition = OnEnemyBiggerTier;
				DragonPlayer player = InstanceManager.player;
				// Get eat behaviour and set correctly
				if ( data.tier < player.data.tier )
				{
					m_selectedTransition = OnEnemySmallerTier;
				}
				else if ( data.tier == player.data.tier)
				{
					m_selectedTransition = OnEnemyEqualTier;
				}

			}

			protected override void OnUpdate() {
				Transition(m_selectedTransition);
			}


		}
	}
}