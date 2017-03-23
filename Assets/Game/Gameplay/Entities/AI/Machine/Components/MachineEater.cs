using UnityEngine;
using System.Collections.Generic;


namespace AI {
	public class MachineEater : MachineComponent {

		public override Type type { get { return Type.Eater; } }


		public MachineEater() {}

		public override void Init() {}

		public override void Update() {
			if (m_machine.GetSignal(Signals.Type.Hungry)) {
				IMachine machine = GetEdible(0.25f);
				if (machine != null) {
					machine.Bite();
				}
			}
		}

		private IMachine GetEdible(float _radiusSqr) {
			MachineOld[] machines = GameObject.FindObjectsOfType<MachineOld>() as MachineOld[];

			for (int i = 0 ; i < machines.Length; i++) { 
				if (machines[i] != m_machine) {
					float dSqr = Vector2.SqrMagnitude(m_machine.position - machines[i].position);
					if (dSqr < _radiusSqr) {
						return machines[i];
					}
				}
			}

			return null;
		}
	}
}