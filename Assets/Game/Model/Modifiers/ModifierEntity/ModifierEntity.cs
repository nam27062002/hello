
public abstract class ModifierEntity : Modifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "entity";


	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public new static ModifierEntity CreateFromDefinition(DefinitionNode _def) {
		string target = _def.Get("target");

		switch (target) {
		case ModEntityXP.TARGET_CODE:		return new ModEntityXP(_def);
		case ModEntitySC.TARGET_CODE: 		return new ModEntitySC(_def);
		case ModEntityGolden.TARGET_CODE:	return new ModEntityGolden(_def);
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
    protected ModifierEntity(DefinitionNode _def) : base(TYPE_CODE, _def) {}

}
