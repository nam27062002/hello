

public abstract class Modifier  {
	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	#region Factory

	public static Modifier CreateFromDefinition(DefinitionNode _def) {
		string type = _def.Get("type");

		switch (type) {
		case ModifierDragon.TYPE_CODE: 		return ModifierDragon.CreateFromDefinition(_def);
		case ModifierEntity.TYPE_CODE: 		return ModifierEntity.CreateFromDefinition(_def);
		case ModifierGamePlay.TYPE_CODE: 	return ModifierGamePlay.CreateFromDefinition(_def);
		}

		return null;
	}

	#endregion


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	protected string m_type = "";
	public string type { get { return m_type; } }

	protected string m_target = "";
	public string target { get { return m_target; } }


	protected DefinitionNode m_def;
	public DefinitionNode def { get { return m_def; } }


	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	protected void Init(string _type) {
		m_type = _type;
	}


	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	public abstract void Apply();
	public abstract void Remove();
}
