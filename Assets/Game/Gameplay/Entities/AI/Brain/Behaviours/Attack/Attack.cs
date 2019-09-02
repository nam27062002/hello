using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public abstract class AttackData : StateComponentData {
			public int consecutiveAttacks = 3;
			public float attackDelay = 0f;
			public float retreatTime = 0f;
			public bool stop = true;
			public bool faceEnemy = true;
			public bool faceEnemyY = false;
			public int facing = 1;
			public bool forceFaceToShoot = false;
			public float maxFacingAngle = 90;
		}

		public abstract class Attack : StateComponent {
			[StateTransitionTrigger]
			private static int onMaxAttacks = UnityEngine.Animator.StringToHash("onMaxAttacks");

            [StateTransitionTrigger]
			private static int onOutOfRange = UnityEngine.Animator.StringToHash("onOutOfRange");



            protected AttackData m_data;

			private bool m_onAttachProjectileEventDone;
			private bool m_onDamageEventDone;
			private bool m_onAttackEndEventDone;

			private int m_attacksLeft;

			private float m_timer;

            private Vector3 m_targetOffset;			

			protected Vector3 m_facingTarget = Vector3.zero;
			public Vector3 facingTarget { get{ return m_facingTarget; }}
            protected PreyAnimationEvents m_animEvents;


			protected override void OnInitialise() {
				m_animEvents 	= m_viewControl.animationEvents;
				m_machine.SetSignal(Signals.Type.Alert, true);

				m_attacksLeft = m_data.consecutiveAttacks;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				if (m_data.stop)
					m_pilot.Stop();

				if (m_attacksLeft <= 0)
					m_attacksLeft =  m_data.consecutiveAttacks;
				m_timer = 0f;

				m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onAttackEnd 		+= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
				m_animEvents.onInterrupt 		+= new PreyAnimationEvents.OnInterruptDelegate(OnAnimEnd);

				m_onAttachProjectileEventDone = true;
				m_onDamageEventDone = true;
				m_onAttackEndEventDone = true;

                m_targetOffset = GameConstants.Vector3.zero;

                if (m_machine.enemy != null) {
                    Entity entity = m_machine.enemy.GetComponent<Entity>();
                    if (entity != null && entity.circleArea != null) {
                        m_targetOffset = entity.circleArea.offset;
                    }
                }
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Attack);

				m_pilot.SetDirection(m_machine.direction, false);

				m_animEvents.onAttachProjectile -= new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onAttackEnd 		-= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
				m_animEvents.onInterrupt 		-= new PreyAnimationEvents.OnInterruptDelegate(OnAnimEnd);

				m_pilot.ReleaseAction(Pilot.Action.Stop);
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					m_timer = 0;
					if (m_onAttackEndEventDone) {

						if (m_machine.enemy == null) {
							Transition(onOutOfRange);
						}
						else
						{
							bool startAttack = true;
							if (m_data.forceFaceToShoot) {
                                Vector3 enemyPos = m_machine.enemy.position + m_targetOffset;

								// Check angle to know if we can shoot
								Vector3 dir = Vector3.right;
								if (m_data.facing > 0) {
                                    dir = enemyPos - m_machine.position;
								} else {
                                    dir = m_machine.position - enemyPos;
								}
							
								Vector3 pilotDir = m_pilot.transform.forward;
								float angle = Vector2.Angle( dir, pilotDir);
								if (Mathf.Abs(angle) > m_data.maxFacingAngle) {
									startAttack = false;
									FacingToShoot();
								} else {
									// Save shooting position
                                    m_facingTarget = enemyPos;
								}
							}

							if (startAttack)
								StartAttack();
						}
					}
				}
			}

			protected void FacingToShoot()
			{
				if (m_data.faceEnemy) {
					Vector3 dir = Vector3.zero;
					if (m_data.facing > 0) {
						dir = m_machine.enemy.position - m_machine.position;
					} else {
						dir = m_machine.position - m_machine.enemy.position;
					}

					if (m_data.faceEnemyY) {						
						dir.z = 0;
					} else {
						dir.y = 0;
						dir.z = 0;
					}
					m_pilot.SetDirection(dir.normalized, true);
				}	
			}


			protected virtual void StartAttack() {
				
				m_pilot.PressAction(Pilot.Action.Attack);

				if (m_data.faceEnemy) {
                    Vector3 dir = Vector3.zero;
                    Vector3 enemyPos = m_machine.enemy.position + m_targetOffset;
					
					if (m_data.facing > 0) {
                        dir = enemyPos - m_machine.position;
					} else {
                        dir = m_machine.position - enemyPos;
					}

					if (m_data.faceEnemyY) {						
						dir.z = 0;
					} else {
						dir.y = 0;
						dir.z = 0;
					}
					m_pilot.SetDirection(dir.normalized, true);
				}

				m_onAttachProjectileEventDone = false;
				m_onDamageEventDone = false;
				m_onAttackEndEventDone = false;
				m_attacksLeft--;

				m_timer = m_data.attackDelay;
				
			}

			protected void OnAttachProjectile() {
				if (!m_onAttachProjectileEventDone) {
					m_onAttachProjectileEventDone = true;
					OnAttachProjectileExtended();
				}
			}
			protected virtual void OnAttachProjectileExtended() {}


			protected void OnAnimDealDamage() {
				if (!m_onDamageEventDone) {
					m_onDamageEventDone = true;
					OnAnimDealDamageExtended();
				}
			}
			protected virtual void OnAnimDealDamageExtended() {}


			protected void OnAnimEnd() {
				if (!m_onAttackEndEventDone) {
					m_onAttackEndEventDone = true;
					OnAnimEndExtended();

					// if this prey has to wait more before attacking again, stop the animation
					if (m_timer > 0) {
						m_pilot.ReleaseAction(Pilot.Action.Attack);
					}

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
			protected virtual void OnAnimEndExtended() {}
		}
	}
}