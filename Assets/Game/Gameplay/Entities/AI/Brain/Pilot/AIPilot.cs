using UnityEngine;
using System.Collections;

namespace AI {
	public abstract class AIPilot : Pilot, Spawnable {

		[SerializeField] private StateMachine m_brainResource;
		private StateMachine m_brain;

		protected Vector3 m_homePosition;
		public Vector3 homePosition { get { return m_homePosition; } }

		protected Vector3 m_target;

		public void Spawn(Spawner _spawner) {
			m_area = _spawner.area.bounds;
			m_homePosition = _spawner.transform.position;

			m_target = transform.position;

			// braaiiiinnn ~ ~ ~ ~ ~
			if (m_brain == null) {
				m_brain = Object.Instantiate(m_brainResource) as StateMachine;
			}
			m_brain.Initialise(gameObject, true);
		}

		public override void OnTrigger(string _trigger) {
			m_brain.Transition(_trigger);
		}

		public void GoTo(Vector3 _target) {
			m_target = _target;
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

		void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(m_target, 0.25f);
		}
	}
}