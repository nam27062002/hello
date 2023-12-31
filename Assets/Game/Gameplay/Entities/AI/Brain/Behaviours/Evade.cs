﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class EvadeData : StateComponentData {
			public float boostSpeed = 5f;
			public float panicSpeed = 10f;
			public bool faceDirectionOnBoost = false;
		}

		[CreateAssetMenu(menuName = "Behaviour/Evade")]
		public class Evade : StateComponent {

			private EvadeData m_data;
			private bool m_alertRestoreValue;
			private bool m_faceDirectionRestoreValue;

			public override StateComponentData CreateData() {
				return new EvadeData();
			}

			public override System.Type GetDataType() {
				return typeof(EvadeData);
			}

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<EvadeData>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_alertRestoreValue = m_machine.GetSignal(Signals.Type.Alert);

				m_machine.SetSignal(Signals.Type.Alert, true);
				m_pilot.SetBoostSpeed(m_data.boostSpeed);
			}

			protected override void OnExit(State newState) {
				m_machine.SetSignal(Signals.Type.Alert, m_alertRestoreValue);

				m_pilot.Avoid(false);
				m_pilot.ReleaseAction(Pilot.Action.Boost);
				m_pilot.ReleaseAction(Pilot.Action.Scared);
			}

			protected override void OnUpdate() {
				m_pilot.Avoid(m_machine.GetSignal(Signals.Type.Warning));

				if (m_machine.GetSignal(Signals.Type.Critical)) {
					m_pilot.SetBoostSpeed(m_data.panicSpeed);
					m_pilot.PressAction(Pilot.Action.Scared);
				} else {
					m_pilot.SetBoostSpeed(m_data.boostSpeed);
					m_pilot.ReleaseAction(Pilot.Action.Scared);
				}

				if (m_machine.GetSignal(Signals.Type.Danger)) {
					m_pilot.PressAction(Pilot.Action.Boost);
				} else {
					m_pilot.ReleaseAction(Pilot.Action.Boost);
				}
			}
		}
	}
}