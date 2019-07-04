using System.Collections.Generic;

public class ModGatchaPet : ModifierGatcha {
	public const string TARGET_CODE = "pet_chance";

	//------------------------------------------------------------------------//
	private List<string> m_skuList;
	private float m_weight;

	//------------------------------------------------------------------------//
	public ModGatchaPet(DefinitionNode _def) : base(TARGET_CODE, _def) {
        m_skuList = _def.GetAsList<string>("param1", ";");
		m_weight = _def.GetAsFloat("param2");

        if (m_skuList.Count == 1) {
            DefinitionNode definition = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_skuList[0]);
            BuildTextParams(definition.GetLocalized("tidName"), StringUtils.FormatNumber(m_weight, 2), UIConstants.PET_CATEGORY_SPECIAL.ToHexString("#"));
        } else {
            BuildTextParams(StringUtils.FormatNumber(m_weight, 2), UIConstants.PET_CATEGORY_SPECIAL.ToHexString("#"));
        }
	}

	public override bool isValid() { 
		List<string> resourceIDs = new List<string>();		
		foreach (string sku in m_skuList) {
			resourceIDs.AddRange(HDAddressablesManager.Instance.GetResourceIDsForPet(sku));
		}		 
		return HDAddressablesManager.Instance.IsResourceListAvailable(resourceIDs); 	
	}

	public override void Apply() {
        for (int i = 0; i < m_skuList.Count; ++i)
            Metagame.RewardEgg.OverridePetProb(m_skuList[i], m_weight);
	}

	public override void Remove() { 
        for (int i = 0; i < m_skuList.Count; ++i)
            Metagame.RewardEgg.RemoveOverridePetProb(m_skuList[i]);
	}
}
