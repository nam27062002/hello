
public class ModGatchaRarity : ModifierGatcha {
	public const string TARGET_CODE = "rarity_chance";

	//------------------------------------------------------------------------//
	private float[] m_values;

	//------------------------------------------------------------------------//
	public ModGatchaRarity(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_values = _def.GetAsArray<float>("param1", ";");
		BuildTextParams(UIConstants.PET_CATEGORY_SPECIAL.ToHexString("#"));
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
