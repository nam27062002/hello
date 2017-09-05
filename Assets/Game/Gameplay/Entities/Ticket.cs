using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ticket : Entity {

	public override Reward GetOnKillReward(bool _burnt) {
		Messenger.Broadcast(GameEvents.TICKET_COLLECTED);
		return base.GetOnKillReward(_burnt);
	}
}
