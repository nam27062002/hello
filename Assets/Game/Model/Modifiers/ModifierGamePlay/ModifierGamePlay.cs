﻿
public abstract class ModifierGamePlay : Modifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "gameplay";


	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public new static ModifierGamePlay CreateFromDefinition(DefinitionNode _def) {
		string target = _def.Get("target");

		switch (target) {
		case ModGamePlaySpawnFrequency.TARGET_CODE:		return new ModGamePlaySpawnFrequency(_def);
		case ModGamePlayKillChainFast.TARGET_CODE:	return new ModGamePlayKillChainFast(_def);
		case ModGamePlayKillChainLonger.TARGET_CODE:return new ModGamePlayKillChainLonger(_def);
		case ModGamePlayMissionReward.TARGET_CODE:	return new ModGamePlayMissionReward(_def);
		case ModGamePlaySC.TARGET_CODE: 			return new ModGamePlaySC(_def);
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
	public ModifierGamePlay(DefinitionNode _def) {
		base.Init(TYPE_CODE);

		m_def = _def;
	}

}
