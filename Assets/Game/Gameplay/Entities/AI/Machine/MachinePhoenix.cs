using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : Machine {

		bool m_phoenixActive = false;
		public GameObject m_fire;

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// Set not fire view
			Deactivate();
		}


		protected virtual void Update() {
			if ( !m_phoenixActive )
			{
				if ( m_pilot.IsActionPressed(Pilot.Action.Fire))
				{	
					// Activate Phoenix Mode!!
					Activate();
				}
			}
			else
			{
				if ( !m_pilot.IsActionPressed(Pilot.Action.Fire))
				{
					// Deactivate Phoenix Mode
					Deactivate();
				}
			}
		}

		private void Activate()
		{
			m_phoenixActive = true;
			m_fire.SetActive(true);
		}

		private void Deactivate()
		{
			m_phoenixActive = false;
			m_fire.SetActive(false);
		}




	}
}