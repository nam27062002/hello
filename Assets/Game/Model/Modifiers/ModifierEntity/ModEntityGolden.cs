
public class ModEntityGolden : ModifierEntity {
	public const string TARGET_CODE = "golden";

	//------------------------------------------------------------------------//
	public ModEntityGolden(DefinitionNode _def) : base(TARGET_CODE, _def) {
		BuildTextParams(UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
	}

	public override void Apply() {
		Entity.SetGoldenModifier(true);
	}

	public override void Remove() {
		Entity.SetGoldenModifier(false);
	}
}
