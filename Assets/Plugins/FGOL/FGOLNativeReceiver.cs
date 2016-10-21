using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FGOL.Plugins.Native;

namespace FGOL.Plugins.Native
{
    public class FGOLNativeReceiver : MonoBehaviour
    {
		#region SINGLETON

		private static FGOLNativeReceiver m_instance = null;

		public static FGOLNativeReceiver Instance 
		{
			get { return m_instance; }
		}

		#endregion

        public delegate void OnMessageBoxClick(bool resultOK);
		private Dictionary<int, OnMessageBoxClick> m_callbacks = new Dictionary<int, OnMessageBoxClick>();

		void Awake()
		{
			m_instance = this;
			DontDestroyOnLoad(this);
		}

		void OnDestroy()
		{
			m_instance = null;
		}

		public void ShowMessageBoxWithCallback(string title, string message, OnMessageBoxClick callback)
		{
			// generate a random number as msg_id and pass it to the native side
			int randInt = (int)(Random.value * 1000.0f);
			while(m_callbacks.ContainsKey(randInt))
			{
				randInt = (int)(Random.value * 1000.0f);
			}
		
			m_callbacks.Add(randInt, callback);
			NativeBinding.Instance.ShowMessageBox(title, message, randInt);
		}

		public void ShowMessageBoxWithButtonsAndCallback(string title, string message, string ok_button, string cancel_button, OnMessageBoxClick callback)
		{
			// generate a random number as msg_id and pass it to the native side
			int randInt = (int)(Random.value * 1000.0f);
			while(m_callbacks.ContainsKey(randInt))
			{
				randInt = (int)(Random.value * 1000.0f);
			}
		
			m_callbacks.Add(randInt, callback);
			NativeBinding.Instance.ShowMessageBoxWithButtons(title, message, ok_button, cancel_button, randInt);
		}


		public void MessageBoxClick(string result)
		{
			string[] resultSplit = result.Split(':');
			//FGOL.Assert.Expect(resultSplit.Length == 2);

			int msgId = int.Parse(resultSplit[0]);
			//FGOL.Assert.Expect(msgId != -1);

			string isOK = resultSplit[1];

			Debug.Log("MessageBoxClick callback (" + msgId + ":" + isOK + ")");

			OnMessageBoxClick callback;
			if(m_callbacks.TryGetValue(msgId, out callback))
			{
				if(callback != null)
				{
					callback(isOK.Equals("OK"));
				}
				m_callbacks.Remove(msgId);
			}
		}

		public delegate void OnPermissionReceived(bool wasGranted);
		private OnPermissionReceived m_permissionCallback = null;

		// Array of required permissions, will be overriden upon each permissions request
		private HashSet<string> m_requestedPermissions = new HashSet<string>();

		public void SetRequestedPermissions(string[] permissions, OnPermissionReceived permissionCallback)
		{
			m_requestedPermissions.Clear();

			for(int i = 0; i < permissions.Length; i++)
			{
				m_requestedPermissions.Add(permissions[i]);
			}

			m_permissionCallback = permissionCallback;
		}

		// Android dangerous permission callback success
		public void PermissionReceivedSuccess(string permission)
		{
			Debug.Log("FGOLNativeReceiver :: PermissionReceivedSuccess:" + permission);
			if(m_requestedPermissions.Contains(permission))
			{
				m_requestedPermissions.Remove(permission);
			}
			//no more expected permissions
			if(m_requestedPermissions.Count == 0)
			{
				Debug.Log("FGOLNativeReceiver :: All Permissions received :: Callback set = " + m_permissionCallback != null);
				if(m_permissionCallback != null)
				{
					m_permissionCallback(true);
				}
				m_permissionCallback = null;
			}
		}

		// Android dangerous permission callback fail
		public void PermissionReceivedFailed(string permission)
		{
			Debug.Log("FGOLNativeReceiver PermissionReceivedFailed:" + permission);
			if(m_permissionCallback != null)
			{
				m_permissionCallback(false);
			}
			m_requestedPermissions.Clear();
			m_permissionCallback = null;
		}
	}
}