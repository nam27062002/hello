
public class ModGamePlayKillChainLonger : ModifierGamePlay {
	public const string TARGET_CODE = "kill_chain_longer";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayKillChainLonger(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		RewardManager.AddDurationPercentage(m_percentage);
	}

	public override void Remove() {
		RewardManager.AddDurationPercentage(-m_percentage);
	}
}
