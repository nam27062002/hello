
public class ModEntityScore : ModifierEntity {
	public const string TARGET_CODE = "score";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModEntityScore(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

    public ModEntityScore(float _percentage) : base(TARGET_CODE) {
        m_percentage = _percentage;
        BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

	public override void Apply() {
		Entity.AddScoreMultiplier(m_percentage);
	}

	public override void Remove() { 
		Entity.AddScoreMultiplier(-m_percentage);	
	}
}
