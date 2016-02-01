using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if !UNITY_IPHONE && !UNITY_ANDROID
public class PlatformUtilsDummyImpl: PlatformUtils
{
	public override string GetCountryCode()
	{
		return "US";
	}

	public override void GetTokens(){}

	public override void ShareImage(string filename, string caption) {}
	
	public override void MakeToast(string text, bool longDuration){}

	public override string GetTrackingId()
	{
		return "dummy";
	}

    public override string FormatPrice( float price , string currencyLocale )
	{
		return currencyLocale + " " + price; 
	}
	
	public override long getFreeDataSpaceAvailable()
	{
		return 1 * 1024 * 1024 * 1024;	// 1Gb
/*
		// Find Drive Info
		DriveInfo[] allDrives = DriveInfo.GetDrives();
		string rootDir = Directory.GetDirectoryRoot( Application.persistentDataPath );
		foreach( DriveInfo di in allDrives)
		{
			if ( di.RootDirectory.Name == rootDir )
			{
				return di.AvailableFreeSpace;
			}
		}
		return 0;
*/
	}
}
#endif
