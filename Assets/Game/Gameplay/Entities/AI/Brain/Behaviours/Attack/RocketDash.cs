using UnityEngine;
using System.Collections;

namespace AI {
    namespace Behaviour {
        [System.Serializable]
        public class RocketDashData : StateComponentData {
            public float lightUpTime = 1.5f;
            public float speed = 0f;
            public float acceleration = 0f;
            public string attackPoint;
        }

        [CreateAssetMenu(menuName = "Behaviour/Attack/Rocket Dash")]
        public class RocketDash : StateComponent {

            [StateTransitionTrigger]
            private static readonly int onDashEnd = UnityEngine.Animator.StringToHash("onDashEnd");


            private enum RocketState {
                None = 0,
                LightUp,
                Dash
            }

            protected RocketDashData m_data;
            private RocketState m_rocketState;

            private Vector3 m_target;
            private float m_speed;
            private float m_elapsedTime;
            private float m_oldDistToTarget;

            private float m_lightUpTimer;


            public override StateComponentData CreateData() {
                return new RocketDashData();
            }

            public override System.Type GetDataType() {
                return typeof(RocketDashData);
            }

            protected override void OnInitialise() {
                m_data = m_pilot.GetComponentData<RocketDashData>();
                m_machine.SetSignal(Signals.Type.Alert, true);
            }

            protected override void OnEnter(State oldState, object[] param) {
                base.OnEnter(oldState, param);

                m_pilot.SetMoveSpeed(m_data.speed, false);
                m_pilot.SlowDown(false);
                m_machine.FaceDirection(true);

                m_rocketState = RocketState.None;
                ChangeState(RocketState.LightUp);
            }

            protected override void OnExit(State _newState) {
                m_pilot.ReleaseAction(Pilot.Action.Button_A);
                m_pilot.ReleaseAction(Pilot.Action.Button_B);
                m_pilot.ReleaseAction(Pilot.Action.ExclamationMark);
                m_machine.SetSignal(Signals.Type.Invulnerable, false);

                m_machine.FaceDirection(false);
            }

            protected override void OnUpdate() {

                switch (m_rocketState) {
                    case RocketState.LightUp:
                    if (m_machine.enemy != null) {
                        m_pilot.SetDirection((m_machine.enemy.position - m_machine.position).normalized, true);
                    }

                    m_lightUpTimer -= Time.deltaTime;
                    if (m_lightUpTimer <= 0) {
                        ChangeState(RocketState.Dash);
                    }
                    break;

                    case RocketState.Dash: {
                            m_pilot.SetMoveSpeed(m_speed, false);
                            m_speed = m_data.speed + m_data.acceleration * m_elapsedTime;
                            m_elapsedTime += Time.deltaTime;

                            float m = (m_machine.position - m_target).sqrMagnitude;

                            if (m > m_oldDistToTarget) {
                                Transition(onDashEnd);
                            } else {
                                m_pilot.GoTo(m_target);
                            }
                            m_oldDistToTarget = m;
                        }
                        break;
                }

                base.OnUpdate();
            }

            private void ChangeState(RocketState _newState) {
                if (m_rocketState != _newState) {
                    switch (m_rocketState) {
                        case RocketState.LightUp:
                        m_pilot.ReleaseAction(Pilot.Action.Button_A);
                        m_pilot.ReleaseAction(Pilot.Action.ExclamationMark);
                        break;

                        case RocketState.Dash:
                        m_pilot.ReleaseAction(Pilot.Action.Button_B);
                        break;
                    }

                    switch (_newState) {
                        case RocketState.LightUp:
                        m_pilot.Stop();
                        m_lightUpTimer = m_data.lightUpTime;
                        m_pilot.PressAction(Pilot.Action.Button_A);
                        m_pilot.PressAction(Pilot.Action.ExclamationMark);
                        break;

                        case RocketState.Dash: {
                                DragonPlayer dragon = InstanceManager.player;
                                Transform target = null;
                                if (m_machine.enemy != null) {
                                    target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
                                    if (target == null) {
                                        target = m_machine.enemy;
                                    }
                                }

                                if (target != null) {
                                    m_target = target.position + dragon.dragonMotion.velocity * 0.5f; //half a second
                                } else {
                                    m_target = m_machine.position + (m_pilot.direction * 5f);
                                }

                                //lets check if there is any collision in our way
                                RaycastHit groundHit;
                                if (Physics.Linecast(m_machine.position, m_target, out groundHit, GameConstants.Layers.GROUND_PREYCOL)) {
                                    m_target = groundHit.point;
                                }

                                m_speed = 0;
                                m_elapsedTime = 0;

                                m_oldDistToTarget = (m_machine.position - m_target).sqrMagnitude;

                                m_machine.SetSignal(Signals.Type.Invulnerable, true);
                                m_pilot.PressAction(Pilot.Action.Button_B);
                            }
                            break;
                    }

                    m_rocketState = _newState;
                }
            }
        }
    }
}