﻿
public class ModEntityXP : ModifierEntity {
	public const string TARGET_CODE = "xp";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModEntityXP(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		Entity.AddXpMultiplier(m_percentage);
	}

	public override void Remove() { 
		Entity.AddXpMultiplier(-m_percentage);	
	}
}
