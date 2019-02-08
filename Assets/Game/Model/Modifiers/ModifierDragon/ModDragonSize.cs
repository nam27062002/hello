
public class ModDragonSize : ModifierDragon {
	public const string TARGET_CODE = "size";

	//------------------------------------------------------------------------//
	private float m_scale; // scale 

	//------------------------------------------------------------------------//
	public ModDragonSize(DefinitionNode _def) : base(TARGET_CODE, _def) {
		m_scale = _def.GetAsFloat("param1");
		BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonPlayer player = InstanceManager.player;
		if (player)
			player.OverrideSize(m_scale);
	}
}
