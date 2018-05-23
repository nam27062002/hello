
public class ModDragonDamage : ModifierDragon {
	public const string TARGET_CODE = "boost_armor";

	//------------------------------------------------------------------------//
	private float m_value; // percentage

	//------------------------------------------------------------------------//
	public ModDragonDamage(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddArmorModifier(m_value);
	}
}
