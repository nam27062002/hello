﻿
public abstract class ModifierGatcha : Modifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "gatcha";


	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public new static ModifierGatcha CreateFromDefinition(DefinitionNode _def) {
		string target = _def.Get("target");

		switch (target) {
		case ModGatchaRarity.TARGET_CODE: 	return new ModGatchaRarity(_def);
		case ModGatchaPet.TARGET_CODE: 		return new ModGatchaPet(_def);
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
	public ModifierGatcha(DefinitionNode _def) {
		base.Init(TYPE_CODE);

		m_def = _def;
	}

}