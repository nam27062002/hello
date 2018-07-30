﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

public abstract class PlatformUtils 
{
	// public delegate void OnTokens(bool success, Dictionary<string,string> tokens);
	// public static OnTokens onTokens;

	private static PlatformUtils instance;

	public static PlatformUtils Instance
	{
		get{
			if(instance == null) {
#if UNITY_IPHONE
				instance = new PlatformUtilsIOSImpl();
#elif UNITY_ANDROID
				instance = new PlatformUtilsAndroidImpl();
#else
				instance = new PlatformUtilsDummyImpl();
#endif
			}
			return instance;
		}
	}

	public abstract string GetCountryCode();
	public abstract void GetTokens();
	public abstract void ShareImage(string filename, string caption);
	public abstract void MakeToast(string text, bool longDuration);
	public abstract string GetTrackingId();
	public abstract string FormatPrice( float price, string currencyLocale );
	
	
	public virtual void askPermissions(){}
	public virtual bool arePermissionsGranted(){ return true; }
	
	public virtual long getFreeDataSpaceAvailable(){ return 0; }
	
	// Replaces Social.ReportProgress in iOS because it doesn't work 
	public virtual void ReportProgress( string achievementId, double progress) {}

	public virtual string[] GetCommandLineArgs(){ return System.Environment.GetCommandLineArgs(); }

	public virtual bool InputPressureSupprted(){ return Input.touchPressureSupported; }

    public bool IsChina()
    {
        string countryCode = PlatformUtils.Instance.GetCountryCode();
        if (countryCode != null)
        {
            countryCode.ToUpper();
        }

        return countryCode == "CN";
    }
}
