using UnityEngine;
using System.Collections;

namespace AI {
    namespace Behaviour {
        [System.Serializable]
        public class CurveDashData : StateComponentData {
            public float speed = 0f;
            public string attackPoint;
            public bool hasWeapon = false;
        }

        [CreateAssetMenu(menuName = "Behaviour/Attack/Curve Dash")]
        public class CurveDash : StateComponent {

            [StateTransitionTrigger]
            private static int onDashEnd = UnityEngine.Animator.StringToHash("onDashEnd");


            protected CurveDashData m_data;

            // Bezier stuff
            private Vector3 m_pointA;
            private Vector3 m_pointB;
            private Vector3 m_pointControl;

            private Vector3 m_target;
            private Transform m_targetTransform;
            private bool m_dashIntoDragon;

            private float m_timeSpeed;

            private float m_timer;


            public override StateComponentData CreateData() {
                return new CurveDashData();
            }

            public override System.Type GetDataType() {
                return typeof(CurveDashData);
            }


            protected override void OnInitialise() {
                m_data = m_pilot.GetComponentData<CurveDashData>();
                m_machine.SetSignal(Signals.Type.Alert, true);
            }

            protected override void OnEnter(State oldState, object[] param) {
                base.OnEnter(oldState, param);

                //	m_machine.SetSignal(Signals.Type.Invulnerable, true);

                m_pilot.SetMoveSpeed(m_data.speed, false);

                m_dashIntoDragon = true;
                m_targetTransform = null;

                CreateCurve();
                UpdateTarget(0f);
            }

            protected override void OnExit(State _newState) {
                //m_machine.SetSignal(Signals.Type.Invulnerable, false);
            }

            protected override void OnUpdate() {
                if (m_timer < 1f) {
                    m_timer += Time.deltaTime / m_timeSpeed;
                    if (m_timer > 1f) {
                        m_timer = 1f;
                        m_pilot.SlowDown(true);
                    }
                }

                UpdateTarget(m_timer);
                m_pilot.GoTo(m_target);

                if (System.Math.Abs(m_timer - 1f) < Mathf.Epsilon) {
                    float m = (m_machine.position - m_target).sqrMagnitude;
                    float d = m_pilot.speed * Time.deltaTime;
                    if (m < d * d) {
                        m_pilot.Stop();

                        if (m_machine.GetSignal(Signals.Type.Danger)) {
                            CreateCurve();
                        } else if (m_machine.GetSignal(Signals.Type.Warning)) {
                            Transition(onDashEnd);
                        }
                    }
                }
            }

            private void CreateCurve() {
                m_pointA = m_machine.transform.position;

                // lets find our target
                DragonPlayer dragon = InstanceManager.player;
                if (m_dashIntoDragon) {
                    m_targetTransform = null;

                    if (m_machine.enemy != null) {
                        m_targetTransform = dragon.holdPreyPoints.GetRandomValue().transform;
                    }

                    if (m_targetTransform != null) {
                        m_pointB = m_targetTransform.position + dragon.dragonMotion.velocity * 1f; //half a second
                    } else {
                        m_pointB = m_machine.position + (m_pilot.direction * 5f);
                    }

                    //lets check if there is any collision in our way
                    RaycastHit groundHit;
                    if (Physics.Linecast(m_machine.position, m_pointB, out groundHit, GameConstants.Layers.GROUND_PREYCOL)) {
                        m_pointB = groundHit.point + groundHit.normal * 2f;
                    }
                } else {
                    // we'll move away to perform another action
                    m_pointB = dragon.dragonMotion.position - dragon.dragonMotion.direction * Random.Range(3f, 6f);
                }

                // and now the control point to create our curve
                float distance = (m_pointB - m_pointA).magnitude * Random.Range(0.25f, 0.75f);
                Vector3 dir = (m_pointB - m_pointA).normalized;
                m_pointControl = m_pointA + dir * distance;

                Vector3 perpendicular = Vector3.Cross(dir, Vector3.back);
                m_pointControl += perpendicular * distance * Random.Range(1f, 1.5f);

                // approximate length
                float length = 0;
                Vector3 last = m_pointA;
                for (float t = 0.1f; t < 1f; t += 0.1f) {
                    UpdateTarget(t);
                    length += (last - m_target).magnitude;

                    Debug.DrawLine(last, m_target, Colors.pink, 0.5f);

                    last = m_target;
                }
                Debug.DrawLine(last, m_pointB, Colors.pink, 0.5f);
                length += (last - m_pointB).magnitude;

                m_timeSpeed = (length / m_pilot.speed);

                m_timer = 0f;
                m_pilot.SlowDown(false);
                m_pilot.SetMoveSpeed(m_data.speed, false);

                // next curve will move us away from dragon before performing another dash
                m_dashIntoDragon = !m_dashIntoDragon;
            }

            private void UpdateTarget(float _t) {
                float oneMinusT = (1 - _t);
                m_target = (oneMinusT * oneMinusT * m_pointA) + (2 * oneMinusT * _t * m_pointControl) + (_t * _t * m_pointB);
            }
        }
    }
}