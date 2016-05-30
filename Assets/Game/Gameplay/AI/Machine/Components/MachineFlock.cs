using UnityEngine;
using System.Collections.Generic;


namespace AI {
	public class MachineFlock : MachineComponent {

		private List<IMachine> m_flock;
		public List<IMachine> flock { get { return m_flock; } set { m_flock = value; } }

		public MachineFlock() {
			m_flock = new List<IMachine>();
		}

		public override void Init() {}
		public override void Update() {}
	}
}