using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
    namespace Behaviour {

        [System.Serializable]
        public class PetSearchStunTargetData : StateComponentData {
            public float dragonSizeRangeMultiplier = 10;
            public Range m_shutdownRange = new Range(10, 20);
            [Tooltip("Coma separated list of entity skus to ignore")]
            public string m_ignoreSkus;
            public IEntity.Tag ignoreTag = 0;
        }

        [CreateAssetMenu(menuName = "Behaviour/Pet/Search Stun Target")]
        public class PetSearchStunTarget : StateComponent {

            [StateTransitionTrigger]
            private static readonly int onEnemyTargeted = UnityEngine.Animator.StringToHash("onEnemyTargeted");

            private float m_shutdownSensorTime;
            private float m_timer;
            private object[] m_transitionParam;

            private Entity[] m_checkEntities = new Entity[50];
            private int m_numCheckEntities = 0;

            private int m_collidersMask;

            DragonPlayer m_owner;
            float m_range;
            MachineSensor m_sensor;

            private PetSearchStunTargetData m_data;
            string[] m_ignoreSkus;
            int m_ignoreSkusCount;


            public override StateComponentData CreateData() {
                return new PetSearchStunTargetData();
            }

            public override System.Type GetDataType() {
                return typeof(PetSearchStunTargetData);
            }

            protected override void OnInitialise() {
                m_timer = 0f;
                m_shutdownSensorTime = 0f;

                m_transitionParam = new object[1];

                m_collidersMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("Obstacle");

                base.OnInitialise();

                m_owner = InstanceManager.player;
                m_data = m_pilot.GetComponentData<PetSearchStunTargetData>();
                m_range = m_owner.data.maxScale * m_data.dragonSizeRangeMultiplier;

                m_sensor = (m_machine as Machine).sensor;

                if (string.IsNullOrEmpty(m_data.m_ignoreSkus)) {
                    m_ignoreSkus = new string[] { };
                } else {
                    m_ignoreSkus = m_data.m_ignoreSkus.Split(new string[] { "," }, StringSplitOptions.None);
                }
                m_ignoreSkusCount = m_ignoreSkus.Length;
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

                    m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(centerPos, m_range, m_checkEntities);
                    for (int e = 0; e < m_numCheckEntities; e++) {
                        Entity entity = m_checkEntities[e];
                        if (!entity.HasTag(m_data.ignoreTag)) {
                            Machine machine = entity.machine as Machine;
                            if (machine != null && !machine.IsDying() && !machine.IsDead() && !machine.isPetTarget) {
                                // if ( entity.IsEdible( m_data.maxValidTier ) && entity.edibleFromTier >= m_data.minValidTier)
                                {
                                    // Test if in front of player!
                                    Vector3 entityDir = machine.position - m_owner.dragonMotion.position;
                                    if (Vector2.Dot(m_owner.dragonMotion.direction, entityDir) > 0) {
                                        bool ignore = false;
                                        for (int i = 0; i < m_ignoreSkusCount && !ignore; ++i) {
                                            if (entity.sku.Equals(m_ignoreSkus[i])) {
                                                ignore = true;
                                            }
                                        }
                                        if (ignore)
                                            continue;

                                        // Check if physics reachable
                                        RaycastHit hit;
                                        Vector3 dir = entity.circleArea.center - m_machine.position;
                                        bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, m_collidersMask);
                                        if (!hasHit) {
                                            // Check if closed? Not for the moment
                                            m_transitionParam[0] = entity.transform;
                                            if (entity.circleArea)
                                                m_sensor.SetupEnemy(entity.transform, entity.circleArea.radius * entity.circleArea.radius, null);
                                            else
                                                m_sensor.SetupEnemy(entity.transform, 0, null);
                                            m_machine.SetSignal(Signals.Type.Warning, true);
                                            Transition(onEnemyTargeted, m_transitionParam);
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
}