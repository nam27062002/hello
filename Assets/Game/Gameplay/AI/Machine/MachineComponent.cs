using System;

namespace AI {
	public abstract class MachineComponent {

		protected Machine m_machine;
		protected Pilot m_pilot;

		public abstract void Init();

		public void AttacheMachine(Machine _machine) {
			m_machine = _machine;
		}

		public void AttachPilot(Pilot _pilot) {
			m_pilot = _pilot;
		}

		public abstract void Update();
	}
}