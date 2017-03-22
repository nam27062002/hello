using UnityEngine;
using System.Collections;

namespace AI {
	public class WaterMachine : MachineOld {

		private enum State {
			Swim = 0,
			JumpingOut,
			JumpingIn
		};

		[SerializeField] private bool m_spawnsInsideWater = true;

		private State m_state;
		private float m_diveTimer;


		public override void Spawn(ISpawner _spawner) {
			m_state = State.Swim;

			if (m_spawnsInsideWater) {
				SetSignal(Signals.Type.InWater, true);
			}

			m_diveTimer = 0f;
			base.Spawn(_spawner);
		}

		public override void CustomUpdate() {
			bool isInsideWater = GetSignal(Signals.Type.InWater);

			switch (m_state) {
				case State.Swim:
					if (!isInsideWater) {						
						m_state = State.JumpingOut;
					}
					break;

				case State.JumpingOut:
					if (!GetSignal(Signals.Type.Latching)) {
						SetSignal(Signals.Type.FallDown, true);
						UseGravity(true);
						m_state = State.JumpingIn;
						m_diveTimer = 1f;
					}
					break;

				case State.JumpingIn:
					if (isInsideWater) {
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

			base.CustomUpdate();
		}
	}
}