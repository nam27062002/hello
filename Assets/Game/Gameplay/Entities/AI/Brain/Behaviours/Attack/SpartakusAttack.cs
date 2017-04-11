using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class SpartakusAttackData : StateComponentData {
			public float jumpVelocityV = 15f;
			public float jumpVelocityH = 5f;
			public float damage = 5f;
			public int consecutiveAttacks = 3;
			public float dizzyTime = 0f;
			public float retreatTime = 0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Spartakus Attack")]
		public class SpartakusAttack : StateComponent {
			[StateTransitionTrigger]
			private static string OnDizzyRecover = "onDizzyRecover";

			[StateTransitionTrigger]
			private static string OnOutOfRange = "onOutOfRange";

			//-----------------------------------------------------
			private enum AttackState {
				Idle = 0,
				Attack,
				Dizzy	
			}

			private SpartakusAnimationEvents m_animEvents;

			private SpartakusAttackData m_data;
			private MeleeWeapon m_meleeWeapon;

			private int m_attacksLeft;
			private float m_timer;

			private AttackState m_attackState;


			//-----------------------------------------------------
			public override StateComponentData CreateData() {
				return new SpartakusAttackData();
			}

			public override System.Type GetDataType() {
				return typeof(SpartakusAttackData);
			}

			protected override void OnInitialise() {
				m_animEvents = m_pilot.FindComponentRecursive<SpartakusAnimationEvents>();

				m_data = m_pilot.GetComponentData<SpartakusAttackData>();
				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();
				m_meleeWeapon.damage = m_data.damage;
				m_meleeWeapon.enabled = false;
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_animEvents.onJumpImpulse   += new SpartakusAnimationEvents.OnJumpImpulseDelegate(Jump);
				m_animEvents.onJumpReception += new SpartakusAnimationEvents.OnJumpReceptionDelegate(EndAttack);
				m_animEvents.onDizzyRecover  += new SpartakusAnimationEvents.OnDizzyRecoverDelegate(DizzyRecover);

				m_pilot.PressAction(Pilot.Action.Attack);
				m_machine.SetSignal(Signals.Type.InvulnerableBite, true);

				if (m_attacksLeft <= 0) 
					m_attacksLeft = m_data.consecutiveAttacks;

				m_attackState = AttackState.Idle;
			}

			protected override void OnExit(State _newState) {
				m_animEvents.onJumpImpulse   -= new SpartakusAnimationEvents.OnJumpImpulseDelegate(Jump);
				m_animEvents.onJumpReception -= new SpartakusAnimationEvents.OnJumpReceptionDelegate(EndAttack);
				m_animEvents.onDizzyRecover  -= new SpartakusAnimationEvents.OnDizzyRecoverDelegate(DizzyRecover);

				m_machine.SetSignal(Signals.Type.InvulnerableBite, false);
				m_pilot.ReleaseAction(Pilot.Action.Attack);
				m_pilot.ReleaseAction(Pilot.Action.Button_B);
				m_pilot.ReleaseAction(Pilot.Action.Jump);

				m_pilot.SetDirection(m_pilot.direction, false);
			}

			protected override void OnUpdate() {
				switch (m_attackState) {
					case AttackState.Idle:
						StartAttack();
						break;

					case AttackState.Attack:						
						break;

					case AttackState.Dizzy:
						if (m_timer > 0f) {
							m_timer -= Time.deltaTime;
							if (m_timer <= 0f) {
								m_pilot.ReleaseAction(Pilot.Action.Button_B);
							}
						}
						break;
				}
			}

			private void StartAttack() {
				m_pilot.PressAction(Pilot.Action.Jump);

				m_attacksLeft--;

				m_attackState = AttackState.Attack;
			}

			private void Jump() {
				m_meleeWeapon.enabled = true;

				Vector3 direction = Vector3.right;
				if (m_machine.enemy.position.x < m_machine.position.x) {
					direction = Vector3.left;
				}
				m_pilot.SetDirection(direction, true);

				m_machine.SetVelocity(new Vector3(m_data.jumpVelocityH * direction.x, m_data.jumpVelocityV, 0f));
			}

			private void EndAttack() {
				m_meleeWeapon.enabled = false;
				m_pilot.Stop();

				if (m_attacksLeft > 0) {
					if (m_machine.GetSignal(Signals.Type.Danger)) {
						m_attackState = AttackState.Idle;
					} else {
						Transition(OnOutOfRange);
					}
				} else {
					m_pilot.PressAction(Pilot.Action.Button_B);
					m_machine.SetSignal(Signals.Type.InvulnerableBite, false);

					m_timer = m_data.dizzyTime;

					m_attackState = AttackState.Dizzy;
				}
			}

			private void DizzyRecover() {
				m_machine.DisableSensor(m_data.retreatTime);
				Transition(OnDizzyRecover);
			}
		}
	}
}