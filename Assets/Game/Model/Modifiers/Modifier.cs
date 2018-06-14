

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

	#endregion


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	protected UnityEngine.Color m_textColor;
	protected string m_textParam;


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
		m_textColor = UIConstants.PET_CATEGORY_DEFAULT;
		m_textParam = "";
	}


	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	public abstract void Apply();
	public abstract void Remove();


	//------------------------------------------------------------------------//
	// INTERFACE METHODS: Definition										  //
	//------------------------------------------------------------------------//
	public string GetIconRelativePath() {
		return m_def.GetAsString("icon");
	}

	public string GetDescription() {
		return LocalizationManager.SharedInstance.Localize(m_def.GetAsString("tidDesc"), m_textParam, m_textColor.ToHexString("#"));
	}

	public string GetDescriptionShort() {
		return LocalizationManager.SharedInstance.Localize(m_def.GetAsString("tidDescShort"), m_textParam, m_textColor.ToHexString("#"));
	}
}
