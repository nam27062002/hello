using UnityEngine;
using System.Collections;

namespace AI {
	public abstract class AIPilot : Pilot, Spawnable {

		[SerializeField] private AISM.StateMachine m_brainResource;
		private AISM.StateMachine m_brain;

		public void Spawn(Spawner _spawner) {
			m_area = _spawner.area.bounds;
			m_homePosition = _spawner.transform.position;

			// braaiiiinnn ~ ~ ~ ~ ~
			if (m_brain == null) {
				m_brain = Object.Instantiate(m_brainResource) as AISM.StateMachine;
			}
			m_brain.Initialise(gameObject, true);
		}

		public override void OnTrigger(string _trigger) {
			m_brain.Transition(_trigger);
		}

		protected virtual void Update() {
			// state machine updates
			if (m_brain != null) {
				m_brain.Update();
			}
			
			// if this machine is outside his area, go back to home position (if it has this behaviour)
			if (!m_area.Contains(transform.position)) {
				m_machine.SetSignal(Signals.BackToHome.name, true);
			}
		}
	}
}