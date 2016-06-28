using System;

namespace AI {
	public abstract class MachineComponent {

		protected Machine m_machine;
		protected Pilot m_pilot;
		protected ViewControl m_viewControl;

		public abstract void Init();

		public void AttacheMachine(Machine _machine) {
			m_machine = _machine;
		}

		public void AttachPilot(Pilot _pilot) {
			m_pilot = _pilot;
		}

		public void AttachViewControl(ViewControl _viewControl) {
			m_viewControl = _viewControl;
		}

		public abstract void Update();
	}
}