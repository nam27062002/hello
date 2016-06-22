using UnityEngine;
using System.Collections.Generic;


namespace AI {
	public class MachineEdible : MachineComponent {
		
		public MachineEdible() {}

		public override void Init() {}

		public void Bite() {
			Debug.Log(m_machine.name + " : I've been bitten!! >_<");
			m_machine.SetSignal(Signals.Destroyed.name, true);
		}

		public override void Update() {}
	}
}