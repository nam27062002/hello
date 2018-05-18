
public class ModDragonSpeedMax : ModifierDragon {
	//------------------------------------------------------------------------//
	private float m_value;

	//------------------------------------------------------------------------//
	public ModDragonSpeedMax(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonMotion motion = InstanceManager.player.dragonMotion;
		if (motion != null)
			motion.AddSpeedModifier(m_value);
	}
}
	