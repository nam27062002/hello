using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		public abstract class Attack : StateComponent {

			[StateTransitionTrigger]
			private static string OnMaxAttacks = "onMaxAttacks";

			[StateTransitionTrigger]
			private static string OnOutOfRange = "onOutOfRange";

			private bool m_onAttachProjectileEventDone;
			private bool m_onDamageEventDone;
			private bool m_onAttackEndEventDone;

			private int m_attacksLeft;
			private float m_attackDelay;

			private float m_timer;

			protected Pilot m_pilot;
			protected Machine m_machine;
			private PreyAnimationEvents m_animEvents;

			private object[] m_transitionParam;


			protected override void OnInitialise(GameObject _go) {
				m_pilot 		= _go.GetComponent<Pilot>();
				m_machine		= _go.GetComponent<Machine>();
				m_animEvents 	= _go.FindComponentRecursive<PreyAnimationEvents>();
				m_machine.SetSignal(Signals.Alert.name, true);

				m_transitionParam = new object[1];
				m_transitionParam[0] = 10f; // retreat time

				m_attacksLeft = 3;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_pilot.SetSpeed(0);
				if (m_attacksLeft <= 0)
					m_attacksLeft = 3;
				m_attackDelay = 2f;
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
				//add time between attacks?

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
					dir.x = m_machine.enemy.position.x - m_machine.position.x;
					m_pilot.SetDirection(dir.normalized);

					m_onAttachProjectileEventDone = false;
					m_onDamageEventDone = false;
					m_onAttackEndEventDone = false;
					m_attacksLeft--;

					m_timer = m_attackDelay;
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
						if (!m_machine.GetSignal(Signals.Danger.name)) {							
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