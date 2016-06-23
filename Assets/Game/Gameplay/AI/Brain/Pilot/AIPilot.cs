using UnityEngine;
using System.Collections;

namespace AI {
	public abstract class AIPilot : Pilot {

		[SerializeField] private AISM.StateMachine m_brainResource;
		private AISM.StateMachine m_brain;

		protected override void Awake() {
			base.Awake();

			// braaiiiinnn ~ ~ ~ ~ ~
			m_brain = Object.Instantiate(m_brainResource) as AISM.StateMachine;
			m_brain.Initialise(gameObject, true);
		}

		public override void OnTrigger(string _trigger) {
			m_brain.Transition(_trigger);
		}

		protected virtual void Update() {
			// state machine updates
			if (m_brain != null) 
				m_brain.Update();
		}
	}
}