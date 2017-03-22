using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineWallWalking : MachineGeneric {

		[SerializeField] protected MC_MotionWallWalking m_wallWalkingMotion = new MC_MotionWallWalking();

		public override Vector3 position		{ get { return m_wallWalkingMotion.position; } set { m_wallWalkingMotion.position = value; } }
		public override Vector3 direction 		{ get { return m_wallWalkingMotion.direction; } }
		public override Vector3 groundDirection	{ get { return m_wallWalkingMotion.groundDirection; } }
		public override Vector3 upVector 		{ get { return m_wallWalkingMotion.upVector; } set { m_wallWalkingMotion.upVector = value; } }
		public override Vector3 velocity		{ get { return m_wallWalkingMotion.velocity; } }
		public override Vector3 angularVelocity	{ get { return m_wallWalkingMotion.angularVelocity; } }

		protected override void Awake() {
			m_motion = m_wallWalkingMotion;
			base.Awake();
		}

		public override void UseGravity(bool _value) { 
			m_wallWalkingMotion.checkCollisions = _value;
		}

		//
		protected override void OnReleaseHold() {
			SetSignal(Signals.Type.FallDown, true);
		}
	}
}