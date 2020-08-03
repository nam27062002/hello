
public class CollectibleEntity : Entity {
	public override Reward GetOnKillReward(DyingReason _reason) {
		return reward;
	}
}
