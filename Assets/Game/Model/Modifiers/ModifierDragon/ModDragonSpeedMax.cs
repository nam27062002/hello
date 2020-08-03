
public class ModDragonSpeedMax : ModifierDragon {
	public const string TARGET_CODE = "speed";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonSpeedMax(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonMotion motion = InstanceManager.player.dragonMotion;
		if (motion != null)
			motion.AddSpeedModifier(m_percentage);
	}
}
	