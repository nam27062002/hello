using UnityEngine;

public class CollectibleAnniversaryCake : CollectibleEntity {
	public override Reward GetOnKillReward(DyingReason _reason) {
		Messenger.Broadcast<Vector3>(MessengerEvents.ANNIVERSARY_CAKE_SLICE_EATEN, m_machine.position);
		return reward;
	}
}
