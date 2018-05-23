﻿
public class ModEntityGolden : ModifierEntity {
	public const string TARGET_CODE = "golden";

	//------------------------------------------------------------------------//
	public ModEntityGolden(DefinitionNode _def) : base(_def) { }

	public override void Apply() {
		Entity.SetGoldenModifier(true);
	}

	public override void Remove() {
		Entity.SetGoldenModifier(false);
	}
}
