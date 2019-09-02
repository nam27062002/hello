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
			private static readonly int onEnemySmallerTier = UnityEngine.Animator.StringToHash("onEnemySmallerTier");

            [StateTransitionTrigger]
			private static readonly int onEnemyEqualTier = UnityEngine.Animator.StringToHash("onEnemyEqualTier");

            [StateTransitionTrigger]
			private static readonly int onEnemyBiggerTier = UnityEngine.Animator.StringToHash("onEnemyBiggerTier");

            private int m_selectedTransition = 0;

			public override StateComponentData CreateData() {
				return new AttackSelectorData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackSelectorData);
			}

			protected override void OnInitialise() {
				AttackSelectorData data = m_pilot.GetComponentData<AttackSelectorData>();
				m_selectedTransition = onEnemyBiggerTier;
				DragonPlayer player = InstanceManager.player;
				// Get eat behaviour and set correctly
				if ( data.tier < player.data.tier )
				{
					m_selectedTransition = onEnemySmallerTier;
				}
				else if ( data.tier == player.data.tier)
				{
					m_selectedTransition = onEnemyEqualTier;
				}

			}

			protected override void OnUpdate() {
				Transition(m_selectedTransition);
			}


		}
	}
}