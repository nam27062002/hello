
public class ModGatchaPet : ModifierGatcha {
	public const string TARGET_CODE = "pet_chance";

	//------------------------------------------------------------------------//
	private string m_sku;
	private float m_weight;

	//------------------------------------------------------------------------//
	public ModGatchaPet(DefinitionNode _def) : base(_def) {
		m_sku = _def.Get("param1");
		m_weight = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		Metagame.RewardEgg.OverridePetProb(m_sku, m_weight);
	}

	public override void Remove() { 
		Metagame.RewardEgg.RemoveOverridePetProb(m_sku);
	}
}
