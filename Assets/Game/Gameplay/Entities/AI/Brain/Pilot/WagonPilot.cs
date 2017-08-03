using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class WagonPilot : Pilot {
	
		[SerializeField] private float m_speed = 5f;

		private float m_speedFactor = 1f;
		public override float speedFactor { get { return m_speedFactor; } set { m_speedFactor = value; } }

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			SetMoveSpeed(m_speed);
			SetBoostSpeed(m_speed);
		}

		public override void CustomUpdate() {
			base.CustomUpdate();
			m_impulse = Vector3.zero;
			m_externalImpulse = Vector3.zero;
		}
	}
}
