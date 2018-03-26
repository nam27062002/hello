using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineOrientable : Machine {
		[SeparatorAttribute]
		[SerializeField] private MC_Motion.UpVector m_defaultUpVector = MC_Motion.UpVector.Up;
		[SerializeField] private float m_orientationSpeed = 120f;
		[SerializeField] private bool m_canBeflipped = true;

		private Vector3 m_direction;
		private Vector3 m_upVector;
		private Quaternion m_rotation;
		protected Quaternion m_targetRotation;

		public override Quaternion orientation 	{ get { return m_rotation; } set { m_rotation = value; } }
		public override Vector3 direction 		{ get { return m_direction; } }
		public override Vector3 upVector 		{ get { return m_upVector; } set { m_upVector = value; } }

		protected override void Awake() {
			switch (m_defaultUpVector) {
				case MC_Motion.UpVector.Up: 		m_upVector = GameConstants.Vector3.up; 		break;
				case MC_Motion.UpVector.Down: 		m_upVector = GameConstants.Vector3.down; 	break;
				case MC_Motion.UpVector.Right: 		m_upVector = GameConstants.Vector3.right; 	break;
				case MC_Motion.UpVector.Left: 		m_upVector = GameConstants.Vector3.left; 	break;
				case MC_Motion.UpVector.Forward: 	m_upVector = GameConstants.Vector3.forward; break;
				case MC_Motion.UpVector.Back: 		m_upVector = GameConstants.Vector3.back;	break;
			}
			base.Awake();
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);
			if (m_canBeflipped) {
				//TODO: check spawner
				Vector3 s = m_transform.localScale;
				s.x *= -1;
				m_transform.localScale = s;
			}
		}

		public override void CustomUpdate() {			
			if (!IsDead()) {
				m_direction = m_pilot.direction;
				m_targetRotation = Quaternion.LookRotation(m_direction + GameConstants.Vector3.back * 0.1f, m_upVector);

				m_rotation = Quaternion.RotateTowards(m_rotation, m_targetRotation, Time.deltaTime * m_orientationSpeed);
				m_transform.rotation = m_rotation;
			}

			base.CustomUpdate();
		}
	}
}