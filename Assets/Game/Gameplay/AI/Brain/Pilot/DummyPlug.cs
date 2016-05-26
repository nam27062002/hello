using UnityEngine;
using System.Collections;

namespace AI {
	public class DummyPlug : Pilot {

		public float m_editorRadius = 1f;
		public float m_editorSpeed = 1f;
		public float m_distanceDelta = 0.1f;

		void LateUpdate() {
			float speed = m_editorSpeed;
			float m = (transform.position - m_target).sqrMagnitude;

			if (m < m_distanceDelta) {
				m_target = Random.insideUnitSphere * m_editorRadius;
				m_target.z = 0;
			} 

			SetSpeed(m_editorSpeed);
			GoTo(m_target);

			m_machine.SetSignal(Machine.Signal.Alert, true);

			Avoid(m_machine.GetSignal(Machine.Signal.Warning));
		}
	}
}