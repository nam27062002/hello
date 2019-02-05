using SimpleJSON;
using System.Collections.Generic;

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
    /// Location type. Used to determine where the asset is stored.
    /// </summary>
    private AddressablesTypes.ELocationType m_locationType;
    public AddressablesTypes.ELocationType LocationType
    {
        get { return m_locationType; }
        set { m_locationType = value; }
    }

    /// <summary>
    /// Location type. Used to determine where the asset is stored.
    /// </summary>
    private string m_path;
    public string Path
    {
        get { return m_path; }
        private set { m_path = value; }
    }

    private string m_assetBundleName;
    public string AssetBundleName
    {
        get { return m_assetBundleName; }
        set { m_assetBundleName = value; }
    }

    private const string ATT_ID = "id";
    private const string ATT_LOCATION_TYPE = "locationType";    
    private const string ATT_PATH = "path";
    private const string ATT_AB_NAME = "abName";

    public void Reset()
    {
        Id = null;
        LocationType = AddressablesTypes.ELocationType.None;
        Path = null;       
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

            // Path
            att = ATT_PATH;
            Path = data[att];

            if (string.IsNullOrEmpty(Path))
            {
                LogLoadAttributeError(att, value);
            }

            // Asset Bundle name
            att = ATT_AB_NAME;
            AssetBundleName = data[att];

            if (string.IsNullOrEmpty(AssetBundleName) && LocationType == AddressablesTypes.ELocationType.AssetBundles)
            {
                LogLoadAttributeError(att, value);
            }
        }
    }

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();

        if (Id != null)

        AddToJSON(data, ATT_ID, Id);
        AddToJSON(data, ATT_LOCATION_TYPE, AddressablesTypes.ELocationTypeToString(LocationType));
        AddToJSON(data, ATT_PATH, Path);
        AddToJSON(data, ATT_AB_NAME, AssetBundleName);

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
                returnValue = !string.IsNullOrEmpty(Path);
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
