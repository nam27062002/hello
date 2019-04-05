
public class ModDragonInvulnerable : ModifierDragon {
	public const string TARGET_CODE = "invincible";

    //------------------------------------------------------------------------//
    public ModDragonInvulnerable() : base(TARGET_CODE) {
        BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public ModDragonInvulnerable(DefinitionNode _def) : base(TARGET_CODE, _def) {
		BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonPlayer player = InstanceManager.player;
		if (player)
			player.modInvulnerable = true;
	}

    public override void Remove() {
        DragonPlayer player = InstanceManager.player;
        if (player)
            player.modInvulnerable = false;
    }
}
