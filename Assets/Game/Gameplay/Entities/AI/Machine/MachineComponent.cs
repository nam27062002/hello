using System;
using UnityEngine;

namespace AI {
	public abstract class MachineComponent {

		public enum Type {
			Motion = 0,
			Sensor_Enemy,
			Sensor_Player,
			Edible,
			Inflammable,
			Eater
		}

		public abstract Type type { get; }

		protected IEntity m_entity;
		protected IMachine m_machine;
		protected Pilot m_pilot;

		public abstract void Init();

		public virtual void Attach(IMachine _machine, IEntity _entity, Pilot _pilot) {
			m_entity = _entity;
			m_pilot = _pilot;
			m_machine = _machine;
		}

		public abstract void Update();
		public virtual void FixedUpdate() {}

		//--------------------------------------------------
		// Debug
		//--------------------------------------------------
		public virtual void OnDrawGizmosSelected(Transform _go) {}
		//--------------------------------------------------
	}
}