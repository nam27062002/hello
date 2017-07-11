// DeviceQualityManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using SimpleJSON;
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

    private class ProfileData
    {
        public float Rating { get; set; }

        // Min memory in bytes required to run this profile
        public int MinMemory { get; set; }
        public JSONNode Json { get; set; }

        public ProfileData(float rating, int minMemory, JSONNode json)
        {
            Rating = rating;
            MinMemory = minMemory;
            Json = json;
        }
    }

    /// <summary>
    /// List of the profile names sorted in ascending order for rating in order to ease the searches of profile per rating
    /// </summary>
    public List<string> Profiles_Names { get; set; }

    /// <summary>
    /// Key: Profile name
    /// Value: Configuration (set of feature settings) for that profile
    /// </summary>
    private Dictionary<string, ProfileData> Profiles_Data { get; set; }

    /// <summary>
    /// Min memory in bytes required to run the game
    /// </summary>
    private int Profiles_MinMemory { get; set; }

    public void Profiles_Clear()
    {
        if (Profiles_Names != null)
        {
            Profiles_Names.Clear();
        }

        if (Profiles_Data != null)
        {
            Profiles_Data.Clear();
        }

        Profiles_MinMemory = int.MaxValue;
    }    

    public void Profiles_AddData(string profileName, float rating, int minMemory, JSONNode settings)
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

            if (Profiles_Data == null)
            {
                Profiles_Data = new Dictionary<string, ProfileData>();
            }

            ProfileData profileData = new ProfileData(rating, minMemory, settings);
            Profiles_Data.Add(profileName, profileData);

            // Makes sure that the profiles are sorted which is important to be able to determine the profile for a rating given (Profiles_RatingToProfileName())
            Profiles_Names.Sort(Profiles_Sort);

            if (minMemory < Profiles_MinMemory)
            {
                Profiles_MinMemory = minMemory;
            }
        }
    }

    private int Profiles_Sort(string p1, string p2)
    {
        int returnValue = 0;

        float rating1 = (Profiles_Data.ContainsKey(p1)) ? Profiles_Data[p1].Rating : 0f;
        float rating2 = (Profiles_Data.ContainsKey(p2)) ? Profiles_Data[p2].Rating : 0f;
        
        if (rating1 > rating2)
        {
            returnValue = 1;
        }
        else if (rating1 < rating2)
        {
            returnValue = -1;
        }

        return returnValue;
    }

    public JSONNode Profiles_GetDataAsJSON(string profileName)
    {
        JSONNode returnValue = null;
        if (Profiles_Data != null && Profiles_Data.ContainsKey(profileName))
        {
            returnValue = Profiles_Data[profileName].Json;
        }

        return returnValue;
    }

    /// <summary>
    /// Returns the name of the profile that corresponds to the device rating passed as a parameter    
    /// </summary>
    /// <param name="rating"></param>
    /// <param name="memorySize">memory size in bytes</param>
    /// <returns></returns>
    public string Profiles_RatingToProfileName(float rating, int memorySize)
    {
        if (memorySize < Profiles_MinMemory)
        {
            LogWarning("memory Size " + memorySize + " is lower than the minimum memory required by the game (" + Profiles_MinMemory + ")");

            // Memory size is forced to min memory. Some devices have a few bytes less than 1GB so we try our luck
            memorySize = 1024;
        }

        string returnValue = null;
        if (Profiles_Names != null)
        {            
            int i;
            int count = Profiles_Names.Count;
            
            // Loops through all profiles, which are sorted in ascending order per rating, until one with bigger rating than the passed as an argument is found
            for (i = 0; i < count && Profiles_Data[Profiles_Names[i]].Rating < rating; i++)
            {
                // Makes sure that it has memory and rating enough to use this profile
                if (memorySize >= Profiles_Data[Profiles_Names[i]].MinMemory && Profiles_Data[Profiles_Names[i]].Rating < rating)
                {
                    returnValue = Profiles_Names[i];
                }                
            }                                   
        }

        return returnValue;
    }       
    #endregion

    #region device
    private void Device_Clear()
    {
        Device_CalculatedRating = 0f;
    }

    public float Device_CalculatedRating { get; set; }    
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
