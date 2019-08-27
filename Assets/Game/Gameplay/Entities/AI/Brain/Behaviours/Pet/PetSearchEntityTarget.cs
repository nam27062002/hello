using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {	

		[System.Serializable]
		public class PetSearchEntityTargetData : StateComponentData {
			/*
			[Comment("Comma Separated list", 5)]
			public string m_preferedEntitiesList;
			public string m_searchButNoEatEntityList;
			public bool m_ignoreNotListedUnits = false;
			public float m_dragonSizeRangeMultiplier = 10;
			public float m_preferedRangeMultiplier = 2;
			*/
			public string m_petSeachEntitySku = "common";
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Entity Target")]
		public class PetSearchEntityTarget : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");

            [StateTransitionTrigger]
			private static readonly int onEnemyInRangeNoEat = UnityEngine.Animator.StringToHash("onEnemyInRangeNoEat");

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
		
			private List<string> m_preferedEntities = new List<string>();
			private List<string> m_searchButNoEatList = new List<string>();
			private bool m_ignoreNotListedUnits = false;
			private float m_preferedRangeMultiplier = 1;

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
				PetSearchEntityTargetData data = m_pilot.GetComponentData<PetSearchEntityTargetData>();
				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_MOVEMENT, data.m_petSeachEntitySku);

				m_range = m_owner.data.maxScale * def.GetAsFloat("searchDistanceMultiplier");
				m_preferedRangeMultiplier = def.GetAsFloat("preferedRangeMultiplier");
				m_preferedEntities = def.GetAsList<string>("preferedEntitiesList");
				m_searchButNoEatList = def.GetAsList<string>("searchButNoEatEntityList");
				m_ignoreNotListedUnits = def.GetAsBool("ignoreNotListedUnits");

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

			private bool IsReachable( Entity entity )
			{
				// Check if physics reachable
				RaycastHit hit;
				Vector3 dir = entity.circleArea.center - m_machine.position;
				bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
				return !hasHit ;
			}

			protected override void OnUpdate() {
				if (m_timer > 0f) {
					m_timer -= Time.deltaTime;
				} else {
					m_eatBehaviour.enabled = true;
					Vector3 centerPos = m_owner.transform.position;

					bool done = false;
					// if prefered entieies check first
					if ( m_preferedEntities.Count > 0 || m_searchButNoEatList.Count > 0 )
					{
						m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( centerPos , m_range * m_preferedRangeMultiplier, m_checkEntities);	
						for (int e = 0; e < m_numCheckEntities; e++) 
						{
							Entity entity = m_checkEntities[e];
							bool inSearchButNotEat = m_searchButNoEatList.Contains( entity.sku );
							if ( inSearchButNotEat || m_preferedEntities.Contains(entity.sku) )
							{
								Machine machine = entity.GetComponent<Machine>();
								if ( machine != null && !machine.IsDying() && !machine.IsDead() && !machine.isPetTarget)
								{
									if ( inSearchButNotEat )
									{
										if (IsReachable( entity ))
										{
											m_transitionParam[0] = entity.transform;
											Transition( onEnemyInRangeNoEat, m_transitionParam);
											done = true;
										}
									}
									else if ( machine.CanBeBitten() )
									{
										if (IsReachable( entity ))
										{
											m_transitionParam[0] = entity.transform;
											Transition( onEnemyInRange, m_transitionParam);
											done = true;
										}
									}
								}
							}
						}
					}

					if ( !m_ignoreNotListedUnits && !done)
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
								if (IsReachable( entity ))
								{
									// Check if closed? Not for the moment
									m_transitionParam[0] = entity.transform;
									Transition( onEnemyInRange, m_transitionParam);
								}
							}
						}
					}

				}
			}
		}
	}
}