
public class ModGamePlaySC : ModifierGamePlay {
	public const string TARGET_CODE = "sc";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModGamePlaySC(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		m_textColor = UIConstants.PET_CATEGORY_SCORE;
		m_textParam = m_percentage + "%";
	}

	public override void Apply() {
		Entity.AddSCMultiplier(m_percentage);
		Mission.AddSCMultiplier(m_percentage);
		Chest.AddSCMultiplier(m_percentage);
	}

	public override void Remove () {
		Entity.AddSCMultiplier(-m_percentage);
		Mission.AddSCMultiplier(-m_percentage);
		Chest.AddSCMultiplier(-m_percentage);
	}
}
