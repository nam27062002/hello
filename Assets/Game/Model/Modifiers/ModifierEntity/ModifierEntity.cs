
public abstract class ModifierEntity : Modifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string TYPE_CODE = "entity";


	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public static ModifierEntity CreateFromDefinition(DefinitionNode _def) {

		return null;
	}

	#endregion


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//



	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	public ModifierEntity(DefinitionNode _def) {
		base.Init(TYPE_CODE);

		m_def = _def;
	}

	public override void Remove() { }
}
