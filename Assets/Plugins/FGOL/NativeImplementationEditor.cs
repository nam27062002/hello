#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FGOL.Plugins.Native
{
    class NativeImplementationEditor : INativeImplementation
    {
        private string m_generatedUserId = null;
        private string m_generatedAdvertId = null;

        public NativeImplementationEditor()
        {
            m_generatedUserId = "USER_UNITY_EDITOR_" + Guid.NewGuid().ToString();
            m_generatedAdvertId = Guid.NewGuid().ToString();
        }

        public string GetBundleVersion()
        {
            return PlayerSettings.bundleVersion;
        }

        public string GetUniqueUserID()
        {
            return m_generatedUserId;
        }

        public string GetUserLocation()
        {
            return "FGOL_OFFICE";
        }
		
		public string GetGameLanguageISO() 
		{ 
			return "en"; 
		} 
		
		public string GetUserCountryISO() 
		{
			return "GB"; 
		}
		
		public string GetConnectionType() 
		{
			return "Wifi";
		}
		
		public string GetDeviceName() 
		{
			return "EDITOR";
		}
		

        public string GetAdvertisingIdentifier()
        {
            return m_generatedAdvertId;
        }

        public void DontBackupDirectory(string directory)
        {
        }

        public string GetPersistentDataPath()
		{
			return Application.persistentDataPath;
		}

		public string GetExpansionFilePath()
		{
			return "";
		}

		public void ShowMessageBox(string title, string message, int msg_id = -1)
		{
			bool result = UnityEditor.EditorUtility.DisplayDialog(title, message, "OK");

			if(msg_id != -1)
			{
				FGOLNativeReceiver.Instance.MessageBoxClick(string.Format("{0}:{1}", msg_id, result ? "OK" : "CANCEL"));
			}
        }

        public void ShowMessageBoxWithButtons(string title, string message, string ok_button, string cancel_button, int msg_id = -1)
        {
			bool result = UnityEditor.EditorUtility.DisplayDialog(title, message, ok_button, cancel_button);

			if(msg_id != -1)
			{
				FGOLNativeReceiver.Instance.MessageBoxClick(string.Format("{0}:{1}", msg_id, result ? "OK" : "CANCEL"));
			}
		}

		public int GetMemoryUsage()
		{
			return 0;
		}

		public int GetMaxMemoryUsage()
		{
			return 0;
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
			return -1;
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

        public bool IsAppInstalled(string bundleID)
		{
			return false;
		}

		public bool IsAudioPlayingFromOtherApps()
		{
			return false;
		}				
		
		public void RequestExclusiveAudio(bool exclusiveAudio)
		{
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

		}

		public string HashedValueForAccountName(string userAccountName)
		{
			return userAccountName;
		}

		public string[] GetCertificateSignatureSHA()
		{
			//only used by Android
			return new string[0];
		}
	}
}

#endif