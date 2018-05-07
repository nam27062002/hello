using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ticket : Entity {

	[SerializeField] private float m_timeOutsideScreen = 5f;

	private bool m_wasVisibleByPlayer;
	private float m_timer;


	override public void Spawn(ISpawner _spawner) {        
		base.Spawn(_spawner);

		m_timer = m_timeOutsideScreen;
		m_wasVisibleByPlayer = false;
	}

	public override void CustomUpdate() { 		
		base.CustomUpdate();

		if (m_wasVisibleByPlayer) {
			if (!m_newCamera.IsInsideActivationMinArea(m_bounds.bounds.bounds)) {
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) { 
					Disable(true);
				}
			}
		} else {
			if (m_isOnScreen) {
				m_wasVisibleByPlayer = m_newCamera.IsInside2dFrustrum(m_bounds.bounds.bounds);
				m_timer = m_timeOutsideScreen;
			}
		}
	}

	public override Reward GetOnKillReward(IEntity.DyingReason _reason) {
		Messenger.Broadcast(MessengerEvents.TICKET_COLLECTED);
		return base.GetOnKillReward(_reason);
	}
}
