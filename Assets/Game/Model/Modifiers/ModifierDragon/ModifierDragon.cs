﻿
public abstract class ModifierDragon : Modifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "dragon";


	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public new static ModifierDragon CreateFromDefinition(DefinitionNode _def) {
		string target = _def.Get("target");

		switch (target) {
		case ModDragonBoostRegen.TARGET_CODE: 		return new ModDragonBoostRegen(_def);
		case ModDragonBoostUnlimited.TARGET_CODE: 	return new ModDragonBoostUnlimited(_def);
		case ModDragonDamage.TARGET_CODE: 			return new ModDragonDamage(_def);
		case ModDragonFuryAlways.TARGET_CODE: 		return new ModDragonFuryAlways(_def);
		case ModDragonLifeDrain.TARGET_CODE:		return new ModDragonLifeDrain(_def);
		case ModDragonSize.TARGET_CODE: 			return new ModDragonSize(_def);
		case ModDragonSpeedMax.TARGET_CODE:			return new ModDragonSpeedMax(_def);
		}

		return null;
	}

	#endregion


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//



	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	public ModifierDragon(DefinitionNode _def) {
		base.Init(TYPE_CODE);

		m_def = _def;
	}

	public override void Remove() { }
}
