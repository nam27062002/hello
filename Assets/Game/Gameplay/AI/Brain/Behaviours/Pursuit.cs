using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Pursuit")]
		public class Pursuit : StateComponent {
			
			private Pilot m_pilot;
			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_pilot 	= _go.GetComponent<Pilot>();
				m_machine	= _go.GetComponent<Machine>();
				m_machine.SetSignal(Signals.Alert.name, true);
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.SetSpeed(3f); // run speed
			}

			protected override void OnUpdate() {
				Machine enemy = m_machine.enemy;

				if (enemy) {
					m_pilot.GoTo(enemy.transform.position);
				}
			}
		}
	}
}