
public class ModDragonArmor : ModifierDragon {
	public const string TARGET_CODE = "armor";

	//------------------------------------------------------------------------//
	private float m_percentage;

	//------------------------------------------------------------------------//
	public ModDragonArmor(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_percentage = _def.GetAsFloat("param1");
		BuildTextParams(
			StringUtils.MultiplierToPercentage(UnityEngine.Mathf.Abs(m_percentage/100f)),
			UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#")
		);
	}

	public override void Apply() {
		DragonHealthBehaviour healthBehaviour = InstanceManager.player.dragonHealthBehaviour;
		if (healthBehaviour)
			healthBehaviour.AddArmorModifier(m_percentage);
	}
}
