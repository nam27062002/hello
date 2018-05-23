
public class ModDragonAirCurrent : ModifierDragon {
	public const string TARGET_CODE = "air_current";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonAirCurrent(DefinitionNode _def) : base(_def) {
		m_percentage = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonMotion motion = InstanceManager.player.dragonMotion;
		if (motion != null)
			motion.AddAirCurrentModifier(m_percentage);
	}
}
	