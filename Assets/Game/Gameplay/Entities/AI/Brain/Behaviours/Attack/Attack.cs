using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[System.Serializable]
		public abstract class AttackData : StateComponentData {
			public int consecutiveAttacks = 3;
			public float attackDelay = 0f;
			public float retreatTime = 0f;
			public int facing = 1;
			public bool faceEnemy = false;
		}

		public abstract class Attack : StateComponent {
			[StateTransitionTrigger]
			private static string OnMaxAttacks = "onMaxAttacks";

			[StateTransitionTrigger]
			private static string OnOutOfRange = "onOutOfRange";


			protected AttackData m_data;

			private bool m_onAttachProjectileEventDone;
			private bool m_onDamageEventDone;
			private bool m_onAttackEndEventDone;

			private int m_attacksLeft;

			private float m_timer;

			private PreyAnimationEvents m_animEvents;

			private object[] m_transitionParam;


			protected override void OnInitialise() {
				m_animEvents 	= m_pilot.FindComponentRecursive<PreyAnimationEvents>();
				m_machine.SetSignal(Signals.Type.Alert, true);

				m_transitionParam = new object[1];
				m_transitionParam[0] = m_data.retreatTime; // retreat time

				m_attacksLeft = m_data.consecutiveAttacks;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_pilot.SetMoveSpeed(0);
				if (m_attacksLeft <= 0)
					m_attacksLeft =  m_data.consecutiveAttacks;
				m_timer = 0f;

				m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onAttackEnd 		+= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);

				m_onAttachProjectileEventDone = true;
				m_onDamageEventDone = true;
				m_onAttackEndEventDone = true;
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Attack);

				m_animEvents.onAttachProjectile -= new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onAttackEnd 		-= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
			}

			protected override void OnUpdate() {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0) {
					m_timer = 0;
					if (m_onAttackEndEventDone) {
						StartAttack();
					}
				}
			}

			private void StartAttack() {
				if (m_machine.enemy == null) {
					Transition(OnOutOfRange);
				} else {
					m_pilot.PressAction(Pilot.Action.Attack);

					Vector3 dir = Vector3.zero;
					if (m_data.facing > 0) {
						dir = m_machine.enemy.position - m_machine.position;
					} else {
						dir = m_machine.position - m_machine.enemy.position;
					}

					if (m_data.faceEnemy) {						
						dir.z = 0;
					} else {
						dir.y = 0;
						dir.z = 0;
					}
					m_pilot.SetDirection(dir.normalized);

					m_onAttachProjectileEventDone = false;
					m_onDamageEventDone = false;
					m_onAttackEndEventDone = false;
					m_attacksLeft--;

					m_timer = m_data.attackDelay;
				}
			}

			private void OnAttachProjectile() {
				if (!m_onAttachProjectileEventDone) {
					m_onAttachProjectileEventDone = true;
					OnAttachProjectileExtended();
				}
			}
			protected virtual void OnAttachProjectileExtended() {}


			private void OnAnimDealDamage() {
				if (!m_onDamageEventDone) {
					m_onDamageEventDone = true;
					OnAnimDealDamageExtended();
				}
			}
			protected virtual void OnAnimDealDamageExtended() {}


			private void OnAnimEnd() {
				if (!m_onAttackEndEventDone) {
					m_onAttackEndEventDone = true;
					OnAnimEndExtended();

					// if this prey has to wait more before attacking again, stop the animation
					if (m_timer > 0) {
						m_pilot.ReleaseAction(Pilot.Action.Attack);
					}

					if (m_attacksLeft > 0) {
						if (!m_machine.GetSignal(Signals.Type.Danger)) {		
							Transition(OnOutOfRange);
						}
					} else {
						Transition(OnMaxAttacks, m_transitionParam);
					}
				}
			}
			protected virtual void OnAnimEndExtended() {}
		}
	}
}