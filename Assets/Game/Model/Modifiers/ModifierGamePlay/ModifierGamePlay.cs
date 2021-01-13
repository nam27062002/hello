
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
		case ModGamePlayKillChainFast.TARGET_CODE:	return new ModGamePlayKillChainFast(_def);
		case ModGamePlayKillChainLonger.TARGET_CODE:return new ModGamePlayKillChainLonger(_def);
		case ModGamePlayMissionReward.TARGET_CODE:	return new ModGamePlayMissionReward(_def);		
		case ModGamePlayPetPowerUp.TARGET_CODE:		return new ModGamePlayPetPowerUp(_def);
        case ModGamePlaySC.TARGET_CODE:             return new ModGamePlaySC(_def);
        case ModGamePlaySceneSelector.TARGET_CODE:  return new ModGamePlaySceneSelector(_def);
        case ModGamePlaySpawnFrequency.TARGET_CODE: return new ModGamePlaySpawnFrequency(_def);
		case ModGamePlayShader.TARGET_CODE: 		return new ModGamePlayShader(_def);
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
    protected ModifierGamePlay(string _starget, DefinitionNode _def) : base(TYPE_CODE, _starget, _def) { }

}
