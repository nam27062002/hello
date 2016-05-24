namespace AI {
	public abstract class MachineComponent {

		protected IPilot m_pilot;

		public void AttachPilot(IPilot _pilot) {
			m_pilot = _pilot;
		}

		public abstract void Update();
	}
}