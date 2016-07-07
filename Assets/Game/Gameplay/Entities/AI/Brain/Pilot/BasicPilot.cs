using UnityEngine;

namespace AI {
	public class BasicPilot : AIPilot {		
		protected override void Update() {
			base.Update();

			// this machine won't use impulse while moving around
			m_impulse = Vector3.zero;
			m_externalImpulse = Vector3.zero;
			transform.position = m_target;
		}
	}
}