using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class OnMineHitData : StateComponentData {
			public Pilot.Action m_action = Pilot.Action.Button_A;
		}

		[CreateAssetMenu(menuName = "Behaviour/On Mine Hit")]
		public class OnMineHit : StateComponent {

			protected OnMineHitData m_data;
			public override StateComponentData CreateData() {
				return new OnMineHitData();
			}

			public override System.Type GetDataType() {
				return typeof(OnMineHitData);
			}

			private static int m_mineLayer;
			private byte m_buttonPressed = 0;

			protected override void OnInitialise() {
				base.OnInitialise();
				m_mineLayer = LayerMask.NameToLayer("Mines");
				m_data = m_pilot.GetComponentData<OnMineHitData>();
			}

			protected override void OnUpdate(){
				if (m_machine.GetSignal(Signals.Type.Trigger)) {
					object[] param = m_machine.GetSignalParams(Signals.Type.Trigger);
					if (param != null && param.Length > 0 && ((GameObject)param[0]).layer.CompareTo(m_mineLayer) == 0) {
						m_machine.SetSignal(Signals.Type.Trigger, false);
						if ( !m_pilot.IsActionPressed(m_data.m_action )){
							m_pilot.PressAction(m_data.m_action);
							m_buttonPressed = 10;
						}
					}
				} else {
					if ( m_buttonPressed > 0){
						--m_buttonPressed;
						if ( m_buttonPressed <= 0 )
						{
							m_pilot.ReleaseAction(m_data.m_action);
						}
					}
				}
			}
		}
	}
}