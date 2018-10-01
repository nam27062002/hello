
public class ModGamePlayMissionReward : ModifierGamePlay {
	public const string TARGET_CODE = "mission_reward";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlayMissionReward(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		MissionManager.AddSCMultiplier(m_percentage);
	}

	public override void Remove() {
        MissionManager.AddSCMultiplier(-m_percentage);
	}
}
