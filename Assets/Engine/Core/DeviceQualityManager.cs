// DeviceQualityManager.cs
// Hungry Dragon
// 
// Created by David Germade
// Copyright (c) 2016 Ubisoft. All rights reserved.

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using SimpleJSON;
using System.Collections.Generic;
using System.Diagnostics;

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
        public int GfxMemory { get; set; }
        public JSONNode Json { get; set; }

        public ProfileData(float rating, int minMemory, int gfxMemory, JSONNode json)
        {
            Rating = rating;
            MinMemory = minMemory;
            GfxMemory = gfxMemory;
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

    /// <summary>
    /// Min gfx memory in bytes required to run the game
    /// </summary>
    private int Profiles_GfxMemory { get; set; }


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

        Profiles_GfxMemory = Profiles_MinMemory = int.MaxValue;
    }    

    public void Profiles_AddData(string profileName, float rating, int minMemory, int gfxMemory, JSONNode settings)
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

            ProfileData profileData = new ProfileData(rating, minMemory, gfxMemory, settings);
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
    public string Profiles_RatingToProfileName(float rating, int memorySize, int gfxMemorySize)
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

            if (count > 0)
            {
                returnValue = Profiles_Names[0];
                // Loops through all profiles, which are sorted in ascending order per rating, until one with bigger rating than the passed as an argument is found
                for (i = 0; i < count && Profiles_Data[Profiles_Names[i]].Rating <= rating; i++)
                {
                    // Makes sure that it has memory and rating enough to use this profile
                    // gfxMemorySize is used only for Android in order to have an idea about how old the device is. This is not used for iOS because profiles is set manually in this platform
#if UNITY_ANDROID
                    int gfxMemoryLimit = (memorySize > (1024 * 3)) ? 0 : Profiles_Data[Profiles_Names[i]].GfxMemory;
                    if (gfxMemorySize >= gfxMemoryLimit && memorySize >= Profiles_Data[Profiles_Names[i]].MinMemory && Profiles_Data[Profiles_Names[i]].Rating <= rating)
#else
                    if (memorySize >= Profiles_Data[Profiles_Names[i]].MinMemory && Profiles_Data[Profiles_Names[i]].Rating <= rating)
#endif
                    {
                        returnValue = Profiles_Names[i];
                    }
                }
            }
        }

        if (returnValue == null)
        {
            LogWarning("No profile available");
        }

        return returnValue;
    }  

    public float Profiles_ProfileNameToRating(string profileName)
    {
        float returnValue = -1f;
        if (Profiles_Data != null && Profiles_Data.ContainsKey(profileName))
        {
            returnValue = Profiles_Data[profileName].Rating;            
        }

        if (returnValue < 0f)
        {
            LogWarning("No profile " + profileName + " found");
        }

        return returnValue;
    }
    
    public int Profiles_GetMaxProfileLevel(int memorySize)
    {
        int returnValue = -1;

        // Max profile allowed depends on memory size
        if (Profiles_Names != null)
        {
            int i;
            int count = Profiles_Names.Count;

            // Loops through all profiles, which are sorted in ascending order per rating, until one with bigger rating than the passed as an argument is found
            for (i = 0; i < count; i++)
            {
                // Makes sure that it has memory and rating enough to use this profile
                if (memorySize >= Profiles_Data[Profiles_Names[i]].MinMemory)
                {
                    returnValue = i;
                }
            }
        }

        if (returnValue == -1)
        {
            // The minimum is returned
            returnValue = 0;            
            LogWarning("No profile available for memory " + memorySize);
        }

	    Log (">>>>>>>>GetMaxProfileLevel: " + returnValue);

        return returnValue;
    }     

    /// <summary>
    /// Returns the minimum amount of memory in megabytes required to run the game
    /// </summary>
    /// <returns></returns>
    public int Profiles_GetMinMemoryRequired()
    {
        int returnValue = int.MaxValue;

        // Max profile allowed depends on memory size
        if (Profiles_Names != null)
        {
            int i;
            int count = Profiles_Names.Count;

            // Loops through all profiles checking their memory requirements
            for (i = 0; i < count; i++)
            {                
                if (Profiles_Data[Profiles_Names[i]].MinMemory < returnValue)
                {
                    returnValue = Profiles_Data[Profiles_Names[i]].MinMemory;
                }
            }
        }

        if (returnValue == int.MaxValue)
        {
            returnValue = 0;            
            LogWarning("No memory data loaded");
        }

        return returnValue;
    }
    #endregion

    #region device
    private void Device_Clear()
    {
        Device_CalculatedRating = 0f;
        Device_CalculatedRatingExt = 0f;
    }

    public float Device_CalculatedRating { get; set; }
    #endregion
    public float Device_CalculatedRatingExt { get; set; }

    public bool Device_UsingRatingFormula { get; set; }


    #region log
    private const string PREFIX = "DeviceQualityManager:";

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string message)
    {
        Debug.Log(PREFIX + message);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogWarning(string message)
    {
        Debug.LogWarning(PREFIX + message);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogError(string message)
    {
        Debug.LogError(PREFIX + message);
    }
    #endregion
}
