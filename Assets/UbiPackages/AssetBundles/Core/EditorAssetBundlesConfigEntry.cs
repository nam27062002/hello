#if UNITY_EDITOR
using SimpleJSON;
using System;
using System.Collections.Generic;

public class EditorAssetBundlesConfigEntry
{
    private static string ATT_ID = "id";
    private static string ATT_LOCATION = "location";

    public string Id { get; set; }

    public enum ELocation
    {
        Local,
        Remote,
        Resources
    };

    private static List<string> LOCATION_NAMES = new List<string>(Enum.GetNames(typeof(ELocation)));

    public ELocation Location { get; set; }

    public EditorAssetBundlesConfigEntry()
    {
        Reset();
    }

    private void Reset()
    {
        Id = null;
        Location = ELocation.Remote;
    }

    public void Load(JSONNode data, Logger logger)
    {
        Reset();

        if (data == null)
        {
            LogLoadError(logger, "A null json can't be loaded into a config entry");
        }
        else
        {
            // Id
            string att = ATT_ID;
            string value = data[att];
            Id = value;

            if (string.IsNullOrEmpty(value))
            {
                LogLoadAttributeError(logger, att, value);
            }

            att = ATT_LOCATION;
            if (data.ContainsKey(att))
            {
                value = data[att];
                int index = LOCATION_NAMES.IndexOf(value);
                if (index > -1)
                {
                    Location = (ELocation)index;
                }
                else
                {
                    LogLoadAttributeError(logger, att, value);
                }
            }
            else
            {
                // By default we assume that an asset bundle is remote
                Location = ELocation.Remote;
            }           
        }
    }

    public void SetAddressablesMode(AddressablesManager.EMode mode)
    {
        // Location
        switch (mode)
        {
            case AddressablesManager.EMode.AllInLocalAssetBundles:
                if (Location == ELocation.Remote)
                {
                    Location = ELocation.Local;
                }
                break;

            case AddressablesManager.EMode.AllInResources:
                Location = ELocation.Resources;
                break;

            case AddressablesManager.EMode.LocalAssetBundlesInResources:
                if (Location == ELocation.Local)
                {
                    Location = ELocation.Resources;
                }
                break;
        }
    }

    public JSONClass ToJSON()
    {
        JSONClass data = new JSONClass();

        AddToJSON(data, ATT_ID, Id);        
        AddToJSON(data, ATT_LOCATION, LOCATION_NAMES[(int)Location]);        

        return data;
    }

    private void AddToJSON(JSONClass data, string attId, string value)
    {
        if (data != null && !string.IsNullOrEmpty(attId) && value != null)
        {
            data[attId] = value;
        }
    }

    #region log
    private void LogLoadAttributeError(Logger logger, string att, string value, string validValues = null)
    {
        if (logger != null)
        {
            string msg = "<" + value + "> is an invalid value for attribute <" + att + ">";
            if (!string.IsNullOrEmpty(validValues))
            {
                msg += ". Choose among [" + validValues + "]";
            }
            LogLoadError(logger, msg);
        }
    }

    private void LogLoadError(Logger logger, string msg)
    {
        if (logger != null)
        {
            string thisMsg = "Error loading entry";
            if (!string.IsNullOrEmpty(Id))
            {
                thisMsg += " <" + Id + ">";
            }

            thisMsg += ": " + msg;
            logger.LogError(thisMsg);
        }
    }
    #endregion
}
#endif