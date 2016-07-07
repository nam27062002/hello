using UnityEngine;
using System.Collections;
using AISM;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Explode")]
		public class Explode : StateComponent {
			[SerializeField] private float m_damage = 5f;

			private Machine m_machine;

			protected override void OnInitialise(GameObject _go) {
				m_machine	= _go.GetComponent<Machine>();
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				// explode
				DragonPlayer dragon = InstanceManager.player;
				if (dragon.HasMineShield()) {
					dragon.LoseMineShield();
				} else {
					dragon.GetComponent<DragonHealthBehaviour>().ReceiveDamage(m_damage, m_machine.transform);
				}

				m_machine.SetSignal(Signals.Destroyed.name, true);
			}
		}
	}
}