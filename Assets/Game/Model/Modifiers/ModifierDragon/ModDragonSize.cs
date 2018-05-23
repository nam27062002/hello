
public class ModDragonSize : ModifierDragon {
	public const string TARGET_CODE = "size";

	//------------------------------------------------------------------------//
	private float m_value; // scale 

	//------------------------------------------------------------------------//
	public ModDragonSize(DefinitionNode _def) : base(_def) {
		m_value = _def.GetAsFloat("param1");
	}

	public override void Apply() {
		DragonPlayer player = InstanceManager.player;
		if (player)
			player.OverrideSize(m_value);
	}
}
