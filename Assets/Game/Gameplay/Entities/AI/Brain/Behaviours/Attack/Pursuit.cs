using UnityEngine;
using System.Collections;

namespace AI {
    namespace Behaviour {
        [System.Serializable]
        public class PursuitData : StateComponentData {
            public float speed;
            public float arrivalRadius = 1f;
            public string attackPoint;
            public bool hasGuardState = false;
            public Range timeout = new Range(0, 0);
        }

        [CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit")]
        public class Pursuit : StateComponent {

            [StateTransitionTrigger]
            private static readonly int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");

            [StateTransitionTrigger]
            private static readonly int onEnemyInGuardArea = UnityEngine.Animator.StringToHash("onEnemyInGuardArea");

            [StateTransitionTrigger]
            private static readonly int onEnemyOutOfSight = UnityEngine.Animator.StringToHash("onEnemyOutOfSight");

            [StateTransitionTrigger]
            private static readonly int onPursuitTimeOut = UnityEngine.Animator.StringToHash("onPursuitTimeOut");


            private enum PursuitState {
                Move_Towards = 0,
                Move_Away
            };


            protected PursuitData m_data;
            protected Transform m_target;
            protected AI.IMachine m_targetMachine;
            protected Entity m_targetEntity;
            private Transform m_mouth;

            private PursuitState m_pursuitState;
            private object[] m_transitionParam;

            protected float m_timer;
            protected float m_timeOut;

            public override StateComponentData CreateData() {
                return new PursuitData();
            }

            public override System.Type GetDataType() {
                return typeof(PursuitData);
            }

            protected override void OnInitialise() {
                m_data = m_pilot.GetComponentData<PursuitData>();

                m_mouth = m_machine.transform.FindTransformRecursive("Fire_Dummy");

                m_machine.SetSignal(Signals.Type.Alert, true);
                m_transitionParam = new object[1];
                m_target = null;
            }

            protected override void OnEnter(State oldState, object[] param) {
                m_pilot.SetMoveSpeed(m_data.speed, false);
                m_pilot.SlowDown(true);

                m_target = null;
                m_targetMachine = null;
                m_targetEntity = null;

                if (param != null && param.Length > 0) {
                    m_target = param[0] as Transform;
                    if (m_target) {
                        m_targetEntity = m_target.GetComponent<Entity>();
                        if (m_targetEntity != null) {
                            m_targetMachine = m_targetEntity.machine;
                        }
                    }
                }

                if (m_target == null && m_machine.enemy != null) {
                    if (m_targetMachine != null) {
                        m_target = SearchClosestHoldPoint(m_targetMachine.holdPreyPoints);
                    } else {
                        m_target = SearchClosestHoldPoint(InstanceManager.player.holdPreyPoints);
                    }

                    m_targetEntity = m_machine.enemy.GetComponent<Entity>();
                    m_targetMachine = m_machine.enemy.GetComponent<IMachine>();
                }

                m_pursuitState = PursuitState.Move_Towards;

                m_timer = 0;
                if (m_data.timeout.max > 0) {
                    m_timeOut = m_data.timeout.GetRandom();
                } else {
                    m_timeOut = -1;
                }

            }

            protected override void OnUpdate() {
                if (m_targetMachine != null) {
                    if (m_targetMachine.IsDead() || m_targetMachine.IsDying()) {
                        m_target = null;
                        m_targetMachine = null;
                        m_targetEntity = null;
                    }
                } else {
                    if (!m_machine.GetSignal(Signals.Type.Warning)) {
                        m_target = null;
                    }
                }

                if (m_target != null && m_target.gameObject.activeInHierarchy) {

                    m_timer += Time.deltaTime;
                    if (m_timeOut >= 0 && m_timer > m_timeOut) {
                        Transition(onPursuitTimeOut);
                    } else if (m_pursuitState == PursuitState.Move_Towards) {
                        if (m_machine.GetSignal(Signals.Type.Critical)) {
                            ChangeState(PursuitState.Move_Away);
                        } else {
                            bool onRange = false;
                            bool onGuardArea = false;

                            if (m_targetEntity != null) {
                                float m = (m_machine.eye - m_targetEntity.circleArea.center).sqrMagnitude;
                                onRange = m < m_data.arrivalRadius * m_data.arrivalRadius;
                            } else {
                                onRange = m_machine.GetSignal(Signals.Type.Danger);
                            }

                            if (onRange) {
                                m_transitionParam[0] = m_target;
                                Transition(onEnemyInRange, m_transitionParam);
                            } else {
                                if (m_data.hasGuardState) {
                                    float m = Mathf.Abs(m_machine.eye.x - m_target.position.x);
                                    onGuardArea = m <= 2f;
                                }

                                if (onGuardArea) {
                                    m_transitionParam[0] = m_target;
                                    Transition(onEnemyInGuardArea, m_transitionParam);
                                }

                                if (m_targetEntity != null) {
                                    m_pilot.GoTo(m_targetEntity.circleArea.center);
                                } else {
                                    m_pilot.GoTo(m_target.position);
                                }
                            }
                        }
                    } else if (m_pursuitState == PursuitState.Move_Away) {
                        if (m_machine.GetSignal(Signals.Type.Critical)) {
                            // Player is inside our Critical area and we can't attack it from here, me should move back a bit
                            Vector3 direction = Vector3.left;
                            if (m_target.position.x < m_machine.eye.x) {
                                direction = Vector3.right;
                            }
                            Vector3 target = m_machine.eye + direction * m_data.speed;
                            m_pilot.GoTo(target);
                        } else {
                            ChangeState(PursuitState.Move_Towards);
                        }
                    }
                } else {
                    Transition(onEnemyOutOfSight);
                }
            }

            private void ChangeState(PursuitState _newState) {
                if (_newState != m_pursuitState) {
                    m_pursuitState = _newState;
                }
            }

            protected Transform SearchClosestHoldPoint(HoldPreyPoint[] holdPreyPoints) {
                float distance = float.MaxValue;
                Transform holdTransform = null;
                for (int i = 0; i < holdPreyPoints.Length; i++) {
                    HoldPreyPoint point = holdPreyPoints[i];
                    if (!point.holded && Vector3.SqrMagnitude(m_mouth.position - point.transform.position) < distance) {
                        distance = Vector3.SqrMagnitude(m_mouth.position - point.transform.position);
                        holdTransform = point.transform;
                    }
                }
                return holdTransform;
            }
        }
    }
}