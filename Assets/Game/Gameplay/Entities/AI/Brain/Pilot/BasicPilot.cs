using UnityEngine;

namespace AI {
	public class BasicPilot : AIPilot {		

		[SerializeField] private bool m_updateMachinePos = true;
        [SerializeField] private bool m_updateMachineRot = false;

		public override void CustomUpdate() {
            base.CustomUpdate();
            // this machine won't use impulse while moving around
            m_impulse = Vector3.zero;
			m_externalImpulse = Vector3.zero;

			if (m_updateMachinePos) {
				m_machine.position = m_target;
			}

            if (m_updateMachineRot) {
                m_machine.transform.rotation = Quaternion.LookRotation(m_direction + GameConstants.Vector3.back * 0.1f, m_machine.upVector);
            }            
        }
	}
}