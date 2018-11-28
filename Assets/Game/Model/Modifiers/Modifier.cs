

public abstract class Modifier : IModifierDefinition {
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
    		case ModifierGatcha.TYPE_CODE:		return ModifierGatcha.CreateFromDefinition(_def);
		}

		return null;
	}

    public static Modifier CreateFromJson(SimpleJSON.JSONNode _data) {
        string type = _data["type"];

        switch (type) {
            case ModifierDragon.TYPE_CODE:      return ModifierDragon.CreateFromJson(_data);
            case ModifierEntity.TYPE_CODE:      return ModifierEntity.CreateFromJson(_data);
            case ModifierGamePlay.TYPE_CODE:    return ModifierGamePlay.CreateFromJson(_data);
            case ModifierGatcha.TYPE_CODE:      return ModifierGatcha.CreateFromJson(_data);
        }

        return null;
    }

	#endregion


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private string[] m_textParams = null;


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
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	public abstract void Apply();
	public abstract void Remove();

	protected void BuildTextParams(params string[] _params) {
		m_textParams = _params;
	}

	//------------------------------------------------------------------------//
	// INTERFACE METHODS: Definition										  //
	//------------------------------------------------------------------------//
	public string GetIconRelativePath() {
		return m_def.GetAsString("icon");
	}

	public string GetDescription() {
		return LocalizationManager.SharedInstance.Localize(m_def.GetAsString("tidDesc"), m_textParams);
	}

	public string GetDescriptionShort() {
		return LocalizationManager.SharedInstance.Localize(m_def.GetAsString("tidDescShort"), m_textParams);
	}
}
