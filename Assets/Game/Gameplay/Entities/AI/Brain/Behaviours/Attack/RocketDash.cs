using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class RocketDashData : StateComponentData {
			public float speed = 0f;
			public float acceleration = 0f;
			public float damage = 5f;
			public float retreatTime = 0f;
			public string attackPoint;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Rocket Dash")]
		public class RocketDash : StateComponent {

			[StateTransitionTrigger]
			private static string OnDashEnd = "onDashEnd";

			private static int m_groundMask;


			protected RocketDashData m_data;

			private Vector3 m_target;
			private float m_speed;
			private float m_elapsedTime;



			public override StateComponentData CreateData() {
				return new RocketDashData();
			}

			public override System.Type GetDataType() {
				return typeof(RocketDashData);
			}

			protected override void OnInitialise() {
				m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions");

				m_data = m_pilot.GetComponentData<RocketDashData>();
				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_pilot.SetMoveSpeed(m_data.speed, false);
				m_pilot.SlowDown(false);

				Transform target = null;
				if (m_machine.enemy != null) {
					target = m_machine.enemy.FindTransformRecursive(m_data.attackPoint);
					if (target == null) {
						target = m_machine.enemy;
					}
				}

				m_target = target.position;

				//lets check if there is any collision in our way
				RaycastHit groundHit;
				if (Physics.Linecast(m_machine.position, m_target, out groundHit, m_groundMask)) {
					m_target = groundHit.point;				
				}

				m_speed = 0;
				m_elapsedTime = 0;

				m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State _newState) {				
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_speed, false);
				m_speed = m_data.speed + m_data.acceleration * m_elapsedTime;
				m_elapsedTime += Time.deltaTime;

				float m = (m_machine.position - m_target).sqrMagnitude;
				float d = m_speed * Time.deltaTime;

				if (m < d * d) {
					Transition(OnDashEnd);
				} else {
					m_pilot.GoTo(m_target);
				}

				base.OnUpdate();
			}
		}
	}
}