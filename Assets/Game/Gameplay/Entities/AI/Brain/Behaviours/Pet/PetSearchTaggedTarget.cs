using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
	namespace Behaviour {
        public enum TargetPriority {
            Any = 0,
            SmallestTier,
            BiggerTier
        };


		[System.Serializable]
        public class PetSearchTaggedTargetData : StateComponentData {
            public IEntity.Tag tag = 0;
			[Tooltip("Max tier this pet will consider target.")]
			public DragonTier maxValidTier = DragonTier.TIER_4;
			[Tooltip("Min tier this pet will consider target.")]
			public DragonTier minValidTier = DragonTier.TIER_0;
            public TargetPriority priority = TargetPriority.Any; 
			public CheckType checkType;
			public float dragonSizeRangeMultiplier = 10;
			public Range m_shutdownRange = new Range(10,20);
		}

        [CreateAssetMenu(menuName = "Behaviour/Pet/Search Tagged Target")]
		public class PetSearchTaggedTarget : StateComponent {

			[StateTransitionTrigger]
			private static string onEnemyTargeted = "onEnemyTargeted";

			private float m_shutdownSensorTime;
			private float m_timer;
			private object[] m_transitionParam;

			private Entity[] m_checkEntities = new Entity[50];
			private int m_numCheckEntities = 0;

			DragonPlayer m_owner;
			float m_range;
			MachineSensor m_sensor;

            private PetSearchTaggedTargetData m_data;
			EatBehaviour m_eatBehaviour;


			public override StateComponentData CreateData() {
                return new PetSearchTaggedTargetData();
			}

			public override System.Type GetDataType() {
                return typeof(PetSearchTaggedTargetData);
			}

			protected override void OnInitialise() {
				m_timer = 0f;
				m_shutdownSensorTime = 0f;

				m_transitionParam = new object[1];

				m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();

				base.OnInitialise();

				m_owner = InstanceManager.player;
                m_data = m_pilot.GetComponentData<PetSearchTaggedTargetData>();
				m_range = m_owner.data.maxScale * m_data.dragonSizeRangeMultiplier;

				m_sensor = (m_machine as Machine).sensor;
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

                    Entity target = null;

					m_numCheckEntities = EntityManager.instance.GetOverlapingEntities( centerPos , m_range, m_checkEntities);
					for (int e = 0; e < m_numCheckEntities; e++) 
					{
						Entity entity = m_checkEntities[e];
						Machine machine = entity.GetComponent<Machine>();
						if (machine != null && machine.CanBeBitten() && !machine.isPetTarget )
						{
                            if (entity.HasTag(m_data.tag)) {
                                bool isViable = false;
                                switch (m_data.checkType) {
                                    case CheckType.Edible: {
                                            if (entity.IsEdible(m_data.maxValidTier) && entity.edibleFromTier >= m_data.minValidTier) {
                                                if (m_eatBehaviour == null) {
                                                    isViable = true;
                                                } else {
                                                    EatBehaviour.SpecialEatAction specialAction = m_eatBehaviour.GetSpecialEatAction(entity.sku);
                                                    if (specialAction != EatBehaviour.SpecialEatAction.CannotEat) {
                                                        isViable = true;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    case CheckType.Burnable: {
                                            isViable = entity.burnableFromTier >= m_data.minValidTier && entity.burnableFromTier <= m_data.maxValidTier;
                                        }
                                        break;
                                }

                                if (isViable) {
                                    // Check if physics reachable
                                    RaycastHit hit;
                                    Vector3 dir = entity.circleArea.center - m_machine.position;
                                    bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, GameConstants.Layers.GROUND_PREYCOL_OBSTACLE);
                                    if (!hasHit) {
                                        if (m_data.priority == TargetPriority.Any) {
                                            target = entity;
                                            break;
                                        } else {
                                            if (target == null) {
                                                target = entity;
                                            } else if (m_data.priority == TargetPriority.SmallestTier) {
                                                if (entity.edibleFromTier < target.edibleFromTier) {
                                                    target = entity;
                                                }
                                            } else if (m_data.priority == TargetPriority.BiggerTier) {
                                                if (entity.edibleFromTier > target.edibleFromTier) {
                                                    target = entity;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
						}
					}

                    if (target != null) {
                        // Check if closed? Not for the moment
                        m_transitionParam[0] = target.transform;
                        if (target.circleArea)
                            m_sensor.SetupEnemy(target.transform, target.circleArea.radius * target.circleArea.radius, null);
                        else
                            m_sensor.SetupEnemy(target.transform, 0, null);
                        m_machine.SetSignal(Signals.Type.Warning, true);
                        Transition(onEnemyTargeted, m_transitionParam);
                    }
				}
			}
		}
	}
}