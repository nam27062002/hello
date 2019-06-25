
public class CollectibleAnniversaryCake : CollectibleEntity {

	private DragonSuperSize m_dragonSuperSize;

	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);
		m_dragonSuperSize = InstanceManager.player.GetComponent<DragonSuperSize>();
	}

	public override Reward GetOnKillReward(DyingReason _reason) {
		if (m_dragonSuperSize.time > 0f) {
			RewardManager.killCount[sku] = 0;
		} else {
			if (RewardManager.killCount.ContainsKey(sku) && RewardManager.killCount[sku] >= 5) {
				RewardManager.killCount[sku] = 0;

				Messenger.Broadcast(MessengerEvents.ALL_HUNGRY_LETTERS_COLLECTED);
			}
		}

		return reward;
	}
}
