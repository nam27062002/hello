using System;
using UnityEngine;

namespace AI {
	public abstract class MachineComponent {

		protected Entity m_entity;
		protected IMachine m_machine;
		protected Pilot m_pilot;

		public abstract void Init();

		public void Attach(IMachine _machine, Entity _entity, Pilot _pilot) {
			m_entity = _entity;
			m_pilot = _pilot;
			m_machine = _machine;
		}

		public abstract void Update();


		// Debug
		public virtual void OnDrawGizmosSelected(Transform _go) {}
	}
}