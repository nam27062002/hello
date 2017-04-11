using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : MachineAir {
		//--------------------------------------------------
		[SeparatorAttribute("Phoenix Effects")]
		[SerializeField] private GameObject m_fire;

		[SerializeField] private GameObject m_view;
		[SerializeField] private GameObject m_viewWhenBurning;

		[SerializeField] private ParticleData m_onFireParticle;
		[SerializeField] private ParticleData m_onFireEndsParticle;


		//--------------------------------------------------
		private bool m_phoenixActive = false;


		//--------------------------------------------------
		protected override void Awake() {
			base.Awake();
			Deactivate();
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// Set not fire view
			Deactivate();
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			if (!m_phoenixActive) {
				if (m_pilot.IsActionPressed(Pilot.Action.Fire)) {	
					// Activate Phoenix Mode!!
					Activate();
				}
			} else {
				if (!m_pilot.IsActionPressed(Pilot.Action.Fire)) {
					// Deactivate Phoenix Mode
					Deactivate();
				}
			}
		}

		private void Activate() {
			m_phoenixActive = true;

			m_view.SetActive(false);
			// on fire particle

			m_fire.SetActive(true);
			m_viewWhenBurning.SetActive(true);
		}

		private void Deactivate() {
			m_phoenixActive = false;

			m_view.SetActive(true);
			// on fire particle

			m_fire.SetActive(false);
			m_viewWhenBurning.SetActive(false);
		}
	}
}