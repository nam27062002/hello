using SimpleJSON;
public class FeatureSettings
{
    public const string KEY_RATING = "rating";
    public const string KEY_PROFILE = "profile";    

    public float Rating { get; set; }
    public string Profile { get; set; }    

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

    protected virtual void ParseJSON(JSONNode json) {}

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

    protected virtual void ExtendedToJSON(SimpleJSON.JSONNode json) { }
}