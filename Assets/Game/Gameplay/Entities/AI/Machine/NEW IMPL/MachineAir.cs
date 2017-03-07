using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineAir : MachineGeneric {

		[SerializeField] protected MC_MotionAir m_airMotion = new MC_MotionAir();

		public override Vector3 position		{ get { return m_airMotion.position; } set { m_airMotion.position = value; } }
		public override Vector3 direction 		{ get { return m_airMotion.direction; } }
		public override Vector3 upVector 		{ get { return m_airMotion.upVector; } set { m_airMotion.upVector = value; } }
		public override Vector3 velocity		{ get { return m_airMotion.velocity; } }
		public override Vector3 angularVelocity	{ get { return m_airMotion.angularVelocity; } }

		protected override void Awake() {
			m_motion = m_airMotion;
			base.Awake();
		}

		public override void FaceDirection(bool _value) {
			m_airMotion.faceDirection = _value;
		}

		public override bool IsFacingDirection() {			
			return m_airMotion.faceDirection;
		}
	}
}