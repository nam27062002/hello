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
		case ModDragonAirCurrent.TARGET_CODE:		return new ModDragonAirCurrent(_def);
		case ModDragonBoostRegen.TARGET_CODE: 		return new ModDragonBoostRegen(_def);
		case ModDragonBoostUnlimited.TARGET_CODE: 	return new ModDragonBoostUnlimited(_def);
		case ModDragonArmor.TARGET_CODE: 			return new ModDragonArmor(_def);
		case ModDragonFuryDuration.TARGET_CODE:		return new ModDragonFuryDuration(_def);
		case ModDragonFuryUnlimited.TARGET_CODE: 	return new ModDragonFuryUnlimited(_def);
		case ModDragonInvulnerable.TARGET_CODE:		return new ModDragonInvulnerable(_def);
		case ModDragonLifeDrain.TARGET_CODE:		return new ModDragonLifeDrain(_def);
		case ModDragonLifeGain.TARGET_CODE:			return new ModDragonLifeGain(_def);
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
