using SimpleJSON;
public class FeatureSettings
{
    public const string KEY_RATING = "rating";

    public float Rating { get; set; }

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
        OverrideFromJSON(json); 
    }
    
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

            ExtendedOverrideFromJSON(json);
        }
    }

    protected virtual void ExtendedOverrideFromJSON(JSONNode json) { }

    public SimpleJSON.JSONNode ToJSON()
    {
        SimpleJSON.JSONClass returnValue = new SimpleJSON.JSONClass();
        returnValue.Add(KEY_RATING, Rating.ToString());
        ExtendedToJSON(returnValue);

        return returnValue;
    }

    protected virtual void ExtendedToJSON(SimpleJSON.JSONNode json) { }
}