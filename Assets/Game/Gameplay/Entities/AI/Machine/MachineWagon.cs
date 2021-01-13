using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineWagon : Machine {		
		[SerializeField] protected MC_MotionWagon m_wagonMotion = new MC_MotionWagon();

		[SeparatorAttribute("Mine Wagon Setup")]
		[SerializeField] private bool m_canExplode = false;
		[SerializeField] private float m_explosionDamage = 5f;
		[SerializeField] private float m_explosionRadius = 0f;
		[SerializeField] private float m_explosionCameraShake = 1f;

		public override Quaternion orientation 	{ get { return m_wagonMotion.orientation; } set { m_wagonMotion.orientation = value; } }
		public override Vector3 position		{ get { return m_wagonMotion.position; } set { m_wagonMotion.position = value; } }
		public override Vector3 direction 		{ get { return m_wagonMotion.direction; } }
		public override Vector3 groundDirection	{ get { return m_wagonMotion.direction; } }
		public override Vector3 upVector 		{ get { return m_wagonMotion.upVector; } set { m_wagonMotion.upVector = value; } }
		public override Vector3 velocity		{ get { return m_wagonMotion.velocity; } }
		public override Vector3 angularVelocity	{ get { return m_wagonMotion.angularVelocity; } }



		private Explosive m_explosive;



		protected override void Awake() {
			m_motion = m_wagonMotion;
			if (m_canExplode) {
				m_explosive = new Explosive(true, m_explosionDamage, m_explosionRadius, m_explosionCameraShake);
			}

			base.Awake();
		}

		public void SetRails(BSpline.BezierSpline _rails) {
			m_wagonMotion.rails = _rails;
		}

		// Collision events
		protected override void OnCollisionEnter(Collision _collision) {
			if (((1 << _collision.collider.gameObject.layer) & GameConstants.Layers.GROUND_PREYCOL_OBSTACLE) != 0) {
				if (m_canExplode) {
					m_explosive.Explode(m_transform, 2f, false);
				}
				SetSignal(Signals.Type.Destroyed, true);
			}
		}

		protected override void OnCollisionStay(Collision _collision) {

		}

		protected override void OnCollisionExit(Collision _collision) {
			
		}

		protected override void OnTriggerEnter(Collider _other) {
			if (_other.CompareTag("WagonFall")) {
				m_wagonMotion.FreeFall();
			} else if (_other.CompareTag("Player")) {
				if (m_canExplode) {
					m_explosive.Explode(m_transform, 2f, true);
				}
				SetSignal(Signals.Type.Destroyed, true);
			}
		}

		protected override void OnTriggerExit(Collider _other) {
			
		}
	}
}