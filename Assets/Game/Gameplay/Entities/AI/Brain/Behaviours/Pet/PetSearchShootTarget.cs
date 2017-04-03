using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {	
		public enum CheckType
		{
			Edible,
			Burnable
		};

		[System.Serializable]
		public class PetSearchShootTargetData : StateComponentData {
			public DragonTier shootingTier;
			public CheckType checkType;
			public float dragonSizeRangeMultiplier = 10;
			public Range m_shutdownRange = new Range(10,20);
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Search Shoot Target")]
		public class PetSearchShootTarget : StateComponent {

			[StateTransitionTrigger]
			private static string onEnemyTargeted = "onEnemyTargeted";

			private float m_shutdownSensorTime;
			private float m_timer;
			private object[] m_transitionParam;

			private Entity[] m_checkEntities = new Entity[50];
			private int m_numCheckEntities = 0;

			private int m_collidersMask;

			DragonPlayer m_owner;
			float m_range;

			private PetSearchShootTargetData m_data;


			public override StateComponentData CreateData() {
				return new PetSearchShootTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetSearchShootTargetData);
			}

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;

				m_transitionParam = new object[1];

				m_collidersMask = 1<<LayerMask.NameToLayer("Ground") | 1<<LayerMask.NameToLayer("Obstacle");

				base.OnInitialise();

				m_owner = InstanceManager.player;
				m_data = m_pilot.GetComponentData<PetSearchShootTargetData>();
				m_range = m_owner.data.GetScaleAtLevel(m_owner.data.progression.maxLevel) * m_data.dragonSizeRangeMultiplier;
			}

			// The first element in _param must contain the amount of time without detecting an enemy
			protected override void OnEnter(State _oldState, object[] _param) {
				m_shutdownSensorTime = m_data.m_shutdownRange.GetRandom();
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
					Vector3 centerPos = m_owner.transform.position;


					m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( centerPos , m_range, m_checkEntities);
					for (int e = 0; e < m_numCheckEntities; e++) 
					{
						Entity entity = m_checkEntities[e];
						Machine machine = entity.GetComponent<Machine>();
						if (machine != null && machine.CanBeBitten() && !machine.isPetTarget )
						{
							bool isViable = false;
							switch( m_data.checkType )
							{
								case CheckType.Edible:
								{
									isViable = entity.IsEdible( m_data.shootingTier );
								}break;
								case CheckType.Burnable:
								{
									isViable = entity.IsBurnable( m_data.shootingTier );
								}break;
							}

							if ( isViable )
							{
								// Test if in front of player!
								Vector3 entityDir = machine.position - m_owner.dragonMotion.position;
								if( Vector2.Dot( m_owner.dragonMotion.direction, entityDir) > 0)
								{
									// Check if physics reachable
									RaycastHit hit;
									Vector3 dir = entity.circleArea.center - m_machine.position;
									bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
									if ( !hasHit )
									{
										// Check if closed? Not for the moment
										m_transitionParam[0] = entity.transform;
										m_machine.enemy = entity.transform;
										m_machine.SetSignal(Signals.Type.Warning, true);
										Transition( onEnemyTargeted, m_transitionParam);
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
}