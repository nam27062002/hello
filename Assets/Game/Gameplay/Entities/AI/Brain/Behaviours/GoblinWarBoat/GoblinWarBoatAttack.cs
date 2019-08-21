using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    namespace Behaviour {
        [System.Serializable]
        public class GoblinWarBoatAttackData : StateComponentData {
            public float damage = 25f;
            public float cannonRotationSpeed = 120f;

            public int consecutiveAttacks = 3;
            public float attackDelay = 0f;
            public float retreatTime = 0f;

            public string projectileName = "";
            public string projectileSpawnTransformName = "";
        }

        [CreateAssetMenu(menuName = "Behaviour/GoblinWarBoat/Attack")]
        public class GoblinWarBoatAttack : StateComponent, IBroadcastListener {
            private enum AttackState {
                Aim = 0,
                Shoot
            }

            [StateTransitionTrigger]
            private static readonly int onMaxAttacks = UnityEngine.Animator.StringToHash("onMaxAttacks");

            [StateTransitionTrigger]
            private static readonly int onOutOfRange = UnityEngine.Animator.StringToHash("onOutOfRange");


            private GoblinWarBoatAttackData m_data;

            private AttackState m_attackState;

            private bool m_onAttachProjectileEventDone;
            private bool m_onDamageEventDone;
            private bool m_onAttackEndEventDone;

            private int m_attacksLeft;
            private float m_timer;

            private Transform m_targetDummy;
            private Transform m_cannonEye;

            private GameObject m_projectile;
            private Transform m_projectileSpawnPoint;

            private PoolHandler m_poolHandler;
            protected PreyAnimationEvents m_animEvents;


            public override StateComponentData CreateData() {
                return new GoblinWarBoatAttackData();
            }

            public override System.Type GetDataType() {
                return typeof(GoblinWarBoatAttackData);
            }

            protected override void OnInitialise() {
                m_data = m_pilot.GetComponentData<GoblinWarBoatAttackData>();

                m_animEvents = m_pilot.FindComponentRecursive<PreyAnimationEvents>();
                m_machine.SetSignal(Signals.Type.Alert, true);

                m_projectileSpawnPoint = m_pilot.FindTransformRecursive(m_data.projectileSpawnTransformName);
                CreatePool();

                // create a projectile from resources (by name) and save it into pool
                Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);

                m_targetDummy = (m_machine as MachineGoblinWarBoat).targetDummy;
                m_cannonEye = (m_machine as MachineGoblinWarBoat).cannonEye;

                m_attacksLeft = m_data.consecutiveAttacks;
            }

            protected override void OnRemove() {
                base.OnRemove();
                Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
            }

            public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
                switch (eventType) {
                    case BroadcastEventType.GAME_AREA_ENTER: {
                            CreatePool();
                        }
                        break;
                }

            }


            void CreatePool() {
                m_poolHandler = PoolManager.CreatePool(m_data.projectileName, 2, true);
            }

            protected override void OnEnter(State _oldState, object[] _param) {
                m_machine.SetSignal(Signals.Type.Ranged, true);
                m_pilot.Stop();

                m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
                m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
                m_animEvents.onAttackEnd += new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
                m_animEvents.onInterrupt += new PreyAnimationEvents.OnInterruptDelegate(OnAnimEnd);

                m_onAttachProjectileEventDone = true;
                m_onDamageEventDone = true;
                m_onAttackEndEventDone = true;

                Vector3 dummyDir = m_targetDummy.position - m_projectileSpawnPoint.position;
                dummyDir.Normalize();

                if (m_attacksLeft <= 0)
                    m_attacksLeft = m_data.consecutiveAttacks;
                m_timer = m_data.attackDelay;

                m_attackState = AttackState.Aim;
            }

            protected override void OnExit(State _newState) {
                m_pilot.ReleaseAction(Pilot.Action.Attack);
                m_machine.SetSignal(Signals.Type.Ranged, false);

                if (m_projectile != null) {
                    m_projectile.SetActive(false);
                    m_poolHandler.ReturnInstance(m_projectile);
                    m_projectile = null;
                }

                m_pilot.SetDirection(m_machine.direction, false);

                m_animEvents.onAttachProjectile -= new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
                m_animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
                m_animEvents.onAttackEnd -= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
                m_animEvents.onInterrupt -= new PreyAnimationEvents.OnInterruptDelegate(OnAnimEnd);

                m_pilot.ReleaseAction(Pilot.Action.Stop);
            }

            protected override void OnUpdate() {
                if (m_machine.enemy == null) {
                    Transition(onOutOfRange);
                    return;
                }

                switch (m_attackState) {
                    case AttackState.Aim:
                    m_timer -= Time.deltaTime;
                    if (m_timer > 0) {
                        Vector3 enemyDir = m_machine.enemy.position - m_projectileSpawnPoint.position;
                        enemyDir.z = 0;
                        enemyDir.Normalize();

                        Quaternion targetRotation = Quaternion.LookRotation(enemyDir, Vector3.forward);
                        m_cannonEye.rotation = Quaternion.RotateTowards(m_cannonEye.rotation, targetRotation, m_data.cannonRotationSpeed * Time.smoothDeltaTime);

                        m_targetDummy.position = m_projectileSpawnPoint.position + (m_cannonEye.forward * 5f);
                    } else {
                        StartAttack();
                    }
                    break;

                    case AttackState.Shoot:
                    break;
                }
            }

            private void StartAttack() {
                m_pilot.PressAction(Pilot.Action.Attack);

                m_onAttachProjectileEventDone = false;
                m_onDamageEventDone = false;
                m_onAttackEndEventDone = false;
                m_attacksLeft--;

                m_timer = m_data.attackDelay;

                m_attackState = AttackState.Shoot;
            }

            private void OnAttachProjectile() {
                if (!m_onAttachProjectileEventDone) {
                    m_projectile = m_poolHandler.GetInstance();

                    if (m_projectile != null) {
                        IProjectile projectile = m_projectile.GetComponent<IProjectile>();
                        projectile.AttachTo(m_projectileSpawnPoint);
                    } else {
                        Debug.LogError("Projectile not available");
                    }

                    m_onAttachProjectileEventDone = true;
                }
            }

            private void OnAnimDealDamage() {
                if (!m_onDamageEventDone) {
                    if (m_projectile != null) {
                        IProjectile projectile = m_projectile.GetComponent<IProjectile>();
                        projectile.ShootAtPosition(m_cannonEye.position + m_cannonEye.forward * 15f, m_cannonEye.forward, m_data.damage, m_machine.transform);

                        m_projectile = null;
                    }
                    m_onDamageEventDone = true;
                }
            }

            private void OnAnimEnd() {
                if (!m_onAttackEndEventDone) {
                    m_onAttackEndEventDone = true;
                    m_attackState = AttackState.Aim;

                    m_timer = m_data.attackDelay;
                    m_pilot.ReleaseAction(Pilot.Action.Attack);

                    if (m_attacksLeft > 0) {
                        if (!m_machine.GetSignal(Signals.Type.Danger)) {
                            Transition(onOutOfRange);
                        }
                    } else {
                        m_machine.DisableSensor(m_data.retreatTime);
                        Transition(onMaxAttacks);
                    }
                }
            }
        }
    }
}
