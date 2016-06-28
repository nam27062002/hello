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
			private float m_attacksLeft;

			private Pilot m_pilot;
			private Machine m_machine;
			private PreyAnimationEvents m_animEvents;


			protected override void OnInitialise(GameObject _go) {
				m_pilot 		= _go.GetComponent<Pilot>();
				m_machine		= _go.GetComponent<Machine>();
				m_animEvents 	= _go.FindComponentRecursive<PreyAnimationEvents>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				m_pilot.SetSpeed(0);
				m_attacksLeft = 3f;

				m_animEvents.onAttachProjectile += new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onAttackEnd 		+= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);

				// first attack
				m_pilot.PressAction(Pilot.Action.Attack);

				StartAttack();
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Attack);

				m_animEvents.onAttachProjectile -= new PreyAnimationEvents.OnAttachprojectile(OnAttachProjectile);
				m_animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
				m_animEvents.onAttackEnd 		-= new PreyAnimationEvents.OnAttackEndDelegate(OnAnimEnd);
			}

			protected override void OnUpdate() {
				//add time between attacks?
			}

			private void StartAttack() {
				Vector3 dir = Vector3.zero;
				dir.x = m_machine.enemy.position.x - m_machine.position.x;
				m_pilot.SetDirection(dir.normalized);

				m_onAttachProjectileEventDone = false;
				m_onDamageEventDone = false;
				m_onAttackEndEventDone = false;
				m_attacksLeft--;
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

					if (m_attacksLeft > 0) {
						if (m_machine.GetSignal(Signals.Danger.name)) {
							StartAttack();
						} else {
							Transition(OnOutOfRange);
						}
					} else {
						Transition(OnMaxAttacks);
					}
				}
			}
			protected virtual void OnAnimEndExtended() {}
		}
	}
}