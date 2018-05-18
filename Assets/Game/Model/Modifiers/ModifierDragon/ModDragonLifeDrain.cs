
public class ModDragonLifeDrain : ModifierDragon {
	//------------------------------------------------------------------------//
	private float m_value; // percentage

	//------------------------------------------------------------------------//
	public ModDragonLifeDrain(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddDrainModifier(m_value);
	}

}
