using UnityEngine;
using System.Collections;

namespace AI {
	public class MachineWater : Machine {

		private enum State {
			Swim = 0,
			JumpingOut,
			JumpingIn
		};

		[SerializeField] protected MC_MotionWater m_waterMotion = new MC_MotionWater();
		[SerializeField] private bool m_spawnsInsideWater = true;

		public override Quaternion orientation 	{ get { return m_waterMotion.orientation; } set { m_waterMotion.orientation = value; } }
		public override Vector3 position		{ get { return m_waterMotion.position; } set { m_waterMotion.position = value; } }
		public override Vector3 direction 		{ get { return m_waterMotion.direction; } }
		public override Vector3 upVector 		{ get { return m_waterMotion.upVector; } set { m_waterMotion.upVector = value; } }
		public override Vector3 velocity		{ get { return m_waterMotion.velocity; } }
		public override Vector3 angularVelocity	{ get { return m_waterMotion.angularVelocity; } }


		private State m_state;
		private float m_diveTimer;

		protected override void Awake() {
			m_motion = m_waterMotion;
			base.Awake();
		}
	}
}