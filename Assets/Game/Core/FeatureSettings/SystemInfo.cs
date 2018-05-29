
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
        freeMemorySize = maxMemorySize = totalMemorySize = systemMemorySize = UnityEngine.SystemInfo.systemMemorySize;
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
#if UNITY_ANDROID && !UNITY_EDITOR

		AndroidJavaObject jc = new AndroidJavaClass("java.lang.Runtime");
        if (jc != null)
        {
            AndroidJavaObject jo = jc.CallStatic<AndroidJavaObject>("getRuntime");
            Debug.Log("java.lang.Runtime ---> availableProcessors: " + jo.Call<int>("availableProcessors"));
            Debug.Log("java.lang.Runtime ---> freememory: " + jo.Call<long>("freeMemory"));
            Debug.Log("java.lang.Runtime ---> maxmemory: " + jo.Call<long>("maxMemory"));
            Debug.Log("java.lang.Runtime ---> totalmemory: " + jo.Call<long>("totalMemory"));

            freeMemorySize = (int)jo.Call<long>("freeMemory");
            maxMemorySize = (int)jo.Call<long>("maxMemory");
            totalMemorySize = (int)jo.Call<long>("totalMemory");
/*
            jo = new AndroidJavaObject("java.io.RandomAccessFile", "/sys/devices/system/cpu/cpu0/cpufreq/scaling_max_freq", "r");
            if (jo != null)
            {
                string freq = jo.Call<string>("readLine");
                Debug.Log("java.io.RandomAccesFile ---> cpu0 freq: " + freq);
            
            }
            else
            {
                Debug.Log("Unable to open: java.io.RandowAccesFile");                
            }
*/

		} else {	
			systemMemorySize = 0;
            Debug.Log("Unable to open: java.lang.Runtime");
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
    static public int freeMemorySize
    {
        get;
        private set;
    }

    static public int maxMemorySize
    {
        get;
        private set;
    }

    static public int totalMemorySize
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
