﻿/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SystemInfo {


    static SystemInfo()
    {
        Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>SystemInfo init");


        operatingSystem = UnityEngine.SystemInfo.operatingSystem;
        deviceModel = UnityEngine.SystemInfo.deviceModel;
        deviceUniqueIdentifier = UnityEngine.SystemInfo.deviceUniqueIdentifier;
        systemMemorySize = UnityEngine.SystemInfo.systemMemorySize;
        graphicsMemorySize = UnityEngine.SystemInfo.graphicsMemorySize;
        processorFrequency = UnityEngine.SystemInfo.processorFrequency;
        processorCount = UnityEngine.SystemInfo.processorCount;
        graphicsDeviceID = UnityEngine.SystemInfo.graphicsDeviceID;
        graphicsDeviceVendor = UnityEngine.SystemInfo.graphicsDeviceVendor;
        graphicsDeviceVendorID = UnityEngine.SystemInfo.graphicsDeviceVendorID;
        graphicsDeviceVersion = UnityEngine.SystemInfo.graphicsDeviceVersion;
        graphicsShaderLevel = UnityEngine.SystemInfo.graphicsShaderLevel;
        maxTextureSize = UnityEngine.SystemInfo.maxTextureSize;
        processorType = UnityEngine.SystemInfo.processorType;
        deviceName = UnityEngine.SystemInfo.deviceName;
        supportsImageEffects = UnityEngine.SystemInfo.supportsImageEffects;

#if UNITY_ANDROID
        AndroidJavaClass jv = new AndroidJavaClass("android.app.ActivityManager.MemoryInfo");
        if (jv != null)
        {
            systemMemorySize = jv.GetStatic<int>("totalMem");
        }
        else
        {
            Debug.Log("Unable to open: android.app.ActivityManager.MemoryInfo");
        }

#endif
    }


    static public string operatingSystem
    {
        get;
        private set;
    }

    static public string deviceModel
    {
        get;
        private set;
    }

    static public string deviceUniqueIdentifier
    {
        get;
        private set;
    }

    static public int systemMemorySize
    {
        get;
        private set;
    }

    static public int graphicsMemorySize
    {
        get;
        private set;
    }

    static public int processorFrequency
    {
        get;
        private set;
    }

    static public int processorCount
    {
        get;
        private set;
    }

    static public int graphicsDeviceID
    {
        get;
        private set;
    }

    static public string graphicsDeviceVendor
    {
        get;
        private set;
    }

    static public int graphicsDeviceVendorID
    {
        get;
        private set;
    }

    static public string graphicsDeviceVersion
    {
        get;
        private set;
    }

    static public int graphicsShaderLevel
    {
        get;
        private set;
    }

    static public int maxTextureSize
    {
        get;
        private set;
    }

    static public string processorType
    {
        get;
        private set;
    }

    static public string deviceName
    {
        get;
        private set;
    }

    static public bool supportsImageEffects
    {
        get;
        private set;
    }
}

*/
