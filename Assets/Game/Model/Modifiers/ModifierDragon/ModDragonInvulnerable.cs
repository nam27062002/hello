
public class ModDragonInvulnerable : ModifierDragon {
	public const string TARGET_CODE = "invincible";

	//------------------------------------------------------------------------//
	public ModDragonInvulnerable(DefinitionNode _def) : base(_def) { }

	public override void Apply() {
		DragonPlayer player = InstanceManager.player;
		if (player)
			player.modInvulnerable = true;
	}
}
