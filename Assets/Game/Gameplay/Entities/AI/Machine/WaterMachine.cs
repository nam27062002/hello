using UnityEngine;
using System.Collections;

namespace AI {
	public class WaterMachine : Machine {

		private bool m_insideWater;


		public override void Spawn(ISpawner _spawner) {
			m_insideWater = true;
			base.Spawn(_spawner);
		}

		protected override void Update() {
			// lets see if we are out of water
			if (!GetSignal(Signals.Type.Latching)) {
				bool isInsideWater = WaterAreaManager.instance.IsInsideWater(position);
				if (m_insideWater != isInsideWater) {
					if (!isInsideWater)
						m_motion.Stop();
					
					SetSignal(Signals.Type.FallDown, !isInsideWater);
					UseGravity(!isInsideWater);
					m_insideWater = isInsideWater;
				}
			}

			base.Update();
		}
	}
}