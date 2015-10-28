using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class WanderFleeBehaviour : WanderBehaviour {

	private SensePlayer m_sensor;
	private Transform m_dragonMouth; // we'll flee away from dragon's mouth!! that's the real danger! 

	override protected void Awake() {
		m_sensor = GetComponent<SensePlayer>();
		m_dragonMouth = InstanceManager.player.GetComponent<DragonMotion>().mouth;

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
}
