
public class ModGamePlayKillChainFast : ModifierGamePlay {
	public const string TARGET_CODE = "kill_chain_fast";

	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModGamePlayKillChainFast(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		RewardManager.AddKillStreakPercentage(m_value);
	}

	public override void Remove() {
		RewardManager.AddKillStreakPercentage(-m_value);
	}
}
