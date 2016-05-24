using UnityEngine;

namespace AI {
	public class MachineMotion : MachineComponent {

		private Machine m_machine;

		private Vector3 m_position;
		private Vector3 m_orientation;

		public MachineMotion(Machine _machine) {
			m_machine = _machine;
			m_position = m_machine.transform.position;
		}

		public override void Update() {
			if (m_pilot != null) {
				// lets move!
				Vector3 impulse = m_pilot.GetImpulse();
				m_position += impulse;
				m_machine.gameObject.transform.position = m_position;
			}
		}
	}
}