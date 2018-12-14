
public class ModEntitySC : ModifierEntity {
	public const string TARGET_CODE = "sc";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModEntitySC(DefinitionNode _def) : base(_def) {          
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

    public ModEntitySC(float _percentage) : base() {
        m_percentage = _percentage;
        BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

	public override void Apply() {
		Entity.AddSCMultiplier(m_percentage);
	}

	public override void Remove() { 
		Entity.AddSCMultiplier(-m_percentage);	
	}
}
