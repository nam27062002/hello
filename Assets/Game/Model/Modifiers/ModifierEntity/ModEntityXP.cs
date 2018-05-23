
public class ModEntityXP : ModifierEntity {
	public const string TARGET_CODE = "xp";

	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModEntityXP(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		Entity.AddXpMultiplier(m_value);
	}

	public override void Remove() { 
		Entity.AddXpMultiplier(-m_value);	
	}
}
