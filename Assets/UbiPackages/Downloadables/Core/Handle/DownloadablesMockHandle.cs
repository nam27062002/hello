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
    ///  No connection error is simulated at t=5s. Download is stopped
    ///  handle.AddAction(5, Downloadables.Handle.EError.NO_CONNECTION);
    /// 
    ///  It simulates that the error has been fixed at t=30s (it will have been stopped 25 seconds). After this the download should resume and it will
    ///  be done at t=45s
    ///  handle.AddAction(30, Downloadables.Handle.EError.NONE);             
    /// </summary>
    public class MockHandle : Handle
    {
        public class Action
        {
            public float TimeAt { get; set; }
            public EError Error { get; set; }
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

        private float TimeAtStart { get; set; }
        private float TimeToDownload { get; set; }
        private float DownloadingTime { get; set; }

        private EError Error { get; set; }

        private List<Action> Actions { get; set; }

        public MockHandle(float downloadedBytesAtStart, float totalBytes, float timeToDownload, bool permissionRequested, bool permissionGranted,
                          List<Action> actions = null)
        {
            Setup(downloadedBytesAtStart, totalBytes, timeToDownload, permissionRequested, permissionGranted, actions);
        }

        //------------------------------------------------------------------------//
        // METHODS																  //
        //------------------------------------------------------------------------//
        public void Setup(float downloadedBytesAtStart, float totalBytes, float timeToDownload, bool permissionRequested, bool permissionGranted, List<Action> actions)
        {
            DownloadedBytesAtStart = downloadedBytesAtStart;
            TotalBytes = totalBytes;

            PermissionRequested = permissionRequested;
            PermissionGranted = permissionGranted;

            TimeToDownload = timeToDownload;
            DownloadingTime = 0f;

            Error = EError.NONE;

            TimeAtStart = Time.realtimeSinceStartup;

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
            return (GetDownloadedBytes() == GetTotalBytes());
        }

        public override long GetDownloadedBytes()
        {
            return (long)(DownloadedBytesAtStart + ((TotalBytes - DownloadedBytesAtStart) * DownloadingTime) / TimeToDownload);
        }

        public override long GetTotalBytes()
        {
            return (long)TotalBytes;
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
            if (!IsAvailable())
            {
                UpdateActions();

                if (Error == EError.NONE)
                {
                    DownloadingTime += Time.deltaTime;
                    if (DownloadingTime > TimeToDownload)
                    {
                        DownloadingTime = TimeToDownload;
                    }
                }
            }
        }

        private void UpdateActions()
        {
            if (Actions != null && Actions.Count > 0)
            {
                float timeSoFar = Time.realtimeSinceStartup - TimeAtStart;

                while (Actions.Count > 0 && Actions[0].TimeAt <= timeSoFar)
                {
                    ApplyAction(Actions[0]);
                    Actions.RemoveAt(0);
                }
            }
        }

        private void ApplyAction(Action action)
        {
            Debug.Log("ApplyAction at " + action.TimeAt + " Error = " + action.Error);
            Error = action.Error;
        }
    }
}
