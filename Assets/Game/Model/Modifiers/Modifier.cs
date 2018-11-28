

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
        string type = _data["custom"]["type"];

        switch (type) {
            case ModifierDragon.TYPE_CODE:      return ModifierDragon.CreateFromJson(_data["custom"]);
           /* case ModifierEntity.TYPE_CODE:      return ModifierEntity.CreateFromJson(_data);
            case ModifierGamePlay.TYPE_CODE:    return ModifierGamePlay.CreateFromJson(_data);
            case ModifierGatcha.TYPE_CODE:      return ModifierGatcha.CreateFromJson(_data);*/
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

    private readonly string m_sku = "";
    private readonly string m_uiCategory = "";
    private readonly string m_iconPath = "";
    private readonly string m_name = "";
    private string m_desc = "";
    private string m_descShort = "";

    private readonly string m_tidDesc = "";
    private readonly string m_tidDescShort = "";


    //------------------------------------------------------------------------//
    // INTERNAL METHODS														  //
    //------------------------------------------------------------------------//
    protected Modifier(string _type) {
        m_type = _type;
    }

    protected Modifier(string _type, DefinitionNode _def) {
        m_type = _type;

        m_sku           = _def.sku;
        m_uiCategory    = _def.Get("uiCategory");
        m_iconPath      = _def.Get("icon");
        m_name          = _def.GetLocalized("tidName");
        m_tidDesc       = _def.Get("tidDesc");
        m_tidDescShort  = _def.Get("tidDescShort");
    }

    protected Modifier(string _type, SimpleJSON.JSONNode _data) {
        m_type = _type;

        m_sku           = "custom";
        m_iconPath      = _data["icon"];
        m_tidDesc       = _data["tidDesc"];
        m_tidDescShort  = _data["tidDescShort"];
    }


    //------------------------------------------------------------------------//
    // ABSTRACT METHODS														  //
    //------------------------------------------------------------------------//
    public abstract void Apply();
	public abstract void Remove();

	protected void BuildTextParams(params string[] _params) {
		m_textParams = _params;

        m_desc = LocalizationManager.SharedInstance.Localize(m_tidDesc, m_textParams);
        m_descShort = LocalizationManager.SharedInstance.Localize(m_tidDescShort, m_textParams);
	}

    protected virtual SimpleJSON.JSONClass __ToJson() {
        SimpleJSON.JSONClass data = new SimpleJSON.JSONClass {
            { "type", m_type },
            { "target", m_target }
        };

        return data;
    }


    //------------------------------------------------------------------------//
    // INTERFACE METHODS: Definition										  //
    //------------------------------------------------------------------------//
    public string GetSku()              { return m_sku;         }
    public string GetUICategory()       { return m_uiCategory;  }
    public string GetIconRelativePath() { return m_iconPath;    }
    public string GetName()             { return m_name;        }
	public string GetDescription()      { return m_desc;        }
    public string GetDescriptionShort() { return m_descShort;	}

    public SimpleJSON.JSONClass ToJson() {
        SimpleJSON.JSONClass container = new SimpleJSON.JSONClass();

        SimpleJSON.JSONClass data = __ToJson();
        {
            data.Add("uiCategory", m_uiCategory);
            data.Add("icon", m_iconPath);
            data.Add("tidName", m_name);
            data.Add("tidDesc", m_tidDesc);
            data.Add("tidDescShort", m_tidDescShort);
        }
        container.Add(m_sku, data);

        return container;
    }
}
