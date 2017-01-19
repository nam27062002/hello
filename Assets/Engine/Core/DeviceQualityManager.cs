// DeviceQualityManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Singleton to manage the quality rating of the device.
/// The device quality rating is calculated when the application starts taking into consideration its hardware. The rating also can be set manually. This rating is translated into a quality profile. 
/// Quality profiles are used to define the values for some parameters that configure how some features perform.
/// </summary>
public class DeviceQualityManager
{        
    public DeviceQualityManager()
    {
        Clear();
    }

    public void Clear()
    {
        Profiles_Clear();
        Device_Clear();
    }

    #region profiles
    // This region is responsible for loading and handling the different quality profiles

    public List<string> Profiles_Names { get; set; }
    private Dictionary<string, FeatureSettings> Profiles_FeatureSettings { get; set; }

    public void Profiles_Clear()
    {
        if (Profiles_Names != null)
        {
            Profiles_Names.Clear();
        }

        if (Profiles_FeatureSettings != null)
        {
            Profiles_FeatureSettings.Clear();
        }
    }    

    public void Profiles_AddFeatureSettings(string profileName, FeatureSettings settings)
    {
        if (Profiles_Names == null)
        {
            Profiles_Names = new List<string>();
        }

        if (Profiles_Names.Contains(profileName))
        {
            LogError("Profile " + profileName + " has already been added");
        }
        else
        {
            Profiles_Names.Add(profileName);

            if (Profiles_FeatureSettings == null)
            {
                Profiles_FeatureSettings = new Dictionary<string, FeatureSettings>();
            }
            Profiles_FeatureSettings.Add(profileName, settings);
        }
    }

    public FeatureSettings Profiles_GetFeatureSettings(string profileName)
    {
        FeatureSettings returnValue = null;
        if (Profiles_FeatureSettings != null && Profiles_FeatureSettings.ContainsKey(profileName))
        {
            returnValue = Profiles_FeatureSettings[profileName];
        }

        return returnValue;
    }

    /// <summary>
    /// Returns the name of the profile that corresponds to the device rating passed as a parameter
    /// </summary>    
    public string Profiles_RatingToProfileName(float rating)
    {
        string returnValue = null;
        if (Profiles_Names != null)
        {
            int i;
            int count = Profiles_Names.Count;
            for (i = 0; i < count && Profiles_FeatureSettings[Profiles_Names[i]].Rating <= rating; i++) ; // Empty
            if (i > 0)
            {
                returnValue = Profiles_Names[i-1];
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Returns the <c>FeatureSettings</c> object of the profile that corresponds to the device rating passed as a parameter.
    /// </summary>    
    public FeatureSettings Profiles_GetFeatureSettingsPerRating(float rating)
    {
        string profileName = Profiles_RatingToProfileName(rating);
        return (profileName == null) ? null : Profiles_FeatureSettings[profileName];        
    }
    #endregion

    #region device
    private void Device_Clear()
    {
        Device_CalculatedRating = 0f;
    }

    public float Device_CalculatedRating { get; set; }

    public FeatureSettings Device_CurrentFeatureSettings { get; set; }
    
    #endregion

    #region log
    private const string PREFIX = "DeviceQualityManager:";

    public static void Log(string message)
    {
        Debug.Log(PREFIX + message);
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning(PREFIX + message);
    }

    public static void LogError(string message)
    {
        Debug.LogError(PREFIX + message);
    }
    #endregion
}
