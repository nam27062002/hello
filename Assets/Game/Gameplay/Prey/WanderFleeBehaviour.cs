using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class WanderFleeBehaviour : WanderBehaviour {

	private SensePlayer m_sensor;
	private Transform m_dragonMouth; // we'll flee away from dragon's mouth!! that's the real danger! 
	private DragonMotion m_dragon;

	override protected void Awake() {
		m_sensor = GetComponent<SensePlayer>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_dragonMouth = m_dragon.tongue;

		base.Awake();
	}

	override protected void FixedUpdate() {
		if (m_sensor.alert) {
			if (m_state == State.Idle) {
				m_nextState = State.Move;
			} else {
				m_motion.Flee(m_dragonMouth.position);
			}
		} /*else if (m_state == State.Move) {
			m_motion.Evade(m_dragonMouth.position, m_dragon.GetVelocity(), m_dragon.GetMaxSpeed());
		}*/

		base.FixedUpdate();
	}

	override protected void UpdateRandomTarget() {
		float rSqr = m_sensor.sensorMinRadius * m_sensor.sensorMinRadius;

		if ((m_target - (Vector2)m_dragonMouth.position).sqrMagnitude <= rSqr) {
			ChooseTarget();
		} else {
			base.UpdateRandomTarget();
		}
	}

	override protected void ChooseTarget() {
		if (m_sensor.alert && m_motion.HasGroundSensor()) {
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