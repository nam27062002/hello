
public class ModDragonBoostRegen : ModifierDragon {
	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModDragonBoostRegen(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonBoostBehaviour boost = InstanceManager.player.dragonBoostBehaviour;
		if (boost)
			boost.AddRefillBonus(m_value);
	}
}
