public class FeatureSettings
{
    private const string RATING_KEY = "rating";

    public float Rating { get; set; }

    private void Reset()
    {
        Rating = 0f;
    }

    protected virtual void ExtendedReset() { }

    public void FromJSON(SimpleJSON.JSONNode json)
    {
        Reset();

        string key = RATING_KEY;
        if (json.ContainsKey(key))
        {
            Rating = json[key].AsFloat;
        }

        ExtendedFromJSON(json);
    }

    protected virtual void ExtendedFromJSON(SimpleJSON.JSONNode json) { }

    public SimpleJSON.JSONNode ToJSON()
    {
        SimpleJSON.JSONClass returnValue = new SimpleJSON.JSONClass();
        returnValue.Add(RATING_KEY, Rating.ToString());
        ExtendedToJSON(returnValue);

        return returnValue;
    }

    protected virtual void ExtendedToJSON(SimpleJSON.JSONNode json) { }
}