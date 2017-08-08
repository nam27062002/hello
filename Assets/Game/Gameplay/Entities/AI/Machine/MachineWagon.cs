using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineWagon : Machine {		
		[SerializeField] protected MC_MotionWagon m_wagonMotion = new MC_MotionWagon();

		public override Quaternion orientation 	{ get { return m_wagonMotion.orientation; } set { m_wagonMotion.orientation = value; } }
		public override Vector3 position		{ get { return m_wagonMotion.position; } set { m_wagonMotion.position = value; } }
		public override Vector3 direction 		{ get { return m_wagonMotion.direction; } }
		public override Vector3 groundDirection	{ get { return m_wagonMotion.direction; } }
		public override Vector3 upVector 		{ get { return m_wagonMotion.upVector; } set { m_wagonMotion.upVector = value; } }
		public override Vector3 velocity		{ get { return m_wagonMotion.velocity; } }
		public override Vector3 angularVelocity	{ get { return m_wagonMotion.angularVelocity; } }

		protected override void Awake() {
			m_motion = m_wagonMotion;
			base.Awake();
		}

		public void SetRails(BSpline.BezierSpline _rails) {
			m_wagonMotion.rails = _rails;
		}

		// Collision events
		protected override void OnCollisionEnter(Collision _collision) {
			if (((1 << _collision.collider.gameObject.layer) & GROUND_MASK) != 0) {
				SetSignal(Signals.Type.Destroyed, true);
			}
		}

		protected override void OnCollisionStay(Collision _collision) {

		}

		protected override void OnCollisionExit(Collision _collision) {
			
		}

		protected override void OnTriggerEnter(Collider _other) {
			if (_other.tag.Equals("WagonFall")) {
				m_wagonMotion.FreeFall();
			}
		}

		protected override void OnTriggerExit(Collider _other) {
			
		}
	}
}