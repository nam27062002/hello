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

			protected AttackSelectorData m_data;

			protected AI.IMachine m_targetMachine;
			protected Entity m_targetEntity;
			protected DragonPlayer m_player;
			protected float m_timer;
			protected float m_timeOut;

			private EatBehaviour m_eatBehaviour;
			private Transform m_mouth;

			private bool m_enemyInRange = false;



			private object[] m_transitionParam;

			public override StateComponentData CreateData() {
				return new AttackSelectorData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackSelectorData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<AttackSelectorData>();
				m_transitionParam = new object[1];
			}

			protected override void OnEnter(State oldState, object[] param) 
			{
				m_player = InstanceManager.player;
			}


			protected override void OnUpdate() {
				if ( m_data.tier < m_player.data.tier )
				{
					Transition(OnEnemySmallerTier);
				}
				else if ( m_data.tier == m_player.data.tier)
				{
					Transition( OnEnemyEqualTier );
				}
				else
				{
					Transition( OnEnemyBiggerTier );
				}
				
			}


		}
	}
}