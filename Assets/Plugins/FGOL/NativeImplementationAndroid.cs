using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_ANDROID

namespace FGOL.Plugins.Native
{
    public class NativeImplementationAndroid : INativeImplementation
    {
        private static AndroidJavaObject _fgolnative = null;

		private string m_cachedDataPath;
		private string m_cachedExpansionPath;

		static NativeImplementationAndroid()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            if (jc == null)
            {
                Debug.LogError("Could not find class com.unity3d.player.UnityPlayer!");
            }

            // find the plugin instance
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if (jo == null)
            {
                Debug.LogError("Could not find currentActivity!");
            }

            using (var pluginClass = new AndroidJavaClass("com.fgol.FGOLNative"))
            {
                _fgolnative = pluginClass.CallStatic<AndroidJavaObject>("init", jo);                
            }

            if (_fgolnative == null)
            {
                Debug.LogError("Could not find FGOLNative android plugin");
            }
        }

        public string GetBundleVersion()
        {
            return _fgolnative.Call<string>("GetBundleVersion");
        }

        public string GetUniqueUserID()
        {
            return _fgolnative.Call<string>("GetAndroidID");
        }

        public string GetUserLocation()
        {
            return _fgolnative.Call<string>("getUserLocation").ToUpper();
        }

        public string GetAdvertisingIdentifier()
        {
            return _fgolnative.Call<string>("GetAdvertisingIdentifier");
        }
        
		public string GetGameLanguageISO() 
		{ 
			return _fgolnative.CallStatic<string>("GetGameLanguageISO"); 
        } 
		
		public string GetUserCountryISO() 
		{
			return _fgolnative.CallStatic<string>("GetUserCountryISO"); 
        }
		
		public string GetConnectionType() 
		{
			return _fgolnative.CallStatic<string>("GetConnectionType");
        }
		
		public string GetDeviceName() 
		{
			return "TODO";
		}

		public void DontBackupDirectory(string directory)
        {
        }

		public string GetPersistentDataPath()
		{
			if(Application.platform != RuntimePlatform.Android)
			{
				return Application.persistentDataPath;
			}
			else 
			{
				if(string.IsNullOrEmpty(m_cachedDataPath))
				{
					m_cachedDataPath = _fgolnative.CallStatic<string>("GetExternalStorageLocation");

					// In case the path cannot be retrieved from Android, use Unity's one
					if(string.IsNullOrEmpty(m_cachedDataPath))
					{
						m_cachedDataPath = Application.persistentDataPath;
					}
				}

				return m_cachedDataPath;
			}

		}

		public string GetExpansionFilePath()
		{
			if(Application.platform != RuntimePlatform.Android)
			{
				return Application.persistentDataPath;
			}
			else
			{
				if(string.IsNullOrEmpty(m_cachedExpansionPath))
				{
					m_cachedExpansionPath = _fgolnative.CallStatic<string>("GetExpansionFileLocation");
				}

				return m_cachedExpansionPath;
			}
		}

		public void ShowMessageBox(string title, string message, int msg_id = -1)
        {
            _fgolnative.Call("ShowMessageBox", title, message, msg_id);
        }

        public void ShowMessageBoxWithButtons(string title, string message, string ok_button, string cancel_button, int msg_id = -1)
        {
            _fgolnative.Call("ShowMessageBoxWithButtons", title, message, ok_button, cancel_button, msg_id);
        }

		public void TryShowPermissionExplanation(string[] permissions, string messageTitle, string messageInfo)
		{
			TryShowPermissionExplanation(string.Join(",", permissions), messageTitle, messageInfo);
		}

		private void TryShowPermissionExplanation(string permissions, string messageTitle, string messageInfo)
		{
			_fgolnative.CallStatic("TryShowPermissionExplanation", permissions, messageTitle, messageInfo);
		}

		// Checks if user has all required permissions, if a single permission from the array is not granted the method will return false
		public bool HasPermissions(string[] permissions)
		{
			for(int i = 0; i < permissions.Length; i++)
			{
				if(!_fgolnative.CallStatic<bool>("HasPermission", permissions[i]))
				{
					return false;
				}
            }

			return true;
		}

		public bool WasLastTouchPressedHard()
		{
			return false;
		}

		public int GetMemoryUsage()
		{
            return _fgolnative.Call<int>("GetMemoryUsage");
		}

		public int GetMaxMemoryUsage()
		{
			return _fgolnative.Call<int>("GetMaxMemoryUsage");
		}

		public long GetMaxHeapMemory()
		{
			return _fgolnative.Call<long>("GetMaxHeapMemory");
        }

		public long GetUsedHeapMemory()
		{
			return _fgolnative.Call<long>("GetUsedHeapMemory");
		}

		public long GetMaxDeviceMemory()
		{
			return _fgolnative.Call<long>("GetMaxDeviceMemory");
		}

		public long GetAvailableDeviceMemory()
		{
			return _fgolnative.Call<long>("GetAvailableDeviceMemory");
		}

		public long GetDeviceMemoryThreshold()
		{
			return _fgolnative.Call<long>("GetDeviceMemoryThreshold");
		}

		public int GetMemoryPSS()
		{
			return _fgolnative.Call<int>("GetTotalMemoryPSS");
		}

		public bool IsAppInstalled(string bundleID)
		{
			return _fgolnative.Call<bool>("isAppInstalled", bundleID);
		}

		public bool IsAudioPlayingFromOtherApps()
		{
			//	TODO: implement
			return false;
		}		

		public void RequestExclusiveAudio(bool exclusiveAudio)
		{
			//	TODO
		}

		public void ToggleSpinner(bool enable, float x = 0, float y = 0)
		{
			_fgolnative.Call("ToggleSpinner", enable, x, y);
		}

		public string HashedValueForAccountName(string userAccountName)
		{
			return userAccountName;
		}

		public string[] GetCertificateSignatureSHA()
		{
			//only used by Android
			int numCerts = _fgolnative.CallStatic<int>("GetNumCertificates");
			if (numCerts == 0)
			{
				return new string[0];
			}
			string[] certs = new string[numCerts];
			for (int i=0; i < numCerts; i++)
			{
				certs[i] = _fgolnative.CallStatic<string>("GetCertificateSignatureSHA", i);
            }
			return certs;
        }
	}
}

#endif