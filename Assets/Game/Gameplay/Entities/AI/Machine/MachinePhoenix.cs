using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : MachineAir {
		//--------------------------------------------------
		[SeparatorAttribute("Phoenix Effects")]
		[SerializeField] private GameObject m_fire;
		[SerializeField] private ParticleSystem m_fireParticle;


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
			m_fire.SetActive(true);
			m_fireParticle.Play();
		}

		private void Deactivate() {
			m_phoenixActive = false;
			m_fire.SetActive(false);
			m_fireParticle.Stop();
		}
	}
}