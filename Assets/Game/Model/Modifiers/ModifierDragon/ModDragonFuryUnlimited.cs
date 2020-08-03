
public class ModDragonFuryUnlimited : ModifierDragon {
	public const string TARGET_CODE = "fury_unlimited";

	//------------------------------------------------------------------------//
	public ModDragonFuryUnlimited(DefinitionNode _def) : base(TARGET_CODE, _def) {
		BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		DragonBreathBehaviour breath = InstanceManager.player.breathBehaviour;
		if (breath) {
			breath.AddFury(breath.furyMax);
			breath.modInfiniteFury = true;
		}
	}
}
