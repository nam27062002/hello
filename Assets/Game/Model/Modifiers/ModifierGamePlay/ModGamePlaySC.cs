
public class ModGamePlaySC : ModifierGamePlay {
	public const string TARGET_CODE = "sc";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlaySC(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_SCORE.ToHexString("#"));
	}

	public override void Apply() {
		Entity.AddSCMultiplier(m_percentage);
		MissionManager.AddSCMultiplier(m_percentage);
		Chest.AddSCMultiplier(m_percentage);
	}

	public override void Remove () {
		Entity.AddSCMultiplier(-m_percentage);
        MissionManager.AddSCMultiplier(-m_percentage);
		Chest.AddSCMultiplier(-m_percentage);
	}
}
