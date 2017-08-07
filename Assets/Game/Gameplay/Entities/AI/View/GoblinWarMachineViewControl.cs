using UnityEngine;
using System.Collections;

public class GoblinWarMachineViewControl : ViewControl {

	private Transform m_eye; // for aiming purpose
	private float m_targetAim;
	private Quaternion m_targetRotation;
	private AI.IMachine m_machine;

	protected override void Awake()	{
		base.Awake();
		m_eye = transform.Find("eye");
	}


	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_machine = m_entity.machine;

		m_targetAim = 0f;
		m_eye.localRotation = Quaternion.identity;
	}


	public override void CustomUpdate() {		
		if (m_aim != m_targetAim) {
			float angle = 90 * m_aim;
			if (m_machine.direction.x < 0) {
				angle = 180 - angle;
			}
			m_targetRotation = Quaternion.AngleAxis(angle, Vector3.back); 

			m_targetAim = m_aim;
		}
	
		m_eye.localRotation = Quaternion.RotateTowards(m_eye.localRotation, m_targetRotation, Time.deltaTime * 240f);
	}
}
