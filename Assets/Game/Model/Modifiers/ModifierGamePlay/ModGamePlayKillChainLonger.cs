
public class ModGamePlayKillChainLonger : ModifierGamePlay {
	public const string TARGET_CODE = "kill_chain_longer";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayKillChainLonger(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		m_textParam = m_percentage + "%";
	}

	public override void Apply() {
		RewardManager.AddDurationPercentage(m_percentage);
	}

	public override void Remove() {
		RewardManager.AddDurationPercentage(-m_percentage);
	}
}
