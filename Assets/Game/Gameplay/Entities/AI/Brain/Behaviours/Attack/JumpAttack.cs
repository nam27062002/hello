using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class JumpAttackData : StateComponentData {
			public float jumpVelocity = 20f;
			public int consecutiveAttacks = 3;
			public float attackDelay = 0f;
			public float retreatTime = 0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Jump Attack")]
		public class JumpAttack : StateComponent {
			[StateTransitionTrigger]
			private static string OnMaxAttacks = "onMaxAttacks";

			[StateTransitionTrigger]
			private static string OnOutOfRange = "onOutOfRange";

			//-----------------------------------------------------
			private enum AttackState {
				Idle = 0,
				Attack
			}

			private JumpAttackData m_data;
			private MeleeWeapon m_meleeWeapon;

			private int m_attacksLeft;
			private float m_timer;

			private AttackState m_attackState;

			private object[] m_transitionParam;


			//-----------------------------------------------------
			public override StateComponentData CreateData() {
				return new JumpAttackData();
			}

			public override System.Type GetDataType() {
				return typeof(JumpAttackData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<JumpAttackData>();
				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();
				m_meleeWeapon.enabled = false;

				m_transitionParam = new object[1];
				m_transitionParam[0] = m_data.retreatTime; // retreat time
			}

			protected override void OnEnter(State oldState, object[] param) {
				if (m_attacksLeft <= 0) 
					m_attacksLeft =  m_data.consecutiveAttacks;
				m_timer = 0f;

				m_attackState = AttackState.Idle;
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Type.Invulnerable, false);
			}

			protected override void OnUpdate() {
				if (m_timer > 0) {
					m_timer -= Time.deltaTime;
				}

				switch (m_attackState) {
					case AttackState.Idle:
						if (m_timer <= 0) {
							StartAttack();
						}
						break;

					case AttackState.Attack:
						if (!m_pilot.IsActionPressed(Pilot.Action.Jump)) {
							EndAttack();
						}
						break;
				}
			}

			private void StartAttack() {
				m_meleeWeapon.enabled = true;

				m_pilot.PressAction(Pilot.Action.Attack);
				m_pilot.PressAction(Pilot.Action.Jump);
				m_machine.SetSignal(Signals.Type.Invulnerable, true);
				m_machine.SetVelocity(Vector3.up * m_data.jumpVelocity);

				m_attacksLeft--;
				m_timer = m_data.attackDelay;

				m_attackState = AttackState.Attack;
			}

			private void EndAttack() {
				m_meleeWeapon.enabled = false;

				m_machine.SetSignal(Signals.Type.Invulnerable, false);
				m_pilot.ReleaseAction(Pilot.Action.Attack);
				m_pilot.Stop();

				if (m_attacksLeft > 0) {
					if (!m_machine.GetSignal(Signals.Type.Danger)) {		
						Transition(OnOutOfRange);
					}
				} else {
					Transition(OnMaxAttacks, m_transitionParam);
				}

				m_attackState = AttackState.Idle;
			}
		}
	}
}