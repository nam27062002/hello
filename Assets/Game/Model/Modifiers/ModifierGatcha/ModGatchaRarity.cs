
public class ModGatchaRarity : ModifierGatcha {
	public const string TARGET_CODE = "rarity_chance";

	//------------------------------------------------------------------------//
	private float[] m_values;

	//------------------------------------------------------------------------//
	public ModGatchaRarity(DefinitionNode _def) : base(_def) {
		m_values = _def.GetAsArray<float>("param1", ";");
	}

	public override void Apply() {
		EggManager.SetWeightIDs(m_values);
		EggManager.BuildDynamicProbabilities();
	}

	public override void Remove() { 
		EggManager.RestoreWeightIDs();
		EggManager.BuildDynamicProbabilities();
	}
}
