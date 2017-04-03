using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_IPHONE
using System.Runtime.InteropServices;

namespace FGOL.Plugins.Native
{
    public class NativeImplementationIOS : INativeImplementation
    {

#region DLL_IMPORTS
	    // ------------------------------------------------------------------------
	    // General
        [DllImport("__Internal")] private static extern string _GetBundleVersion();
	    [DllImport("__Internal")] public static extern string _GetUniqueDeviceIdentifier();
	    [DllImport("__Internal")] private static extern string _GetDeviceName();
	    [DllImport("__Internal")] private static extern string _GetUserLocation();
		[DllImport("__Internal")] private static extern string _GetUserCountryISO();
		[DllImport("__Internal")] private static extern int _GetMemoryUsage();
		[DllImport("__Internal")] private static extern int _GetMaxMemoryUsage();
        [DllImport("__Internal")] private static extern void _DontBackupDirectory(string directory);

        [DllImport("__Internal")] private static extern bool _IsIPodMusicPlaying();
	    [DllImport("__Internal")] private static extern void _ShowMessageBox(string title, string message, int msg_id);
	    [DllImport("__Internal")] private static extern void _ShowMessageBoxWithButtons(string title, string message, string ok_button, string cancel_button, int msg_id);	    
	    [DllImport("__Internal")] private static extern bool _CanOpenURL(string url);

        [DllImport("__Internal")] private static extern string _GetIDFA();
        [DllImport("__Internal")] private static extern bool _IsiOSTrackingEnabled();
        [DllImport("__Internal")] private static extern uint _GetBuildEncryptionChecksum();

		[DllImport("__Internal")] private static extern string _GetLanguage();
		[DllImport("__Internal")] private static extern string _GetConnectionType();

		[DllImport("__Internal")] private static extern bool _IsAudioPlayingFromOtherApps();		
		[DllImport("__Internal")] private static extern void _SetAudioExclusive(bool audioExclusive);

		[DllImport("__Internal")] private static extern string _HashedValueForAccountName(string userAccountName);
		
		[DllImport("__Internal")] private static extern long _GetMaxDeviceMemory();


#endregion

        public string GetBundleVersion()
        {
            return _GetBundleVersion();
        }

        public string GetUniqueUserID()
        {
            return _GetUniqueDeviceIdentifier();
        }

        public string GetUserLocation()
        {
            return _GetUserLocation().ToUpper();
        }

        public string GetAdvertisingIdentifier()
        {
            return _GetIDFA();
        }
        
		public string GetGameLanguageISO() 
		{ 
			return _GetLanguage(); 
		}
		
		public string GetUserCountryISO() 
		{
			return _GetUserCountryISO();
		}
		
		public string GetConnectionType() 
		{
			return _GetConnectionType();
		}
		
		public string GetDeviceName() 
		{
			return _GetDeviceName();
		}

		public string GetPersistentDataPath()
		{
			return Application.persistentDataPath;
		}

        public void DontBackupDirectory(string directory)
        {
            _DontBackupDirectory(directory);
        }


        public string GetExpansionFilePath()
		{
			return "";
		}

        public void ShowMessageBox(string title, string message, int msg_id = -1)
        {
            _ShowMessageBox(title, message, msg_id);
        }

        public void ShowMessageBoxWithButtons(string title, string message, string ok_button, string cancel_button, int msg_id = -1)
        {
            _ShowMessageBoxWithButtons(title, message, ok_button, cancel_button, msg_id);
        }

		public int GetMemoryUsage()
		{
			return _GetMemoryUsage();
		}

		public int GetMaxMemoryUsage()
		{
			return _GetMaxMemoryUsage();
		}

		public long GetMaxHeapMemory()
		{
			return -1;
		}

		public long GetUsedHeapMemory()
		{
			return -1;
		}

		public long GetMaxDeviceMemory()
		{
			return _GetMaxDeviceMemory();
		}

		public long GetAvailableDeviceMemory()
		{
			return -1;
		}

		public long GetDeviceMemoryThreshold()
		{
			return -1;
		}

		public int GetMemoryPSS()
		{
			return -1;
		}

		public bool IsAppInstalled(string url)
		{
			return _CanOpenURL(url);
		}

		public bool IsAudioPlayingFromOtherApps()
		{
			return _IsAudioPlayingFromOtherApps ();
		}
		
		public void RequestExclusiveAudio(bool exclusiveAudio)
		{
			_SetAudioExclusive (exclusiveAudio);
		}

		public void TryShowPermissionExplanation(string[] permissions, string messageTitle, string messageInfo)
		{

		}

		public bool HasPermissions(string[] permissions)
		{
			return true;
		}

		public void ToggleSpinner(bool enable, float x = 0, float y = 0)
		{
			// TODO
		}

		public string HashedValueForAccountName(string userAccountName)
		{
			return _HashedValueForAccountName (userAccountName);
		}

		public string[] GetCertificateSignatureSHA()
		{
			//only used by Android
			return new string[0];
		}

    }
}

#endif