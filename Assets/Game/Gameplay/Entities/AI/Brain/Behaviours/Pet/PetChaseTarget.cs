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
            private static readonly int onCollisionDetected = UnityEngine.Animator.StringToHash("onCollisionDetected");

            [StateTransitionTrigger]
            private static readonly int onChaseTimeOut = UnityEngine.Animator.StringToHash("onChaseTimeout");

            [StateTransitionTrigger]
            private static readonly int onEnemyOutOfSight = UnityEngine.Animator.StringToHash("onEnemyOutOfSight");

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
                // if eating move forward only
                if (m_eatBehaviour != null && m_eatBehaviour.IsEating()) {
                    m_pilot.SlowDown(true);
                    return;
                }
            
                if (m_checkCanBeBitten && m_targetMachine != null && !m_targetMachine.CanBeBitten()) {
                    m_target = null;
                    m_targetEntity = null;
                    m_targetMachine.isPetTarget = false;
                    m_targetMachine = null;
                }

                // if collides with ground then -> recover/loose sight
                if (m_machine.GetSignal(Signals.Type.Collision)) {
                    object[] param = m_machine.GetSignalParams(Signals.Type.Collision);
                    if (param != null && param.Length > 0) {
                        Collision collision = param[0] as Collision;
                        if (collision != null && collision.collider != null && collision.collider.gameObject != null) {
                            if (collision.collider.gameObject.layer == LayerMask.NameToLayer("ground")) {
                                // We go back
                                m_transitionParam[0] = m_cooldown.GetRandom();
                                Transition(onCollisionDetected, m_transitionParam);
                                return;
                            }
                        }
                    }
                }

                if (m_target != null && m_target.gameObject.activeInHierarchy) {
                    m_pilot.SlowDown(false);
                    // if not eating check chase timeout
                    m_timer += Time.deltaTime;
                    if (m_timer >= m_chaseTimeout) {
                        m_transitionParam[0] = m_cooldown.GetRandom();
                        Transition(onChaseTimeOut, m_transitionParam);
                    } else {
                        Vector3 pos;
                        // Chase
                        if (m_targetEntity != null && m_targetEntity.circleArea != null) {
                            pos = m_targetEntity.circleArea.center;
                        } else {
                            pos = m_target.position;
                        }

                        float magnitude = (pos - m_pilot.transform.position).sqrMagnitude;
                        if (magnitude < m_speed * 0.25f) // !!!
                            magnitude = m_speed * 0.25f;

                        m_pilot.SetMoveSpeed(Mathf.Min(m_speed, magnitude));
                        m_pilot.GoTo(pos);
                    }
                } else {
                    m_transitionParam[0] = m_cooldown.GetRandom();
                    Transition(onEnemyOutOfSight, m_transitionParam);
                }
            }
		}
	}
}