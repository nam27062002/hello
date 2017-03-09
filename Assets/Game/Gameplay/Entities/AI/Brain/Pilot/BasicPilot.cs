using UnityEngine;

namespace AI {
	public class BasicPilot : AIPilot {		

		[SerializeField] private bool m_updateMachinePos = true;

		public override void CustomUpdate() {
			base.CustomUpdate();

			// this machine won't use impulse while moving around
			m_impulse = Vector3.zero;
			m_externalImpulse = Vector3.zero;

			if (m_updateMachinePos) {
				m_machine.position = m_target;
			}
		}
	}
}