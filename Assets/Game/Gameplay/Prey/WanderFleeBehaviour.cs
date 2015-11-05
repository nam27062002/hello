using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class WanderFleeBehaviour : WanderBehaviour {

	private SensePlayer m_sensor;
	private Transform m_dragonMouth; // we'll flee away from dragon's mouth!! that's the real danger! 

	override protected void Awake() {
		m_sensor = GetComponent<SensePlayer>();
		m_dragonMouth = InstanceManager.player.GetComponent<DragonMotion>().tongue;

		base.Awake();
	}

	override protected void FixedUpdate() {
		if (m_sensor.alert) {
			if (m_state == State.Idle) {
				m_nextState = State.Move;
			} else {
				m_motion.Flee(m_dragonMouth.position);
			}
		}

		base.FixedUpdate();
	}

	override protected void ChooseTarget() {
		if (m_sensor.alert) {
			Vector3 direction = transform.position - m_dragonMouth.position;
			if (direction.x < 0) {
				m_target = m_motion.ProjectToGround(m_area.bounds.min);
			} else {
				m_target = m_motion.ProjectToGround(m_area.bounds.max);
			}
		} else {
			base.ChooseTarget();
		}
	}
}