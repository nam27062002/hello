﻿using System;

namespace AI {
	public abstract class MachineComponent {

		protected Machine m_machine;
		protected Pilot m_pilot;

		public void AttachPilot(Pilot _pilot) {
			m_pilot = _pilot;
		}

		public abstract void Update();
	}
}