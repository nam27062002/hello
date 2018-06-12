
public class Collectible : Entity {
	public override Reward GetOnKillReward(bool _burnt) {
		return reward;
	}
}
