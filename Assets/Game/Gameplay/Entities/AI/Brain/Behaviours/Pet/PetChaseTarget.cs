using UnityEngine;
using System.Collections;

namespace AI {
    namespace Behaviour {
        [System.Serializable]
        public class PetChaseTargetData : StateComponentData {
            // public float speedMultiplier = 1.5f;
            // public float chaseTimeout;
            // public Range m_cooldown;

            public string petChaseSku = "common";
            public bool checkCanBeBitten = true;
        }

        [CreateAssetMenu(menuName = "Behaviour/Pet/Chase Target")]
        public class PetChaseTarget : StateComponent {

            [StateTransitionTrigger]
            private static string OnCollisionDetected = "onCollisionDetected";

            [StateTransitionTrigger]
            private static string OnChaseTimeOut = "onChaseTimeout";

            [StateTransitionTrigger]
            private static string OnEnemyOutOfSight = "onEnemyOutOfSight";

            protected Transform m_target;
            protected AI.IMachine m_targetMachine;
            protected Entity m_targetEntity;
            protected MachineEatBehaviour m_eatBehaviour;
            protected float m_timer;
            protected float m_speed;

            private object[] m_transitionParam;

            protected float m_chaseTimeout;
            protected Range m_cooldown;
            protected bool m_checkCanBeBitten = true;

            public override StateComponentData CreateData() {
                return new PetChaseTargetData();
            }

            public override System.Type GetDataType() {
                return typeof(PetChaseTargetData);
            }

            protected override void OnInitialise() {
                PetChaseTargetData data = m_pilot.GetComponentData<PetChaseTargetData>();
                DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_MOVEMENT, data.petChaseSku);

                m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed * def.GetAsFloat("chaseSpeedMultiplier");
                m_chaseTimeout = def.GetAsFloat("chaseTimeout");
                m_cooldown = def.GetAsRange("chaseCooldown");
                m_checkCanBeBitten = data.checkCanBeBitten;
                m_eatBehaviour = m_pilot.GetComponent<MachineEatBehaviour>();

                m_machine.SetSignal(Signals.Type.Alert, true);
                m_target = null;

                m_transitionParam = new object[1];
            }

            protected override void OnEnter(State _oldState, object[] _param) {
                m_pilot.SetMoveSpeed(m_speed);
                m_pilot.SlowDown(false);

                m_target = null;
                m_targetMachine = null;
                m_targetEntity = null;

                if (_param != null && _param.Length > 0) {
                    m_target = _param[0] as Transform;
                    if (m_target) {
                        m_targetEntity = m_target.GetComponent<Entity>();
                        if (m_targetEntity)
                            m_targetMachine = m_targetEntity.machine;
                    }
                }

                if (m_target == null && m_machine.enemy != null) {
                    m_target = m_machine.enemy;
                    m_targetEntity = m_machine.enemy.GetComponent<Entity>();
                    m_targetMachine = m_targetEntity.machine;
                }

                if (m_targetMachine != null)
                    m_targetMachine.isPetTarget = true;

                m_timer = 0;
            }

            protected override void OnExit(State _newState) {
                if (m_targetMachine != null)
                    m_targetMachine.isPetTarget = false;
                m_pilot.SlowDown(true);
                m_target = null;
                m_targetMachine = null;
                m_targetEntity = null;
            }

            protected override void OnUpdate() {
                int errorCode = -1;
                try {
                    errorCode = 0;
                    // if eating move forward only
                    if (m_eatBehaviour != null && m_eatBehaviour.IsEating()) {
                        errorCode = 1;
                        m_pilot.SlowDown(true);
                        return;
                    }

                    errorCode = 2;
                    if (m_checkCanBeBitten && m_targetMachine != null && !m_targetMachine.CanBeBitten()) {
                        errorCode = 3;
                        m_target = null;
                        m_targetEntity = null;
                        m_targetMachine.isPetTarget = false;
                        m_targetMachine = null;
                    }

                    errorCode = 4;
                    // if collides with ground then -> recover/loose sight
                    if (m_machine.GetSignal(Signals.Type.Collision)) {
                        errorCode = 5;
                        object[] param = m_machine.GetSignalParams(Signals.Type.Collision);
                        errorCode = 6;
                        if (param != null && param.Length > 0) {
                            errorCode = 7;
                            Collision collision = param[0] as Collision;
                            if (collision != null) {
                                errorCode = 8;
                                if (collision.collider.gameObject.layer == LayerMask.NameToLayer("ground")) {
                                    errorCode = 9;
                                    // We go back
                                    m_transitionParam[0] = m_cooldown.GetRandom();
                                    errorCode = 10;
                                    Transition(OnCollisionDetected, m_transitionParam);
                                    return;
                                }
                            }
                        }
                    }

                    errorCode = 11;
                    if (m_target != null && m_target.gameObject.activeInHierarchy) {
                        errorCode = 12;
                        m_pilot.SlowDown(false);
                        // if not eating check chase timeout
                        errorCode = 13;
                        m_timer += Time.deltaTime;
                        if (m_timer >= m_chaseTimeout) {
                            errorCode = 14;
                            m_transitionParam[0] = m_cooldown.GetRandom();
                            errorCode = 15;
                            Transition(OnChaseTimeOut, m_transitionParam);
                        } else {
                            Vector3 pos;
                            // Chase
                            errorCode = 16;
                            if (m_targetEntity != null && m_targetEntity.circleArea != null) {
                                errorCode = 17;
                                pos = m_targetEntity.circleArea.center;
                            } else {
                                errorCode = 18;
                                pos = m_target.position;
                            }
                            errorCode = 19;
                            float magnitude = (pos - m_pilot.transform.position).sqrMagnitude;
                            if (magnitude < m_speed * 0.25f) // !!!
                                magnitude = m_speed * 0.25f;
                            errorCode = 20;
                            m_pilot.SetMoveSpeed(Mathf.Min(m_speed, magnitude));
                            errorCode = 21;
                            m_pilot.GoTo(pos);
                        }
                    } else {
                        errorCode = 22;
                        m_transitionParam[0] = m_cooldown.GetRandom();
                        errorCode = 23;
                        Transition(OnEnemyOutOfSight, m_transitionParam);
                    }
                } catch (System.Exception e) {
                    Fabric.Crashlytics.Crashlytics.RecordCustomException("PetChaseTarget.OnUpdate",  errorCode + " - " + " PetName: " + m_stateMachine.gameObject.name, e.ToString());
                    //throw new System.Exception("PetChaseTarget.OnUpdate: " + errorCode + "\n" + " PetName: " + m_stateMachine.gameObject.name + "\n" + e);
                }
            }
		}
	}
}