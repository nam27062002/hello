
public class ModDragonAirCurrent : ModifierDragon {
	public const string TARGET_CODE = "air_current";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonAirCurrent(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonMotion motion = InstanceManager.player.dragonMotion;
		if (motion != null)
			motion.AddAirCurrentModifier(m_percentage);
	}
}
	