
public class ModDragonBoostUnlimited : ModifierDragon {
	public const string TARGET_CODE = "boost_unlimited";

    //------------------------------------------------------------------------//
    public ModDragonBoostUnlimited() {
        BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public ModDragonBoostUnlimited(DefinitionNode _def) : base(_def) {
		BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonBoostBehaviour boost = InstanceManager.player.dragonBoostBehaviour;
		if (boost)
			boost.modInfiniteBoost = true;
	}

    public override void Remove() {
        DragonBoostBehaviour boost = InstanceManager.player.dragonBoostBehaviour;
        if (boost)
            boost.modInfiniteBoost = false;
    }
}
