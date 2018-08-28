
public class ModGatchaPet : ModifierGatcha {
	public const string TARGET_CODE = "pet_chance";

	//------------------------------------------------------------------------//
	private string m_sku;
	private float m_weight;

	//------------------------------------------------------------------------//
	public ModGatchaPet(DefinitionNode _def) : base(_def) {
		m_sku = _def.Get("param1");
		m_weight = _def.GetAsFloat("param2");

		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_sku);
		BuildTextParams(def.GetLocalized("tidName"), StringUtils.FormatNumber(m_weight, 2), UIConstants.PET_CATEGORY_SPECIAL.ToHexString("#"));
	}

	public override void Apply() {
		Metagame.RewardEgg.OverridePetProb(m_sku, m_weight);
	}

	public override void Remove() { 
		Metagame.RewardEgg.RemoveOverridePetProb(m_sku);
	}
}
