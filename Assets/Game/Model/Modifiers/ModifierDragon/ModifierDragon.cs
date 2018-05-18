
public abstract class ModifierDragon : Modifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "dragon";


	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public static ModifierDragon CreateFromDefinition(DefinitionNode _def) {

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
