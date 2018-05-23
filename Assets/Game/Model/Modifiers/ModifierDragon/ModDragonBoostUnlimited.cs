
public class ModDragonBoostUnlimited : ModifierDragon {
	public const string TARGET_CODE = "boost_unlimited";

	//------------------------------------------------------------------------//
	public ModDragonBoostUnlimited(DefinitionNode _def) : base(_def) { }

	public override void Apply() {
		DragonBoostBehaviour boost = InstanceManager.player.dragonBoostBehaviour;
		if (boost)
			boost.modInfiniteBoost = true;
	}
}
