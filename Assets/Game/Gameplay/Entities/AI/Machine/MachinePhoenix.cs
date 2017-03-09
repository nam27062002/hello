using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : Machine {

		bool m_phoenixActive = false;
		public GameObject m_fire;

		public GameObject m_view;
		public GameObject m_viewWhenBurning;

		public ParticleData m_onFireParticle;
		public ParticleData m_onFireEndsParticle;

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// Set not fire view
			Deactivate();
		}


		public override void CustomUpdate() {
			base.CustomUpdate();
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

			m_view.SetActive(false);
			// on fire particle

			m_fire.SetActive(true);
			m_viewWhenBurning.SetActive(true);
		}

		private void Deactivate()
		{
			m_phoenixActive = false;

			m_view.SetActive(true);
			// on fire particle

			m_fire.SetActive(false);
			m_viewWhenBurning.SetActive(false);
		}
	}
}