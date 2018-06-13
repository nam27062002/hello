
public class ModGamePlaySC : ModifierGamePlay {
	public const string TARGET_CODE = "sc";

	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModGamePlaySC(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
		m_textColor = UIConstants.PET_CATEGORY_SCORE;
	}

	public override void Apply() {
		Entity.AddSCMultiplier(m_value);
		Mission.AddSCMultiplier(m_value);
		Chest.AddSCMultiplier(m_value);
	}

	public override void Remove () {
		Entity.AddSCMultiplier(-m_value);
		Mission.AddSCMultiplier(-m_value);
		Chest.AddSCMultiplier(-m_value);
	}

	protected override string GetDescritiponParam() {
		return m_value + "%";
	}
}
