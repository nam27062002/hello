using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AI {
    namespace Behaviour {
        public enum CheckType {
            Edible,
            Burnable
        };

        [System.Serializable]
        public class PetSearchShootTargetData : StateComponentData {
            [Tooltip("Max tier this pet will consider target.")]
            public DragonTier maxValidTier = DragonTier.TIER_4;
            [Tooltip("Min tier this pet will consider target.")]
            public DragonTier minValidTier = DragonTier.TIER_0;
            public CheckType checkType;
            public Signals.Type ignoreSignal = Signals.Type.None;
            public float dragonSizeRangeMultiplier = 10;
            public Range m_shutdownRange = new Range(10, 20);
        }

        [CreateAssetMenu(menuName = "Behaviour/Pet/Search Shoot Target")]
        public class PetSearchShootTarget : StateComponent {

            [StateTransitionTrigger]
            private static readonly int onEnemyTargeted = UnityEngine.Animator.StringToHash("onEnemyTargeted");

            private float m_shutdownSensorTime;
            private float m_timer;
            private object[] m_transitionParam;

            private Entity[] m_checkEntities = new Entity[50];
            private int m_numCheckEntities = 0;

            DragonPlayer m_owner;
            float m_range;
            MachineSensor m_sensor;

            private PetSearchShootTargetData m_data;
            EatBehaviour m_eatBehaviour;


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

                m_eatBehaviour = m_pilot.GetComponent<EatBehaviour>();

                base.OnInitialise();

                m_owner = InstanceManager.player;
                m_data = m_pilot.GetComponentData<PetSearchShootTargetData>();
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


                    m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(centerPos, m_range, m_checkEntities);
                    for (int e = 0; e < m_numCheckEntities; e++) {
                        Entity entity = m_checkEntities[e];
                        Machine machine = entity.GetComponent<Machine>();
                        if (machine != null && !machine.IsDying() && !machine.IsDead() && machine.CanBeBitten() && !machine.isPetTarget) {
                            bool isViable = false;

                            switch (m_data.checkType) {
                                case CheckType.Edible: {
                                        if (entity.IsEdible(m_data.maxValidTier) && entity.edibleFromTier >= m_data.minValidTier) {
                                            EatBehaviour.SpecialEatAction specialAction = m_eatBehaviour.GetSpecialEatAction(entity.sku);
                                            if (specialAction != EatBehaviour.SpecialEatAction.CannotEat) {
                                                isViable = true;
                                            }
                                        }
                                    }
                                    break;
                                case CheckType.Burnable: {
                                        isViable = entity.burnableFromTier >= m_data.minValidTier && entity.burnableFromTier <= m_data.maxValidTier;
                                    }
                                    break;
                            }

                            if (isViable && m_data.ignoreSignal != Signals.Type.None) {
                                isViable = !machine.GetSignal(m_data.ignoreSignal);
                            }

                            if (isViable) {
                                // Test if in front of player!
                                Vector3 entityDir = machine.position - m_owner.dragonMotion.position;
                                if (Vector2.Dot(m_owner.dragonMotion.direction, entityDir) > 0) {
                                    // Check if physics reachable
                                    RaycastHit hit;
                                    Vector3 dir = entity.circleArea.center - m_machine.position;
                                    bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, GameConstants.Layers.GROUND_OBSTACLE);
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