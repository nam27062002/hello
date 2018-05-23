
public class ModDragonFuryAlways : ModifierDragon {
	public const string TARGET_CODE = "fury_unlimited";

	//------------------------------------------------------------------------//
	public ModDragonFuryAlways(DefinitionNode _def) : base(_def) { }

	public override void Apply() {
		DragonBreathBehaviour breath = InstanceManager.player.breathBehaviour;
		if (breath) {
			breath.AddFury(breath.furyMax);
			breath.modInfiniteFury = true;
		}
	}
}
