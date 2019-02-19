using System;
using System.Collections.Generic;

/// <summary>
/// Class container for types required by the Addressables system
/// </summary>
public class AddressablesTypes
{
    public enum ELocationType
    {
        None,
        Resources,
        AssetBundles,        
    };    

    private static List<string> ELocationTypeNames = new List<string>(Enum.GetNames(typeof(ELocationType)));
    
    public static string GetValidLocationTypeNamesAsString()
    {
        List<string> validNames = new List<string>();
        
        foreach (ELocationType value in Enum.GetValues(typeof(ELocationType)))
        {
            if (value != ELocationType.None)
            {
                validNames.Add(value.ToString());
            }
        }

        return UbiListUtils.GetListAsString(validNames);
    }

    public static ELocationType StringToELocationType(string value)
    {
        int index = ELocationTypeNames.IndexOf(value);
        return (index > -1) ? (ELocationType)index : ELocationType.None;
    }

    public static string ELocationTypeToString(ELocationType value)
    {
        int index = (int)value;
        if (index < 0 || index >= ELocationTypeNames.Count)
        {
            index = 0;
        }

        return ELocationTypeNames[index];
    }    

    /// <summary>
    /// Provider mode. There are different provider modes to use in different situations.
    /// </summary>
    public enum EProviderMode
    {
        Editor,         // All assets are provided by the editor regarless their locationType in catalog. It works only in editor. It's useful in development mode so assets are always available in the most convenient way
        AsCatalog      // locationType values in catalog are used to provide assets
    };   
}
