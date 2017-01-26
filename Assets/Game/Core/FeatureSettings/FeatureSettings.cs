using SimpleJSON;
using System.Collections.Generic;
public class FeatureSettings
{
    public const string KEY_RATING = "rating";
    public const string KEY_PROFILE = "profile";

    public static List<string> Keys_Supported;

    private static void Build()
    {
        if (Keys_Supported == null)
        {
            Keys_Supported = new List<string>();
            Keys_AddKey(KEY_RATING);
            Keys_AddKey(KEY_PROFILE);
        }
    }

    public static void Keys_AddKey(string key)
    {
        if (Keys_Supported == null)
        {
            Keys_Supported = new List<string>();
        }

        if (Keys_Supported.Contains(key))
        {
            LogError("Key " + key + " duplicated");
        }
        else
        {
            Keys_Supported.Add(key);
        }        
    }

    public static bool Keys_HasKey(string key)
    {
        return (Keys_Supported != null && Keys_Supported.Contains(key));
    }

    public float Rating { get; set; }
    public string Profile { get; set; }    

    public FeatureSettings()
    {
        Build();
    }

    public void Reset()
    {
        Rating = 0f;
        ExtendedReset();
    }

    protected virtual void ExtendedReset() { }

    /// <summary>
    /// Loads this object with the data defined by <c>json</c>. Default values are set for the keys that are not defined in <c>json</c>.
    /// </summary>    
    public void FromJSON(JSONNode json)
    {        
        Reset();
        ParseJSON(json);    
        OverrideFromJSON(json); 
    }

    protected void ParseJSON(JSONNode json)
    {
        if (json != null)
        {
            JSONClass jsonClass = json as JSONClass;
            foreach (KeyValuePair<string, JSONNode> pair in jsonClass.m_Dict)
            {
                if (Keys_HasKey(pair.Key))
                {
                    ParseValue(pair.Key, pair.Value);                    
                }
                else
                {
                    LogError("Key " + pair.Key + " not valid");
                }                
            }
        }
    }

    protected virtual void ParseValue(string key, string valueAsString) {}

    /// <summary>
    /// Overrides the current data of this object with the data defined in <c>json</c>. Only the keys defined in <c>json</c> are overriden.
    /// </summary>    
    public void OverrideFromJSON(JSONNode json)
    {
        if (json != null)
        {
            string key = KEY_RATING;
            if (json.ContainsKey(key))
            {
                Rating = json[key].AsFloat;
            }

            key = KEY_PROFILE;
            if (json.ContainsKey(key))
            {
                Profile = json[key];
            }           

            ExtendedOverrideFromJSON(json);
        }
    }

    protected virtual void ExtendedOverrideFromJSON(JSONNode json) { }

    public SimpleJSON.JSONNode ToJSON()
    {
        SimpleJSON.JSONClass returnValue = new SimpleJSON.JSONClass();
        returnValue.Add(KEY_RATING, Rating.ToString());
        returnValue.Add(KEY_PROFILE, Rating.ToString());
        ExtendedToJSON(returnValue);

        return returnValue;
    }

    protected virtual void ExtendedToJSON(SimpleJSON.JSONNode json) {}

    #region debug
    private const string DEBUG_CHANNEL = "[FeatureSettings] ";
    public static void Log(string msg)
    {
        Debug.Log(DEBUG_CHANNEL + msg);
    }

    public static void LogWarning(string msg)
    {
        Debug.LogWarning(DEBUG_CHANNEL + msg);
    }

    public static void LogError(string msg)
    {
        Debug.LogError(DEBUG_CHANNEL + msg);
    }
    #endregion
}