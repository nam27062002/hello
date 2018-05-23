
public class ModGamePlayKillChainLonger : ModifierGamePlay {
	public const string TARGET_CODE = "kill_chain_longer";

	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModGamePlayKillChainLonger(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		RewardManager.AddDurationPercentage(m_value);
	}

	public override void Remove() {
		RewardManager.AddDurationPercentage(-m_value);
	}
}
