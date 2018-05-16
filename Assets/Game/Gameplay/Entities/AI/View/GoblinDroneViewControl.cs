using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinDroneViewControl : ViewControl {
	
	[SeparatorAttribute("Rotation Speeds")]
	[SerializeField] private float m_headRotationSpeed;
	[SerializeField] private float m_helixRotationSpeed;

	[SeparatorAttribute("Mobile parts")]
	[SerializeField] private Transform m_head;
	[SerializeField] private Transform m_helixLeft01;
	[SerializeField] private Transform m_helixLeft02;
	[SerializeField] private Transform m_helixRight01;
	[SerializeField] private Transform m_helixRight02;


	private float m_headAngle;
	private float m_helixRotation;

	private Transform m_player;


	protected override void Awake() {
		base.Awake();
		m_headAngle = 40f;
		m_helixRotation = 0f;

		m_player = InstanceManager.player.transform;
	}

	void LateUpdate() {
		if (m_head != null) {
			// lets find the position of the target relative to the cabin
			Vector3 targetDir = m_player.position - m_head.position;
			targetDir.z = 0f;
			targetDir.Normalize();

			Vector3 cross = Vector3.Cross(targetDir, Vector3.down);

			//the goblin should follow our target!
			float angle = (cross.z) * -40f;

			m_headAngle = Mathf.Lerp(m_headAngle, angle, m_headRotationSpeed * Time.smoothDeltaTime);
			m_head.localRotation = Quaternion.Euler(m_headAngle, 0, 0);
		}

		m_helixRotation += m_helixRotationSpeed * Time.deltaTime;
		m_helixLeft01.localRotation = Quaternion.AngleAxis(m_helixRotation, GameConstants.Vector3.left);
		m_helixLeft02.localRotation = Quaternion.AngleAxis(m_helixRotation, GameConstants.Vector3.left);
		m_helixRight01.localRotation = Quaternion.AngleAxis(m_helixRotation, GameConstants.Vector3.right);
		m_helixRight02.localRotation = Quaternion.AngleAxis(m_helixRotation, GameConstants.Vector3.right);
	}
}
