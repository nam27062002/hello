using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class JumpAttackData : StateComponentData {
			public float jumpVelocityV = 15f;
			public float jumpVelocityH = 5f;
			public float preJumpTime = 0.1f;
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
				Start_Jump,
				Attack
			}

			private JumpAttackData m_data;
			private MeleeWeapon m_meleeWeapon;

			private int m_attacksLeft;
			private float m_timer;

			private AttackState m_attackState;


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
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.PressAction(Pilot.Action.Attack);
				m_machine.SetSignal(Signals.Type.Invulnerable, true);

				if (m_attacksLeft <= 0) 
					m_attacksLeft =  m_data.consecutiveAttacks;
				m_timer = 0f;

				m_attackState = AttackState.Idle;
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Type.Invulnerable, false);

				m_pilot.ReleaseAction(Pilot.Action.Attack);
			}

			protected override void OnUpdate() {
				switch (m_attackState) {
					case AttackState.Idle:
						m_timer -= Time.deltaTime;
						if (m_timer <= 0) {
							StartAttack();
						}
						break;

					case AttackState.Start_Jump:
						m_timer -= Time.deltaTime;
						if (m_timer <= 0) {
							Jump();
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
				m_pilot.PressAction(Pilot.Action.Jump);

				m_attacksLeft--;
				m_timer = m_data.preJumpTime;

				m_attackState = AttackState.Start_Jump;
			}

			private void Jump() {
				m_machine.SetVelocity(new Vector3(m_data.jumpVelocityH * m_pilot.direction.x, m_data.jumpVelocityV, 0f));
				m_attackState = AttackState.Attack;
			}

			private void EndAttack() {
				m_meleeWeapon.enabled = false;
				m_timer = m_data.attackDelay;

				m_pilot.Stop();

				if (m_attacksLeft > 0) {
					if (!m_machine.GetSignal(Signals.Type.Danger)) {		
						Transition(OnOutOfRange);
					}
				} else {
					m_machine.DisableSensor(m_data.retreatTime);
					Transition(OnMaxAttacks);
				}

				m_attackState = AttackState.Idle;
			}
		}
	}
}