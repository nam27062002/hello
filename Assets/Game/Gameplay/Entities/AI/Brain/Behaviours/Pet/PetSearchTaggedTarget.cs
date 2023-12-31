﻿using UnityEngine;
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
            public IEntity.Tag ignoreTag = 0;
            [Tooltip("Max tier this pet will consider target.")]
            public DragonTier maxValidTier = DragonTier.TIER_4;
            public TargetPriority priority = TargetPriority.Any;
            public CheckType checkType;
            public bool checkTypeWithPlayerTier = false;
            public Signals.Type ignoreSignal = Signals.Type.None;
            public float dragonSizeRangeMultiplier = 10;
            public Range m_shutdownRange = new Range(10, 20);
        }

        [CreateAssetMenu(menuName = "Behaviour/Pet/Search Tagged Target")]
        public class PetSearchTaggedTarget : StateComponent {

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

            private PetSearchTaggedTargetData m_data;
            EatBehaviour m_eatBehaviour;
            DragonTier m_playerTier;


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

                m_playerTier = InstanceManager.player.data.tier;
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

                    m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(centerPos, m_range, m_checkEntities);
                    for (int e = 0; e < m_numCheckEntities; e++) {
                        Entity entity = m_checkEntities[e];
                        Machine machine = entity.GetComponent<Machine>();
                        if (machine != null && !machine.IsDying() && !machine.IsDead() && machine.CanBeBitten() && !machine.isPetTarget) {
                            if (entity.HasTag(m_data.tag) && !entity.HasTag(m_data.ignoreTag)) {
                                bool isViable = false;
                                switch (m_data.checkType) {
                                    case CheckType.Edible: {
                                            if ( m_data.checkTypeWithPlayerTier )
                                            {
                                                isViable = entity.IsEdible( m_playerTier ) || entity.CanBeGrabbed( m_playerTier );
                                            } else {
                                                isViable = entity.IsEdible(m_data.maxValidTier);
                                            }

                                            if ( isViable && m_eatBehaviour != null )
                                            {
                                                EatBehaviour.SpecialEatAction specialAction = m_eatBehaviour.GetSpecialEatAction(entity.sku);
                                                isViable = specialAction != EatBehaviour.SpecialEatAction.CannotEat;
                                            }
                                        }
                                        break;
                                    case CheckType.Burnable: {
                                            if ( m_data.checkTypeWithPlayerTier )
                                            {
                                                isViable = entity.burnableFromTier <= m_playerTier;
                                            }
                                            else
                                            {
                                                isViable = entity.burnableFromTier <= m_data.maxValidTier;
                                            }
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
                                        bool hasHit = Physics.Raycast(m_machine.position, dir.normalized, out hit, dir.magnitude, GameConstants.Layers.GROUND_PLAYER_OBSTACLE);
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