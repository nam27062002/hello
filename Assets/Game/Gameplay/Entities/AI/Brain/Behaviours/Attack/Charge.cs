﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class ChargeData : StateComponentData {
			public float speed = 0f;
			public float acceleration = 0f;
			public float damage = 5f;
			public float retreatTime = 0f;
			public string attackPoint;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Charge")]
		public class Charge : StateComponent {

			[StateTransitionTrigger]
			private static string OnChargeEnd = "onChargeEnd";

			private static int m_groundMask;


			protected ChargeData m_data;
			private object[] m_transitionParam;

			private Vector3 m_target;

			private float m_speed;
			private float m_elapsedTime;
			private MeleeWeapon m_meleeWeapon;


			public override StateComponentData CreateData() {
				return new ChargeData();
			}

			public override System.Type GetDataType() {
				return typeof(ChargeData);
			}

			protected override void OnInitialise() {
				m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions");

				m_data = m_pilot.GetComponentData<ChargeData>();
				m_machine.SetSignal(Signals.Type.Alert, true);

				m_transitionParam = new object[1];
				m_transitionParam[0] = m_data.retreatTime; // retreat time

				m_meleeWeapon = m_pilot.FindComponentRecursive<MeleeWeapon>();
				m_meleeWeapon.enabled = false;
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_meleeWeapon.damage = m_data.damage;
				m_meleeWeapon.enabled = true;

				m_speed = 0;
				m_pilot.SetMoveSpeed(m_speed, false);
				m_pilot.SlowDown(false);

				Transform target = null;
				if (m_machine.enemy != null) {
					target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
					if (target == null) {
						target = m_machine.enemy;
					}
				}

				m_target = target.position;
				m_target += (m_target - m_machine.position);///.normalized * 3f;

				//lets check if there is any collision in our way
				RaycastHit groundHit;
				if (Physics.Linecast(m_machine.position, m_target, out groundHit, m_groundMask)) {
					m_target = groundHit.point;
					m_target -= (m_target - m_machine.position).normalized * 1f;
				}

				m_elapsedTime = 0f;

				m_machine.SetSignal(Signals.Type.Invulnerable, true);
				m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State _newState) {
				m_machine.SetSignal(Signals.Type.Invulnerable, false);
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
				m_pilot.ReleaseAction(Pilot.Action.Attack);

				m_meleeWeapon.enabled = false;
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_speed, false);
				m_speed = m_data.speed + m_data.acceleration * m_elapsedTime;
				m_elapsedTime += Time.deltaTime;

				//
				if (m_machine.GetSignal(Signals.Type.Danger)) {
					m_pilot.PressAction(Pilot.Action.Attack);
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Attack);
				}
				//

				float m = (m_machine.position - m_target).sqrMagnitude;

				if (m < 0.5f) {
					Transition(OnChargeEnd, m_transitionParam);
				} else {
					m_pilot.GoTo(m_target);
				}

				base.OnUpdate();
			}
		}
	}
}