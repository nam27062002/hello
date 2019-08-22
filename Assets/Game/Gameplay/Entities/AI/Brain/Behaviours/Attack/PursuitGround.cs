using UnityEngine;
using System.Collections;

namespace AI {
    namespace Behaviour {
        [System.Serializable]
        public class PursuitGroundData : StateComponentData {
            public float speed;
            public float arrivalRadius = 1f;
            public string attackPoint;
            public bool hasGuardState = false;
            public bool moveAwayOnCritical = true;
        }

        [CreateAssetMenu(menuName = "Behaviour/Attack/Pursuit Ground")]
        public class PursuitGround : StateComponent {

            [StateTransitionTrigger]
            private static readonly int onEnemyInRange = UnityEngine.Animator.StringToHash("onEnemyInRange");

            [StateTransitionTrigger]
            private static readonly int onEnemyInGuardArea = UnityEngine.Animator.StringToHash("onEnemyInGuardArea");

            [StateTransitionTrigger]
            private static readonly int onEnemyOutOfSight = UnityEngine.Animator.StringToHash("onEnemyOutOfSight");


            private enum PursuitState {
                Move_Towards = 0,
                Move_Away
            };


            protected PursuitGroundData m_data;
            protected Transform m_target;

            private PursuitState m_pursuitState;
            private object[] m_transitionParam;

            public override StateComponentData CreateData() {
                return new PursuitData();
            }

            public override System.Type GetDataType() {
                return typeof(PursuitGroundData);
            }

            protected override void OnInitialise() {
                m_data = m_pilot.GetComponentData<PursuitGroundData>();

                m_machine.SetSignal(Signals.Type.Alert, true);
                m_transitionParam = new object[1];
                m_target = null;
            }

            protected override void OnEnter(State oldState, object[] param) {
                m_pilot.Stop();
                m_pilot.SlowDown(true);

                m_target = null;

                if (m_machine.enemy != null) {
                    m_target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
                    if (m_target == null) {
                        m_target = m_machine.enemy;
                    }
                }

                m_pursuitState = PursuitState.Move_Towards;
            }

            protected override void OnUpdate() {
                if (!m_machine.GetSignal(Signals.Type.Warning)) {
                    m_target = null;
                }

                if (m_target != null) {
                    if (m_pursuitState == PursuitState.Move_Towards) {
                        if (m_data.moveAwayOnCritical && m_machine.GetSignal(Signals.Type.Critical)) {
                            ChangeState(PursuitState.Move_Away);
                        } else {
                            bool onGuardArea = false;

                            if (m_machine.GetSignal(Signals.Type.Danger)) {
                                if (m_target.position.x < m_machine.position.x) m_pilot.SetDirection(Vector3.left, true);
                                else m_pilot.SetDirection(Vector3.right, true);
                                m_transitionParam[0] = m_target;

                                m_pilot.Stop();
                                Transition(onEnemyInRange, m_transitionParam);
                            } else {
                                if (m_data.hasGuardState) {
                                    float m = Mathf.Abs(m_machine.position.x - m_target.position.x);
                                    onGuardArea = m <= 2f;
                                }

                                if (onGuardArea) {
                                    if (m_target.position.x < m_machine.position.x) m_pilot.SetDirection(Vector3.left, true);
                                    else m_pilot.SetDirection(Vector3.right, true);
                                    m_transitionParam[0] = m_target;

                                    m_pilot.Stop();
                                    Transition(onEnemyInGuardArea, m_transitionParam);
                                } else {
                                    m_pilot.SetMoveSpeed(m_data.speed, false);

                                    Vector3 direction = m_machine.groundDirection;
                                    direction.z = 0f;

                                    if (m_target.position.x < m_machine.position.x) {
                                        direction *= -1;
                                    }

                                    Vector3 target = m_machine.position + direction * m_data.speed;
                                    m_pilot.GoTo(target);
                                }
                            }
                        }
                    } else if (m_pursuitState == PursuitState.Move_Away) {
                        if (m_machine.GetSignal(Signals.Type.Critical)) {
                            m_pilot.SetMoveSpeed(m_data.speed, false);

                            // Player is inside our Critical area and we can't attack it from here, me should move back a bit
                            Vector3 direction = m_machine.groundDirection;
                            direction.z = 0f;

                            if (m_target.position.x > m_machine.position.x) {
                                direction *= -1;
                            }

                            Vector3 target = m_machine.position + direction * m_data.speed;
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
        }
    }
}