using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class ChargeData : PursuitData {
			public float acceleration = 0f;
		}

		[CreateAssetMenu(menuName = "Behaviour/Attack/Charge")]
		public class Charge : Pursuit {

			private float acceleration;
			private float m_elapsedTime;


			public override StateComponentData CreateData() {
				return new ChargeData();
			}

			public override System.Type GetDataType() {
				return typeof(ChargeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<ChargeData>();
				m_machine.SetSignal(Signals.Type.Alert, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				base.OnEnter(oldState, param);

				m_pilot.SetMoveSpeed(m_data.speed);
				m_pilot.SlowDown(false);

				acceleration = ((ChargeData)m_data).acceleration;
				m_elapsedTime = 0f;

				m_pilot.PressAction(Pilot.Action.Button_A);
			}

			protected override void OnExit(State _newState) {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
			}

			protected override void OnUpdate() {
				m_pilot.SetMoveSpeed(m_data.speed + acceleration * m_elapsedTime * m_elapsedTime);
				m_elapsedTime += Time.deltaTime;

				base.OnUpdate();
			}
		}
	}
}