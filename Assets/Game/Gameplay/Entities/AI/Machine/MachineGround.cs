using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineGround : Machine {		
		[SerializeField] protected MC_MotionGround m_groundMotion = new MC_MotionGround();
		public MC_MotionGround groundMotion { get{return m_groundMotion;} }

		public override Quaternion orientation 	{ get { return m_groundMotion.orientation; } set { m_groundMotion.orientation = value; } }
		public override Vector3 position		{ get { return m_groundMotion.position; } set { m_groundMotion.position = value; } }
		public override Vector3 direction 		{ get { return m_groundMotion.direction; } }
		public override Vector3 groundDirection	{ get { return m_groundMotion.groundDirection; } }
		public override Vector3 upVector 		{ get { return m_groundMotion.upVector; } set { m_groundMotion.upVector = value; } }
		public override Vector3 velocity		{ get { return m_groundMotion.velocity; } }
		public override Vector3 angularVelocity	{ get { return m_groundMotion.angularVelocity; } }

		protected override void Awake() {
			m_motion = m_groundMotion;
			base.Awake();
		}

		//
		protected override void OnReleaseHold() {
			SetSignal(Signals.Type.FallDown, true);
		}
	}
}