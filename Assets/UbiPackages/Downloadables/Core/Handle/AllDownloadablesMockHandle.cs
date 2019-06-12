//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    //----------------------------------------------------------------------------//
    // CLASSES																	  //
    //----------------------------------------------------------------------------//
    /// <summary>
    /// Temporary class used as skeleton for UI development while the Asset Groups are 
    /// being implemented.
    /// 
    /// Example:
    /// 
    ///  Creates a mock handle to handle a download of 1000 bytes that starts with 500 bytes already downloaded. 
    ///  The download will take 20 seconds. 
    ///  Mobile data permission has already been requested and it hasn't been granted
    ///  No actions passed in constructor because they'll be added afterwards
    /// 
    ///  handle = new Downloadables.MockHandle(500, 1000, 20, true, false, null);
    /// 
    ///  "No connection" error is simulated at t=5s. Download is stopped
    ///  handle.AddAction(5, Downloadables.Handle.EError.NO_CONNECTION);
    /// 
    ///  Simulate that the error has been fixed at t=30s (it will have been stopped 25 seconds). After this the download should resume and it will
    ///  be done at t=45s
    ///  handle.AddAction(30, Downloadables.Handle.EError.NONE);             
    /// </summary>
    public class AllDownloadablesMockHandle : Handle
    {

        //------------------------------------------------------------------------//
        // SINGLETON                    										  //
        //------------------------------------------------------------------------//
        // Use a singleton to mock the allDownloadablesHandle
        private static AllDownloadablesMockHandle m_instance;

        public static AllDownloadablesMockHandle Instance
        {
            get {

                if (m_instance == null)
                {
                    m_instance = new AllDownloadablesMockHandle();
                }
                return m_instance;
            }
        }

        //------------------------------------------------------------------------//
        // INNER CLASSES                  										  //
        //------------------------------------------------------------------------//

        public class Action
        {
            public float TimeAt { get; set; }
            public EError Error { get; set; }
            public bool isError { get { return Error != EError.NONE; } }
            public int Index { get; set; }

            public Action(float timeAt, EError error)
            {
                TimeAt = timeAt;
                Error = error;
            }
        }

        //------------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES												  //
        //------------------------------------------------------------------------//
        private float DownloadedBytesAtStart { get; set; }
        private float TotalBytes { get; set; }

        private bool PermissionRequested { get; set; }
        private bool PermissionGranted { get; set; }

        private float TimeToDownload { get; set; }
        private float DownloadingTime { get; set; }
        private float DownloadingTimeDelta { get; set; }    // Time downloading during the last "frame"

        private float TimeAtStart { get; set; }
        private float LastUpdateTime { get; set; }

        private float DownloadSpeed { get; set; }

        private EError Error { get; set; }

        private List<Action> Actions { get; set; }


        // Internal
        private bool m_mockupInitialized = false;

        public bool MockupInitialized { get { return m_mockupInitialized; } }



        //------------------------------------------------------------------------//
        // METHODS																  //
        //------------------------------------------------------------------------//

        public void Initialize(float downloadedBytesAtStart, float totalBytes, float timeToDownload, bool permissionRequested, bool permissionGranted,
              List<Action> actions = null)
        {

            m_mockupInitialized = true;
            Setup(downloadedBytesAtStart, totalBytes, timeToDownload, permissionRequested, permissionGranted, actions);

        }


        public void Setup(float downloadedBytesAtStart, float totalBytes, float timeToDownload, bool permissionRequested, bool permissionGranted, List<Action> actions)
        {
            DownloadedBytesAtStart = downloadedBytesAtStart;
            TotalBytes = totalBytes;

            PermissionRequested = permissionRequested;
            PermissionGranted = permissionGranted;

            TimeToDownload = timeToDownload;
            DownloadingTime = 0f;
			DownloadingTimeDelta = 0f;
			DownloadSpeed = 0f;

            Error = EError.NONE;

            TimeAtStart = Time.unscaledTime;
			LastUpdateTime = Time.unscaledTime;

            Actions = actions;
            if (Actions != null)
            {
                int count = Actions.Count;
                for (int i = 0; i < count; i++)
                {
                    Actions[i].Index = i;
                }
            }

            SortActions();
            UpdateActions();
        }

        public void AddAction(float time, EError error)
        {
            if (Actions == null)
            {
                Actions = new List<Action>();
            }

            Action action = new Action(time, error);
            action.Index = Actions.Count;
            Actions.Add(action);
            SortActions();
            UpdateActions();
        }

        private void SortActions()
        {
            if (Actions != null)
            {             
                Actions.Sort(SortActionsFunc);
            }
        }

        private int SortActionsFunc(Action x, Action y)
        {
            if (x.TimeAt == y.TimeAt)
            {
                // Indices are also taken into consideration because List<T>.Sort is not stable (doesn't maintain original order)
                // https://social.msdn.microsoft.com/Forums/vstudio/en-US/f5ea4976-1c3d-4e10-90e7-c7a0491fc28a/stable-sort-using-listlttgt?forum=netfxbcl
                if (x.Index == y.Index)
                {
                    return 0;
                }
                else if (x.Index > y.Index)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }

            }
            else if (x.TimeAt > y.TimeAt)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public override bool IsAvailable()
        {
            return (GetDownloadedBytes() >= GetTotalBytes());
        }

        public override long GetDownloadedBytes()
        {
            return (long)(DownloadedBytesAtStart + ((TotalBytes - DownloadedBytesAtStart) * DownloadingTime) / TimeToDownload);
        }

        public override long GetTotalBytes()
        {
            return (long)TotalBytes;
        }

		public override float GetSpeed() {
			return DownloadSpeed;
		}

		public override void SetIsPermissionRequested(bool value)
        {
            PermissionRequested = value;
        }

        public override void SetIsPermissionGranted(bool value)
        {
            PermissionGranted = value;
        }

        protected override bool ExtendedNeedsToRequestPermission()
        {
            return !PermissionRequested;
        }

        protected override bool ExtendedGetIsPermissionGranted()
        {
            return PermissionGranted;
        }        

        protected override Error.EType ExtendedGetErrorType()
        {
            return EErrorToErrorType(Error);
        }

        public override void Update()
        {
            // Wait until permission is requested (user has accepted the download popup)
            if (!IsAvailable() && PermissionRequested)
            {
				// Store some vars
				long downloadedBytesAtStart = GetDownloadedBytes();

				// Compute downloading time increase in this frame based on actions performed
				DownloadingTimeDelta = 0;
				UpdateActions();

				// Increase total download time
				DownloadingTime += DownloadingTimeDelta;

				// Compute download speed this frame
				long downloadedBytesAtEnd = GetDownloadedBytes();
				long downloadedBytesDelta = downloadedBytesAtEnd - downloadedBytesAtStart;
				if(DownloadingTimeDelta > 0) {
					DownloadSpeed = (float)downloadedBytesDelta / DownloadingTimeDelta;
				} else {
					DownloadSpeed = 0f;
				}

				// Update some vars
				LastUpdateTime = Time.unscaledTime;
            }
        }

        private void UpdateActions()
        {
			// Time at the start and end of the frame
			float timeAtStart = LastUpdateTime - TimeAtStart;
			float timeAtEnd = Time.unscaledTime - TimeAtStart;

			// Aux vars
			Action lastAction = null;
			Action currentAction = null;

			// Are there any actions to process?
			if (Actions != null && Actions.Count > 0)
            {
				while (Actions.Count > 0 && Actions[0].TimeAt <= timeAtEnd)
                {
					currentAction = Actions[0];

					// Adjust timing based on last and current actions
					if(lastAction != null) {
						// Last action wasn't an error and we're introducing one in the same frame
						if(!lastAction.isError && currentAction.isError) {
							// Increase downloading time with the time elapsed between both actions
							DownloadingTimeDelta += currentAction.TimeAt - lastAction.TimeAt;
						}
					} else {
						// There wasn't any error and the first action in the frame is an error
						if(Error == EError.NONE && currentAction.isError) {
							// Increase downloading time with the time elapsed since the start of the frame
							DownloadingTimeDelta += currentAction.TimeAt - LastUpdateTime;
						}
					}

					// Execute action
					ApplyAction(currentAction);
					lastAction = currentAction;
					Actions.RemoveAt(0);
                }
			}

			// End of frame: did we execute any action?
			if(lastAction != null) {
				// Yes! If it was not an error, increase downloading time with the time elapsed since the action happened
				if(!lastAction.isError) {
					DownloadingTimeDelta += timeAtEnd - lastAction.TimeAt;
				}
			} else {
				// No action was executed during this frame
				// If we are downloading (no error), increase downloading time for the whole duration of the frame
				if(Error == EError.NONE) {
					DownloadingTimeDelta = timeAtEnd - timeAtStart;
				}
			}
		}

        private void ApplyAction(Action action)
        {
            Debug.Log("ApplyAction at " + action.TimeAt + " Error = " + action.Error);

			EError previousError = Error;
            Error = action.Error;
		}
	}
}
