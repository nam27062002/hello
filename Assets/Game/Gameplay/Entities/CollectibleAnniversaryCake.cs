using UnityEngine;

public class CollectibleAnniversaryCake : CollectibleEntity {

	private int m_cakesToHuge;
	private DragonSuperSize m_dragonSuperSize;

	protected override void Awake() {
		base.Awake();

		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
		m_cakesToHuge = def.GetAsInt("anniversaryCakesToHuge", 1);
		m_dragonSuperSize = InstanceManager.player.GetComponent<DragonSuperSize>();
	}

	public override Reward GetOnKillReward(DyingReason _reason) {
		if (m_dragonSuperSize.time > 0f) {
			RewardManager.killCount[sku] = 0;
		} else {
			if (RewardManager.killCount.ContainsKey(sku) && RewardManager.killCount[sku] >= m_cakesToHuge) {
				RewardManager.killCount[sku] = 0;
				Messenger.Broadcast(MessengerEvents.ALL_HUNGRY_LETTERS_COLLECTED);
			}
		}

		return reward;
	}
}
