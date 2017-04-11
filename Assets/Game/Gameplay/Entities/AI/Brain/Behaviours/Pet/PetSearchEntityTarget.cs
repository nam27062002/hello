using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {	

		[System.Serializable]
		public class PetSearchEntityTargetData : StateComponentData {
			[Comment("Comma Separated list", 5)]
			public string m_preferedEntitiesList;
			public string m_searchButNoEatEntityList;
			public bool m_ignoreNotListedUnits = false;
			public float m_dragonSizeRangeMultiplier = 10;
			public float m_preferedRangeMultiplier = 2;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Entity Target")]
		public class PetSearchEntityTarget : StateComponent {

			[StateTransitionTrigger]
			private static string OnEnemyInRange = "onEnemyInRange";

			private float m_shutdownSensorTime;
			private float m_timer;
			private DragonTier m_eaterTier;
			private object[] m_transitionParam;

			private Entity[] m_checkEntities = new Entity[40];
			private int m_numCheckEntities = 0;

			private int m_collidersMask;

			DragonPlayer m_owner;
			float m_range;

			EatBehaviour m_eatBehaviour;

			private PetSearchEntityTargetData m_data;
			private List<string> m_preferedEntities = new List<string>();
			private List<string> m_searchButNoEatList = new List<string>();


			public override StateComponentData CreateData() {
				return new PetSearchEntityTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetSearchEntityTargetData);
			}

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;

				// Temp
				MachineEatBehaviour machineEat = m_pilot.GetComponent<MachineEatBehaviour>();
				if ( machineEat )
					m_eaterTier = machineEat.eaterTier;

				m_transitionParam = new object[1];

				m_collidersMask = 1<<LayerMask.NameToLayer("Ground") | 1<<LayerMask.NameToLayer("Obstacle");

				base.OnInitialise();

				m_owner = InstanceManager.player;
				m_data = m_pilot.GetComponentData<PetSearchEntityTargetData>();
				m_range = m_owner.data.GetScaleAtLevel(m_owner.data.progression.maxLevel) * m_data.m_dragonSizeRangeMultiplier;

				if (!string.IsNullOrEmpty( m_data.m_preferedEntitiesList) )
				{
					// Use the separator string to split the string value
					string[] splitResult = m_data.m_preferedEntitiesList.Split(new string[] { "," }, StringSplitOptions.None);
					m_preferedEntities = new List<string>( splitResult );
				}

				if (!string.IsNullOrEmpty( m_data.m_searchButNoEatEntityList) )
				{
					// Use the separator string to split the string value
					string[] splitResult = m_data.m_searchButNoEatEntityList.Split(new string[] { "," }, StringSplitOptions.None);
					m_searchButNoEatList = new List<string>( splitResult );
				}

				// if prefered entieies we should tell the mouth
				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();

				// This will allow to eat them ignoring tier limit
				for( int i = 0; i<m_preferedEntities.Count; i++ )
				{
					m_eatBehaviour.AddToEatExceptionList( m_preferedEntities[i] );
				}

				// This will make eater to not eat it. If the same sku is in both lists it will not eat it!
				for( int i = 0; i<m_searchButNoEatList.Count; i++ )
				{
					m_eatBehaviour.AddToIgnoreList( m_searchButNoEatList[i] );
				}

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
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
				} else {
					m_eatBehaviour.enabled = true;
					Vector3 centerPos = m_owner.transform.position;


					// if prefered entieies check first
					if ( m_preferedEntities.Count > 0 || m_searchButNoEatList.Count > 0 )
					{
						m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( centerPos , m_range * m_data.m_preferedRangeMultiplier, m_checkEntities);	
						for (int e = 0; e < m_numCheckEntities; e++) 
						{
							Entity entity = m_checkEntities[e];
							bool inSearchButNotEat = m_searchButNoEatList.Contains( entity.sku );
							if ( inSearchButNotEat || m_preferedEntities.Contains(entity.sku) )
							{
								Machine machine = entity.GetComponent<Machine>();
								if ( machine != null && !machine.isPetTarget)
								{
									if ( inSearchButNotEat || machine.CanBeBitten() )
									{
										// Check if physics reachable
										RaycastHit hit;
										Vector3 dir = entity.circleArea.center - m_machine.position;
										bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
										if ( !hasHit )
										{
											// Check if closed? Not for the moment
											m_transitionParam[0] = entity.transform;
											Transition( OnEnemyInRange, m_transitionParam);
											return;
										}
									}
								}
							}
						}
					}

					if ( !m_data.m_ignoreNotListedUnits )
					{
						m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( centerPos , m_range, m_checkEntities);
						for (int e = 0; e < m_numCheckEntities; e++) 
						{
							Entity entity = m_checkEntities[e];
							IMachine machine = entity.machine;
							EatBehaviour.SpecialEatAction specialAction = m_eatBehaviour.GetSpecialEatAction( entity.sku );
							if (
								entity.IsEdible() && specialAction != EatBehaviour.SpecialEatAction.CannotEat && entity.IsEdible( m_eaterTier ) && machine != null && machine.CanBeBitten() && !machine.isPetTarget
							)
							{
								// Check if physics reachable
								RaycastHit hit;
								Vector3 dir = entity.circleArea.center - m_machine.position;
								bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
								if ( !hasHit )
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
	}
}