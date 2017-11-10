using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ticket : Entity {

	private bool m_wasVisibleByPlayer;


	override public void Spawn(ISpawner _spawner) {        
		base.Spawn(_spawner);

		m_wasVisibleByPlayer = false;
	}

	public override void CustomUpdate() { 		
		base.CustomUpdate();

		if (m_wasVisibleByPlayer) {
			if (!m_newCamera.IsInsideActivationMinArea(m_bounds.bounds.bounds)) {
				Disable(true);
			}
		} else {
			if (m_isOnScreen) {
				m_wasVisibleByPlayer = m_newCamera.IsInside2dFrustrum(m_bounds.bounds.bounds);
			}
		}
	}

	public override Reward GetOnKillReward(bool _burnt) {
		Messenger.Broadcast(GameEvents.TICKET_COLLECTED);
		return base.GetOnKillReward(_burnt);
	}
}
