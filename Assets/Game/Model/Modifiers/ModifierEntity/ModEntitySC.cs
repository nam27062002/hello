
public class ModEntitySC : ModifierEntity {
	public const string TARGET_CODE = "sc";

	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModEntitySC(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		Entity.AddSCMultiplier(m_value);
	}

	public override void Remove() { 
		Entity.AddSCMultiplier(-m_value);	
	}
}
