using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_IOS
public class PlatformUtilsIOSImpl : PlatformUtils
{
	[DllImport("__Internal")]
	private static extern string IOsGetCountryCode();
	
	public override string GetCountryCode()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer )
			return IOsGetCountryCode();
		return "US";
	}
	
	[DllImport("__Internal")]
	public static extern void generateIdentityVerificationSignature();
	
	public override void GetTokens()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer )
		{
			generateIdentityVerificationSignature();
		}
		/*
		else
		{
			if ( onTokens != null )
				onTokens( Social.localUser.authenticated, new Dictionary<string,string>());
		}
		*/
	}

	
	public override void MakeToast(string text, bool longDuration){
		Debug.Log("MakeToast unimplemented"); // TODO: Edu.
	}

	[DllImport("__Internal")]
	private static extern string IOsGetTrackingId();

	public override string GetTrackingId()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer )
			return IOsGetTrackingId();
		return "DEFAULT";
	}
	
	[DllImport("__Internal")]
	private static extern string IOsFormatPrice( float price, string currencyLocale );
	
	public override string FormatPrice( float price, string currencyLocale )
	{
		if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			return IOsFormatPrice(price, currencyLocale);
		}
		return currencyLocale + price;
	}
	
	[DllImport("__Internal")]
	private static extern long IOsGetFreeSpaceAvailable( string path );
	
	public override long getFreeDataSpaceAvailable()
	{
		if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			return IOsGetFreeSpaceAvailable( Application.persistentDataPath );
		}
		else
		{
			// Find Drive Info
			DriveInfo[] allDrives = DriveInfo.GetDrives();
			string rootDir = Directory.GetDirectoryRoot( Application.persistentDataPath );
			foreach( DriveInfo di in allDrives)
			{
				if ( di.RootDirectory.Name.Equals( rootDir) )
				{
					return di.AvailableFreeSpace;
				}
			}
		}
		return 0;
	}

	#region sharing images
	public struct ConfigStruct
	{
		public string title;
		public string message;
	}
	
	[DllImport ("__Internal")] private static extern void showAlertMessage(ref ConfigStruct conf);
	
	public struct SocialSharingStruct
	{
		public string text;
		public string url;
		public string image;
		public string subject;
	}
	
	[DllImport ("__Internal")] private static extern void showSocialSharing(ref SocialSharingStruct conf);
	
	private void CallSocialShare(string title, string message)
	{
		ConfigStruct conf = new ConfigStruct();
		conf.title  = title;
		conf.message = message;
		showAlertMessage(ref conf);
	}
	
	private void CallSocialShareAdvanced(string img, string caption)
	{
		SocialSharingStruct conf = new SocialSharingStruct();
		conf.text = caption; 
		conf.url = "";
		conf.image = img;
		conf.subject = "";
		
		showSocialSharing(ref conf);
	}

	public override void ShareImage(string filename, string caption){
		CallSocialShareAdvanced(filename, caption);
	}
	#endregion
	
	
	[DllImport ("__Internal")] private static extern void IOsReportAchievement( string achievementId, double progress);
	
	public override void ReportProgress( string achievementId, double progress) 
	{
		if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			IOsReportAchievement( achievementId, progress );
		}
	}


	[DllImport ("__Internal")] private static extern ulong IOsGetResidentMemory();
	public override ulong getResidentMemory()
	{
		if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			return IOsGetResidentMemory();
		}

		return 0;
	}

	[DllImport ("__Internal")] private static extern ulong IOsGetMaxResidentMemory();
	public override ulong getMaxResidentMemory()
	{
		if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			return IOsGetMaxResidentMemory();
		}

		return 0;
	}

	static string[] s_localParams = null;
	[DllImport ("__Internal")] private static extern string IOsGetCommandLineArgs();
	public override string[] GetCommandLineArgs()
	{
		if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			if ( s_localParams == null )
			{
				string _params = IOsGetCommandLineArgs();
				s_localParams = _params.Split('#');
			}
			return s_localParams;
		}
		return System.Environment.GetCommandLineArgs();
	}
	
}
#endif
