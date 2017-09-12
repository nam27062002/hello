using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinWarBoatViewControl : ViewControl {
	
	[SeparatorAttribute("Targeting system")]
	[SerializeField] private Transform m_targetDummy;
	[SerializeField] private Transform m_goblinEye;
	[SerializeField] private Transform m_cannonEye;
	[SerializeField] private float m_cabinOrientationSpeed;

	[SeparatorAttribute("Mobile parts")]
	[SerializeField] private Transform m_cabin;
	[SerializeField] private Transform m_cannon;
	[SerializeField] private Transform m_gearBig;
	[SerializeField] private Transform m_gearSmall;


	private float m_cabinAngle;


	protected override void Awake() {
		base.Awake();
		m_cabinAngle = 40f;
	}

	public override void CustomUpdate() {
		base.CustomUpdate();

		// lets find the position of the target relative to the cabin
		Vector3 targetDir = m_targetDummy.position - m_goblinEye.position;
		targetDir.z = 0f;
		targetDir.Normalize();

		Vector3 cross = Vector3.Cross(targetDir, Vector3.down);

		//the goblin should follow our target!
		float angle = (cross.z - 1f) * -90f;
		if (angle < 40f) angle = 40f;
		if (angle > 140f) angle = 140f;

		m_cabinAngle = Mathf.Lerp(m_cabinAngle, angle, m_cabinOrientationSpeed * Time.smoothDeltaTime);
		m_cabin.localRotation = Quaternion.Euler(m_cabinAngle, 0, 0);


		//then aim to destroy it!! >; O
		//we are not delaying the movement here!
		targetDir = m_targetDummy.position - m_cannonEye.position;
		targetDir.z = 0f;
		targetDir.Normalize();

		float cannonAngle = Vector3.Angle(Vector3.left, targetDir);
		if (targetDir.y > 0) {
			cannonAngle = -cannonAngle;
		}

		m_cannon.localRotation = Quaternion.AngleAxis(cannonAngle, Vector3.forward);

		//add rotate the gears (this "move" our cannon)
		m_gearBig.localRotation = Quaternion.AngleAxis(cannonAngle, Vector3.forward);
		m_gearSmall.localRotation = Quaternion.AngleAxis(cannonAngle, Vector3.back);
	}
}
