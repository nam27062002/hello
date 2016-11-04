using UnityEngine;
using System.Collections;

namespace AI {
	public class WaterMachine : Machine {

		private enum State {
			Swim = 0,
			JumpingOut,
			JumpingIn
		};

		private State m_state;
		private float m_diveTimer;

		private bool m_spawnSplashParticles;



		public override void Spawn(ISpawner _spawner) {
			m_state = State.Swim;
			m_diveTimer = 0f;
			m_spawnSplashParticles = false;
			base.Spawn(_spawner);
		}

		protected override void Update() {
			bool isInsideWater = WaterAreaManager.instance.IsInsideWater(position);

			switch (m_state) {
				case State.Swim:
					if (!isInsideWater) {
						GameObject ps = ParticleManager.Spawn("PS_Dive", transform.position, "Water");
						if (ps != null) {
							ps.transform.localScale = Vector3.one * 0.5f;
						}
						m_state = State.JumpingOut;
					}
					break;

				case State.JumpingOut:
					if (!GetSignal(Signals.Type.Latching)) {
						SetSignal(Signals.Type.FallDown, true);
						UseGravity(true);
						m_spawnSplashParticles = true;
						m_state = State.JumpingIn;
						m_diveTimer = 0.5f;
					}
					break;

				case State.JumpingIn:
					if (isInsideWater) {
						if (m_spawnSplashParticles) {
							ParticleManager.Spawn("PS_Dive", transform.position, "Water");
							m_spawnSplashParticles = false;
						}

						m_pilot.SetDirection(Vector3.down, true);
						m_diveTimer -= Time.deltaTime;
						if (m_diveTimer <= 0f) {
							m_pilot.SetDirection(Vector3.down, false);
							SetSignal(Signals.Type.FallDown, false);
							UseGravity(false);
							m_state = State.Swim;
						}
					}
					break;
			}

			base.Update();
		}
	}
}