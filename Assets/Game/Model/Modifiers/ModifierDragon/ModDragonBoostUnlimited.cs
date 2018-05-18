
public class ModDragonBoostUnlimited : ModifierDragon {
	//------------------------------------------------------------------------//
	public ModDragonBoostUnlimited(DefinitionNode _def) : base(_def) { }

	public override void Apply() {
		DragonBoostBehaviour boost = InstanceManager.player.dragonBoostBehaviour;
		if (boost)
			boost.modInfiniteBoost = true;
	}
}
