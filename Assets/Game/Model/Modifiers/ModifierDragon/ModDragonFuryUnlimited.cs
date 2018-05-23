
public class ModDragonFuryUnlimited : ModifierDragon {
	public const string TARGET_CODE = "fury_unlimited";

	//------------------------------------------------------------------------//
	public ModDragonFuryUnlimited(DefinitionNode _def) : base(_def) { }

	public override void Apply() {
		DragonBreathBehaviour breath = InstanceManager.player.breathBehaviour;
		if (breath) {
			breath.AddFury(breath.furyMax);
			breath.modInfiniteFury = true;
		}
	}
}
