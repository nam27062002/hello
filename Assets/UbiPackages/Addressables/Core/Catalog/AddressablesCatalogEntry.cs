using SimpleJSON;

/// <summary>
/// This class is responsible for storing the information related to a single addressable asset that belongs to the addressables catalog
/// </summary>
public class AddressablesCatalogEntry
{
    public static Logger sm_logger;

    /// <summary>
    /// Identifier of this addressable asset. It must be unique.
    /// </summary>
    private string m_id;
    public string Id
    {
        get { return m_id; }
        private set { m_id = value; }
    }

    /// <summary>
    /// Variant of an addressable. This is used to be able to have more than one version of the same asset. It's typically used to offer different quality levels of the same asset
    /// </summary>
    private string m_variant;
    public string Variant
    {
        get { return m_variant; }
        private set { m_variant = value; }
    }

    /// <summary>
    /// Location type. Used to determine where the asset is stored.
    /// </summary>
    private AddressablesTypes.ELocationType m_locationType;
    public AddressablesTypes.ELocationType LocationType
    {
        get { return m_locationType; }
        set { m_locationType = value; }
    }

    private string m_assetBundleName;
    public string AssetBundleName
    {
        get { return m_assetBundleName; }
        set { m_assetBundleName = value; }
    }
    
    public string AssetName { get; set; }       

    private const string ATT_ID = "id";
    private const string ATT_VARIANT = "variant";
    private const string ATT_LOCATION_TYPE = "locationType";        
    private const string ATT_AB_NAME = "abName";
    private const string ATT_ASSET_NAME = "assetName";

#if UNITY_EDITOR
    private const string ATT_GUID = "guid";
    private const string ATT_PLATFORM = "platform";

    private string m_guid;
    public string GUID
    {
        get { return m_guid; }
        private set { m_guid = value; }
    }

    private string m_platform;
    public string Platform
    {
        get { return m_platform; }
        set { m_platform = value; }
    }

    public bool IsAvailableForPlatform(UnityEditor.BuildTarget target)
    {
        return m_platform == null || m_platform == target.ToString();
    }

    private bool m_editorMode;

    public AddressablesCatalogEntry(bool editorMode) : this()
    {
        m_editorMode = editorMode;
    }

    public AddressablesCatalogEntry(string id, string variant, string gui, bool editorMode) : this() {
        Id = id;
        Variant = variant;
        GUID = gui;
        m_editorMode = editorMode;        
    }
#endif

    public AddressablesCatalogEntry()
    {
    }

    public void Reset()
    {
        Id = null;
        Variant = null;
        LocationType = AddressablesTypes.ELocationType.None;
        AssetBundleName = null;
        AssetName = null;

#if UNITY_EDITOR
        GUID = null;
#endif
    }

    /// <summary>
    /// Loads an entry from JSON
    /// </summary>
    /// <param name="data">JSON to load into this entry</param>
    /// <returns></returns>
    public void Load(JSONNode data)
    {
        if (data == null)
        {
            LogLoadError("A null json can't be loaded into a catalog entry");
        }
        else
        {
            // Id
            string att = ATT_ID;
            string value = data[att];
            Id = value;

            if (string.IsNullOrEmpty(value))
            {
                LogLoadAttributeError(att, value);
            }

            // Variant
            att = ATT_VARIANT;
            value = data[att];
            Variant = value;

            // Location type
            att = ATT_LOCATION_TYPE;
            value = data[att];
            LocationType = AddressablesTypes.StringToELocationType(value);
            if (sm_logger != null)
            {
                if (LocationType == AddressablesTypes.ELocationType.None)
                {
                    LogLoadAttributeError(att, value, AddressablesTypes.GetValidLocationTypeNamesAsString());
                }
            }

#if UNITY_EDITOR
            // GUID
            att = ATT_GUID;
            GUID = data[att];

            if (string.IsNullOrEmpty(GUID) && m_editorMode)
            {
                LogLoadAttributeError(att, value);
            }

            att = ATT_PLATFORM;
            Platform = data[att];
#endif

            // Asset Bundle name            
            att = ATT_AB_NAME;
            AssetBundleName = data[att];

            if (string.IsNullOrEmpty(AssetBundleName) && LocationType == AddressablesTypes.ELocationType.AssetBundles)
            {
                LogLoadAttributeError(att, value);
            }            

            att = ATT_ASSET_NAME;
            AssetName = data[att];

#if UNITY_EDITOR
            if (m_editorMode && AddressablesManager.EffectiveMode == AddressablesManager.EMode.AllInResources)
            {
                SetupAsEntryInResources(Id);
            }
#endif
        }
    }   

    public void SetupAsEntryInResources(string id)
    {
        Id = id;        
        AssetName = id;
        LocationType = AddressablesTypes.ELocationType.Resources;
    }

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();
        
        AddToJSON(data, ATT_ID, Id);

        if (!string.IsNullOrEmpty(Variant))
        {
            AddToJSON(data, ATT_VARIANT, Variant);
        }
        
        AddToJSON(data, ATT_LOCATION_TYPE, AddressablesTypes.ELocationTypeToString(LocationType));

        bool needsToAddAssetBundleName = LocationType == AddressablesTypes.ELocationType.AssetBundles;
#if UNITY_EDITOR
        if (m_editorMode)
        {
            AddToJSON(data, ATT_GUID, GUID);            
            needsToAddAssetBundleName = true;

            if (!string.IsNullOrEmpty(Platform))
            {
                AddToJSON(data, ATT_PLATFORM, Platform);
            }
        }
#endif
        if (needsToAddAssetBundleName)
        {
            AddToJSON(data, ATT_AB_NAME, AssetBundleName);
        }

        AddToJSON(data, ATT_ASSET_NAME, AssetName);

        return data;
    }
    
    private void AddToJSON(JSONClass data, string attId, string value)
    {
        if (data != null && !string.IsNullOrEmpty(attId) && value != null)
        {
            data[attId] = value;
        }
    }    

    public bool IsValid()
    {        
        return !string.IsNullOrEmpty(Id) && IsLocationValid();
    }
    
    private bool IsLocationValid()
    {
        bool returnValue = true;
        switch (LocationType)
        {
            case AddressablesTypes.ELocationType.None:
                returnValue = false;
                break;

            case AddressablesTypes.ELocationType.Resources:
                returnValue = !string.IsNullOrEmpty(AssetName);
#if UNITY_EDITOR
                if (m_editorMode)
                {
                    returnValue = true;
                }
#endif
                break;

            case AddressablesTypes.ELocationType.AssetBundles:
                returnValue = !string.IsNullOrEmpty(AssetBundleName);
                break;
        }

        return returnValue;
    }  

#region log
    private void LogLoadAttributeError(string att, string value, string validValues=null)
    {
        if (sm_logger != null)
        {
            string msg = "<" + value + "> is an invalid value for attribute <" + att + ">";
            if (!string.IsNullOrEmpty(validValues))
            {
                msg += ". Choose among [" + validValues + "]";
            }
            LogLoadError(msg);
        }
    }

    private void LogLoadError(string msg)
    {
        if (sm_logger != null)
        {
            string thisMsg = "Error loading entry";
            if (!string.IsNullOrEmpty(Id))
            {
                thisMsg += " <" + Id + ">";
            }

            thisMsg += ": " + msg;
            sm_logger.LogError(thisMsg);
        }
    }
#endregion
}
