using UnityEngine;
using System.Collections;
//[DGR] No support added yet
//using FGOL.Plugins.Native;
using System;

namespace FGOL.ExpansionFiles
{
	// Responsible for downloading required expansion file (currently only supports OBB on Android)
	public class ExpansionFileDownloader : MonoBehaviour
	{
		private const float	DownloadCompleteCheckInterval = 0.5f;   // How often should we check if expansion file has already been downloaded

		private State	m_state;				// Current state
		private float	m_timer;                // Time since state has started - not timescale dependant
        // [DGR] No support added yet
        /*
        private bool	m_userAcceptedDownload = false; // User pressed download button
		private string	m_expansionFilePath = null;	// Path to the expansion file
		private string	m_localUrl = null;			// Local URL to the expansion file
        */


		// Possible errors
		public enum Result
		{
			Success,
			UserDownloadPermissionRequired,
			ErrorStorage,
			ErrorDownload
		}

		// Possible states
		private enum State
		{
			Idle,						// Not active requests
			Downloading,				// Downloading expansion from remote server
			Loading,					// Loading expansion from local URL
		}

		// Raised upon finishing downloading expansion file (either successfully or upon error)
		public delegate void ExpansionFileDownloadResultEventHandler(Result result, string message);
		public static event ExpansionFileDownloadResultEventHandler OnExpansionFileDownloadResult;

		public void Awake()
		{
			SetState(State.Idle);

			//	Warning fix	
            /*
            //[DGR] No support added yet		
			GameUtil.Noop(m_userAcceptedDownload);
			GameUtil.Noop(m_expansionFilePath);
			GameUtil.Noop(m_localUrl);
            */
		}

		// Checks if expansion files are required and downloads them, if already present or not required raises downloaded event immidiately
		// Receives reference to callback method
		public void GetExpansionFile(ExpansionFileDownloadResultEventHandler OnExpansionFileDownloadResultCallback, bool userAcceptedDownload)
		{
			if(m_state != State.Idle)
			{
				return;
			}

			// Set callback
			OnExpansionFileDownloadResult = OnExpansionFileDownloadResultCallback;

            // [DGR] Not supported yet
#if !UNITY_EDITOR && !SINGLE_APK && (UNITY_ANDROID && !AMAZON) && false
			m_userAcceptedDownload = userAcceptedDownload;
			if(Application.platform == RuntimePlatform.Android)
			{
				m_expansionFilePath = GooglePlayDownloader.GetExpansionFilePath();
				Debug.Log(string.Format("ExpansionFileManager :: File path = {0}", m_expansionFilePath));
				if(m_expansionFilePath == null)
				{
					// No storage detected for this device - ask them to check and restart
					Finish(Result.ErrorStorage);
				}
				else
				{
					string mainPath = GooglePlayDownloader.GetMainOBBPath(m_expansionFilePath);
					Debug.Log(string.Format("ExpansionFileManager :: Obb local path = {0}", mainPath));
					if(string.IsNullOrEmpty(mainPath) && !m_userAcceptedDownload)
					{
						// We need to download the Obb file so give the user a button to start the sequence
						Finish(Result.UserDownloadPermissionRequired);
					}
					else if(string.IsNullOrEmpty(mainPath) && m_userAcceptedDownload)
					{
						SetState(State.Downloading);
					}
					else
					{
						// User already has the OBB file, just load it from local path
						m_localUrl = "file://" + mainPath;
						SetState(State.Loading);
					}
				}
			}
			else
			{
				Debug.Log("ExpansionFileManager :: Device is not being detected as running Android");
				Finish(Result.Success);
			}
#else
			// No need to look for anything to download on these platforms - just move straight to game
			Finish(Result.Success);
#endif
		}

		public bool IsExpansionFileReady()
		{
#if UNITY_IOS || UNITY_EDITOR || SINGLE_APK
			return true;
#elif UNITY_ANDROID && !AMAZON
            // [DGR] Not supported yet
            /*
			m_expansionFilePath = GooglePlayDownloader.GetExpansionFilePath();
			if(m_expansionFilePath != null)
			{
				string mainPath = GooglePlayDownloader.GetMainOBBPath(m_expansionFilePath);
				if(!string.IsNullOrEmpty(mainPath))
				{
					return true;
				}
			}*/

			return false;
#endif
            return true;
		}

		// New state logic
		private void SetState(State state)
		{
			m_state = state;
			m_timer = 0;

			switch(m_state)
			{
				case State.Idle:
					break;

				case State.Downloading:
                    // [DGR] Not supported yet
#if UNITY_ANDROID && !UNITY_EDITOR && !AMAZON && false
					// Fetch OBB
					GooglePlayDownloader.FetchOBB();
#endif
					// Set timer to check interval for it to be checked on the next frame
					m_timer = DownloadCompleteCheckInterval;
					break;

				case State.Loading:
#if UNITY_ANDROID && !UNITY_EDITOR
					// According to official Unity response in the forums, it's not neccessary to load the obb file manually using www as it's automatically loaded http://forum.unity3d.com/threads/more-google-obb-drama.135224/
					Finish(Result.Success);
					//StartCoroutine(DownloadAndLoad());
#endif
					break;
			}
		}

		// Called on each frame
		private void Update()
		{
			m_timer += Time.unscaledDeltaTime;

			switch(m_state)
			{
				case State.Idle:
					break;

				case State.Downloading:
                    // [DGR] Not supported yet
#if UNITY_ANDROID && !UNITY_EDITOR && !AMAZON && false
					// Check again after given time interval
					if(m_timer >= DownloadCompleteCheckInterval)
					{
						string mainPath = GooglePlayDownloader.GetMainOBBPath(m_expansionFilePath);
						Debug.Log(string.Format("ExpansionFileManager :: Downloading obb :: Main path = {0}", mainPath));

						// If path has been set, set state to downloading
						if(!string.IsNullOrEmpty(mainPath))
						{
							// Set URL for further usage
							m_localUrl = "file://" + mainPath;
							SetState(State.Loading);
						}
						// Otherwise reset timer and try next time
						else
						{
							m_timer = 0;
						}
					}
#endif
					break;

				case State.Loading:
					break;
			}
		}

		// Notify listeners and get back to idle state
		private void Finish(Result result, string message = null)
		{
			if(OnExpansionFileDownloadResult != null)
			{
				OnExpansionFileDownloadResult(result, message);
			}

			OnExpansionFileDownloadResult = null;

			// Set state back to idle
			SetState(State.Idle);
		}

#if UNITY_ANDROID && !UNITY_EDITOR
	/*protected IEnumerator DownloadAndLoad()
	{
		Debug.Log(string.Format("ExpansionFileManager :: Obb local path = {0}", m_localUrl));
		WWW www = WWW.LoadFromCacheOrDownload(m_localUrl, 0);

		// Wait for download to complete
		yield return www;
		Debug.Log("ExpansionFileManager :: www finsihed");

		if(www.error != null)
		{
			Debug.Log(string.Format("ExpansionFileManager :: www error = {0}", www.error));
			Finish(Result.ErrorDownload, www.error);
		}
		else
		{
			Finish(Result.Success);
		}
	}*/
#endif
	}
}

