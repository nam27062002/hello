﻿
public class ModDragonLifeDrain : ModifierDragon {
	public const string TARGET_CODE = "life_drain";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonLifeDrain(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
		m_textParam = m_percentage + "%";
	}

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddDrainModifier(m_percentage);
	}

}
