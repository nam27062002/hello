using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for storing a downloadable entry status
    /// </summary>
    public class CatalogEntryStatus
    {
        public static float TIME_TO_WAIT_AFTER_ERROR = 10f;
        public static float TIME_TO_WAIT_BETWEEN_SAVES = 3f;
        public static float TIME_TO_WAIT_BETWEEN_ACTUAL_UPDATES = 180f;
        private static float BYTES_TO_MB = 1024 * 1024;

        private static Disk sm_disk;        
        private static Tracker sm_tracker;

        public delegate void OnDownloadEndCallback(CatalogEntryStatus entryStatus, Error.EType errorType);
        private static OnDownloadEndCallback sm_onDownloadEndCallback;

        // Stored so it can be used by callbacks that are called from downloader thread, which can't use Time.realtiemSinceStartup because this must be called from main thread
        private static float sm_realtimeSinceStartup;
        private static NetworkReachability sm_currentNetworkReachability;        

        private static CatalogEntry sm_entryHelper = new CatalogEntry();

        private static Config sm_config;

        public static void StaticSetup(Config config, Disk disk, Tracker tracker, OnDownloadEndCallback onDownloadEndCallback = null)
        {
            sm_config = config;
            sm_disk = disk;
            sm_tracker = tracker;
            sm_onDownloadEndCallback = onDownloadEndCallback;
        }

        public static void StaticUpdate(float realtimeSinceStartup, NetworkReachability currentNetworkReachability)
        {
            sm_realtimeSinceStartup = realtimeSinceStartup;
            sm_currentNetworkReachability = currentNetworkReachability;
        }

        public string Id { get; private set; }        

        public enum EState
        {
            None,
            ReadingManifest,
            ReadingDataInfo,
            InQueueForDownload,
            Downloading,
            CalculatingCRC,
            DealingWithCRCMismatch,
            Available
        };

        private EState m_state;
        public EState State
        {
            get
            {
                return m_state;
            }

            private set
            {
                switch (m_state)
                {                    
                    case EState.CalculatingCRC:
                        if (CalculatingCRCFromADownload)
                        {
                            CalculatingCRCFromADownload = false;

                            Error.EType errorType = Error.EType.Other;
                            switch (value)
                            {
                                case EState.Available:
                                    errorType = Error.EType.None;
                                    break;

                                case EState.DealingWithCRCMismatch:
                                    errorType = Error.EType.Internal_CRC_Mismatch;
                                    break;
                            }

                            OnDownloadEnd(errorType);
                        }
                        break;
                }

                m_state = value;

                // Latest error and save are reseted so the incoming state will be executed and will be able to save data right away
                m_latestErrorAt = -1f;
                m_latestSaveAt = -1f;
                m_latestSimulationAt = -1f;

                switch (m_state)
                {
                    case EState.ReadingDataInfo:
                        m_dataInfo.Size = 0;
                        m_dataInfo.CRC = 0;
                        break;

                    case EState.Downloading:
                        LatestError = null;                        
                        TrackDownloadStart();
                        break;

                    case EState.Available:
                        if (m_requestState == ERequestState.Running)
                        {
                            m_requestState = ERequestState.Done;
                        }
                        break;
                }
            }
        }

        private CatalogEntryManifest m_manifest;
        private CatalogEntry m_dataInfo;
        public CatalogEntry DataInfo { get { return m_dataInfo; } }

        private float m_latestErrorAt;
        private float m_latestSaveAt;
        private float m_latestSimulationAt;
        private float m_latestActualUpdateAt;

        public enum ERequestState
        {
            None,
            Running,
            Done
        };

        private ERequestState m_requestState;        
        public ERequestState RequestState
        {
            get
            {
                return m_requestState;
            }

            private set
            {
                m_requestState = value;

                switch (m_requestState)
                {
                    case ERequestState.None:
                        RequestError = null;
                        break;

                    case ERequestState.Running:
                        // Latest error and save are reseted so the request will be resolved with no delays
                        m_latestErrorAt = -1f;
                        m_latestSaveAt = -1f;
                        m_latestSimulationAt = -1f;
                        break;
                }
            }
        }
        
        public Error LatestError { get; private set; }
        public void ResetLatestError()
        {
            LatestError = null;
            m_latestErrorAt = -1f;
        }

        public Error RequestError { get; private set; }        
       
        /// <summary>
        /// Amount of times a CRC mismatch error happened
        /// </summary>
        private int CRCMismatchErrorTimes { get; set; }        

        private bool CalculatingCRCFromADownload { get; set; }

        /// <summary>
        /// Set of groups. Used to look up mobile data access permission
        /// </summary>
        private HashSet<CatalogGroup> Groups { get; set; }

        public CatalogEntryStatus()
        {
            m_manifest = new CatalogEntryManifest();
            m_dataInfo = new CatalogEntry();

            Reset();
        }

        public void Reset()
        {
            Id = null;
            m_manifest.Reset();
            m_dataInfo.Reset();
            State = EState.None;
            RequestState = ERequestState.None;            
            CRCMismatchErrorTimes = 0;
            CalculatingCRCFromADownload = false;

            if (Groups != null)
            {
                Groups.Clear();
            }
        }

        public void LoadManifest(string id, JSONNode json)
        {
            Id = id;
            m_manifest.Load(json);
            State = EState.ReadingManifest;
        }
        
        private bool HasErrorExpired()
        {
            bool returnValue = m_latestErrorAt < 0f;
            if  (!returnValue)
            {
                float timeSinceLatestError = sm_realtimeSinceStartup - m_latestErrorAt;
                returnValue = timeSinceLatestError >= TIME_TO_WAIT_AFTER_ERROR;                 
            }

            return returnValue;
        }

        private bool HasSimulationExpired()
        {
            bool returnValue = m_latestSimulationAt < 0f;
            if (!returnValue)
            {
                float timeSinceLatestSimulation = sm_realtimeSinceStartup - m_latestSimulationAt;
                returnValue = timeSinceLatestSimulation >= TIME_TO_WAIT_AFTER_ERROR;
            }

            return returnValue;
        }

        public void Update()
        {           
            if (m_requestState == ERequestState.Done)
            {               
                m_requestState = ERequestState.None;
            }

            // It shouldn't be updated if it's downloading because all stuff is done by DownloadablesDownloader in a separate thread
            bool canUpdate = m_state != EState.Downloading && HasErrorExpired();
            if (canUpdate)
            {
                switch (m_state)
                {
                    case EState.ReadingManifest:
                        ProcessReadingManifest();
                        break;

                    case EState.ReadingDataInfo:
                        ProcessReadingDataInfo();
                        break;

                    case EState.InQueueForDownload:
                        break;

                    case EState.CalculatingCRC:
                        ProcessCalculatingCRC();
                        break;

                    case EState.DealingWithCRCMismatch:
                        ProcessDealingWithCRCMismatch();
                        break;

                    case EState.Available:
                        if (sm_realtimeSinceStartup - m_latestActualUpdateAt >= TIME_TO_WAIT_BETWEEN_ACTUAL_UPDATES)
                        {
                            m_latestActualUpdateAt = sm_realtimeSinceStartup;

                            Error error;

                            // Verifies that the file is still in disk
                            bool exists = sm_disk.File_Exists(Disk.EDirectoryId.Downloads, Id, out error);

                            bool needsToDownloadAgain = false;
                            if (error == null)
                            {
                                if (exists)
                                {
                                    // Gets the downloaded file size
                                    FileInfo fileInfo = sm_disk.File_GetInfo(Disk.EDirectoryId.Downloads, Id, out error);
                                    if (error == null)
                                    {
                                        if (fileInfo.Length != m_dataInfo.Size)
                                        {
                                            needsToDownloadAgain = true;
                                        }                                        
                                    }
                                }
                                else
                                {
                                    needsToDownloadAgain = true;
                                }
                            }

                            if (needsToDownloadAgain)
                            {
                                State = EState.ReadingDataInfo;
                            }
                        }
                        break;
                }

                if (m_manifest.NeedsToSave)
                {
                    bool canSave = true;

                    if (m_latestSaveAt >= 0f)
                    {
                        float timeSinceLatestSave = sm_realtimeSinceStartup - m_latestSaveAt;
                        canSave = timeSinceLatestSave >= TIME_TO_WAIT_BETWEEN_SAVES;
                    }

                    if (canSave)
                    {
                        Save();
                    }
                }
            }
        }

        public void Save()
        {
            if (m_manifest.NeedsToSave)
            {                
                Error error;                
                sm_disk.File_WriteJSON(Disk.EDirectoryId.Manifests, Id, m_manifest.ToJSON(), out error);
                if (error == null)
                {
                    m_manifest.NeedsToSave = false;
                }
                else
                {
                    m_latestSaveAt = sm_realtimeSinceStartup;
                    NotifyError(error);
                }
            }
        }

        private void NotifyError(Error error)
        {
            if (error != null)
            {
                m_latestErrorAt = sm_realtimeSinceStartup;
                LatestError = error;
                                
                if (m_requestState == ERequestState.Running)
                {
                    RequestError = error;
                    RequestState = ERequestState.Done;
                }
            }            
        }

        public CatalogEntryManifest GetManifest()
        {
            return m_manifest;
        }

        public bool HasBeenDownloadedBefore()
        {
            return m_manifest.DownloadedTimes > 0;
        }

        private void ProcessReadingManifest()
        {                      
            Error error = null;
            bool exists = sm_disk.File_Exists(Disk.EDirectoryId.Manifests, Id, out error);

            bool canAdvance = error == null;
            if (error == null && exists)
            {
                JSONNode json = sm_disk.File_ReadJSON(Disk.EDirectoryId.Manifests, Id, out error);
                canAdvance = (error == null);
                if (error == null)
                {
                    sm_entryHelper.Load(json);

                    // Check if the manifest in disk is outdated. If it's not then we need to update the manifest
                    // information with the one read from disk as it contains the amount of times it's been downloaded
                    if (sm_entryHelper.Compare(m_manifest.CatalogEntry))
                    {
                        m_manifest.Load(json);
                    }
                    else
                    {
                        // Deletes the download corresponding to this entry, if it exists, as it's outdated
                        sm_disk.File_Delete(Disk.EDirectoryId.Downloads, Id, out error);                        

                        canAdvance = error == null;

                        // We need to update the manifest after having deleted its download because otherwise
                        // if there's an error when deleting the download and the download doesn't get deleted,
                        // it would be considered as the correct one
                        if (canAdvance)
                        {
                            m_manifest.IsVerified = false;                            
                        }
                    }
                }                
            }

            if (error != null)
            {                                
                NotifyError(error);
            }

            if (canAdvance)
            {
                State = EState.ReadingDataInfo;
            }
        }

        private void ProcessReadingDataInfo()
        {            
            Error error = null;
            bool exists = sm_disk.File_Exists(Disk.EDirectoryId.Downloads, Id, out error);
            
            if (error == null)
            {
                if (exists)
                {
                    // Gets the downloaded file size
                    FileInfo fileInfo = sm_disk.File_GetInfo(Disk.EDirectoryId.Downloads, Id, out error);
                    if (error == null)
                    {
                        m_dataInfo.Size = fileInfo.Length;
                        if (m_manifest.IsVerified)
                        {
                            m_dataInfo.CRC = m_manifest.CRC;
                        }
                    }
                }

                if (error == null)
                {
                    if (m_dataInfo.Size == m_manifest.Size)
                    {
                        State = (m_manifest.IsVerified) ? EState.Available : EState.CalculatingCRC;
                    }
                    else if (m_dataInfo.Size > m_manifest.Size)
                    {
                        // Too big, which means that the file is not valid, hence the file downloaded must be deleted
                        sm_disk.File_Delete(Disk.EDirectoryId.Downloads, Id, out error);
                        if (error == null)
                        {
                            m_dataInfo.Size = 0;
                            m_dataInfo.CRC = 0;

                            m_manifest.IsVerified = false;
                            State = EState.InQueueForDownload;
                        }
                    }
                    else
                    {
                        State = EState.InQueueForDownload;
                    }
                }

                if (error != null)
                {
                    NotifyError(error);
                }
            }
        }

        private void ProcessCalculatingCRC()
        {
            Error error = null;

            // Makes sure that the download file exists (the user may have deleted the device cache right before
            // this entry entered in this state)
            bool exists = sm_disk.File_Exists(Disk.EDirectoryId.Downloads, Id, out error);

            if (error == null)
            {
                if (exists)
                {
                    byte[] bytes = sm_disk.File_ReadAllBytes(Disk.EDirectoryId.Downloads, Id, out error);
                    if (error == null)
                    {
                        long CRC = StringUtils.CRC32(bytes);
                        if (CRC == m_manifest.CRC)
                        {
                            m_manifest.IsVerified = true;
                            State = EState.Available;
                        }
                        else
                        {
                            CRCMismatchErrorTimes++;
                            State = EState.DealingWithCRCMismatch;
                        }
                    }
                }
                else
                {
                    State = EState.ReadingDataInfo;
                }
            }

            if (error != null)
            {
                NotifyError(error);
            }
        }

        private void ProcessDealingWithCRCMismatch()
        {
            Error error;
            sm_disk.File_Delete(Disk.EDirectoryId.Downloads, Id, out error);
            if (error == null)
            {
                State = EState.ReadingDataInfo;
            }
            else
            {
                NotifyError(error);
            }
        }

        public long GetTotalBytes()
        {
            return m_manifest.Size;
        }

        public float GetTotalMb()
        {
            return m_manifest.Size / BYTES_TO_MB;
        }

        public long GetBytesLeftToDownload()
        {
            long diff = m_manifest.Size - m_dataInfo.Size;
            if (diff < 0)
            {
                diff = 0;
            }

            return diff;
        }

        public long GetBytesDownloadedSoFar()
        {            
            return m_dataInfo.Size;
        }

        public float GetMbDownloadedSoFar()
        {
            return GetBytesDownloadedSoFar() / BYTES_TO_MB;
        }

        public float GetMbLeftToDownload()
        {
            return GetBytesLeftToDownload() / BYTES_TO_MB;
        }

        public bool Compare(CatalogEntryStatus other)
        {
            return other != null && other.m_manifest.Compare(other.m_manifest) && m_dataInfo.Compare(other.m_dataInfo);
        }

        public bool CanBeRequested()
        {
            return RequestState == ERequestState.None;
        }        

        public void Request()
        {
            if (RequestState == ERequestState.None)
            {
                RequestState = ERequestState.Running;
            }
        }        
        
        public bool IsRequestRunning()
        {
            return RequestState == ERequestState.Running;
        }

        public Error.EType GetErrorBlockingDownload()
        {
            Error.EType returnValue = Error.EType.None;
            if (CRCMismatchErrorTimes >= sm_config.GetMaxTimesPerSessionPerErrorType(Error.EType.Internal_CRC_Mismatch))
            {
                returnValue = Error.EType.Internal_Too_Many_CRC_Mismatches;
            }

            return returnValue;
        }

        public bool CanAutomaticDownload(bool simulation)
        {
            bool returnValue = GetErrorBlockingDownload() == Error.EType.None;
            if (returnValue)
            {
                returnValue = (simulation) ? HasSimulationExpired() : HasErrorExpired();
            }

            return returnValue;
        }        

        // Used only for tracking purposes
        public void SimulateDownload()
        {
            m_latestSimulationAt = sm_realtimeSinceStartup;
            TrackDownloadStart();
            TrackDownloadEnd(Error.EType.Network_Unauthorized_Reachability);
        }        

        public void OnDownloadStart()
        {
            if (State == EState.InQueueForDownload)
            {                
                State = EState.Downloading;
            }
        }

        public void OnDownloadFinish(Error error)
        {
            if (State == EState.Downloading)
            {
                if (error == null || m_manifest.Size == m_dataInfo.Size)
                {
                    CalculatingCRCFromADownload = true;
                    State = EState.CalculatingCRC;                    
                }
                else
                {                    
                    State = EState.InQueueForDownload;
                    OnDownloadEnd(error.Type);
                }

                NotifyError(error);           
            }
        }

        /// <summary>
        /// This method is called when the download is complete and verified if everything went ok or when an error happened
        /// </summary>        
        private void OnDownloadEnd(Error.EType errorType)
        {
            if (errorType == Error.EType.None)
            {
                m_manifest.DownloadedTimes++;
            }

            if (sm_onDownloadEndCallback != null)
            {
                sm_onDownloadEndCallback(this, errorType);
            }

            TrackDownloadEnd(errorType);
        }

        private void TrackDownloadStart()
        {
            if (sm_tracker != null)
            {
                sm_tracker.NotifyDownloadStart(sm_realtimeSinceStartup, Id, DataInfo.Size, m_manifest.Size, sm_currentNetworkReachability, HasBeenDownloadedBefore());
            }
        }

        private void TrackDownloadEnd(Error.EType errorType)
        {
            if (sm_tracker != null)
            {
                sm_tracker.NotifyDownloadEnd(sm_realtimeSinceStartup, Id, DataInfo.Size, m_manifest.Size, sm_currentNetworkReachability, errorType);
            }
        }

        public bool IsAvailable(bool checkDisk)
        {
            bool returnValue = State == EState.Available;
            if (returnValue && checkDisk)
            {
                Error error;
                returnValue = sm_disk.File_Exists(Disk.EDirectoryId.Downloads, Id, out error);
            }

            // If the state is not Available anymore then it's reseted
            if (!returnValue && State == EState.Available)
            {
                State = EState.ReadingDataInfo;
            }

            return returnValue;
        }

        public Error DeleteDownload()
        {
            Error error;
            if (sm_disk.File_Exists(Disk.EDirectoryId.Downloads, Id, out error))
            {
                // Deletes the download corresponding to this entry, if it exists, as it's outdated
                sm_disk.File_Delete(Disk.EDirectoryId.Downloads, Id, out error);
            }
            
            if (State == EState.Downloading)
            {
                m_dataInfo.Size = 0;
            }
            else
            {                
                // Updates its state if needed
                IsAvailable(true);
            }

            return error;
        }

        public void AddGroup(CatalogGroup group)
        {
            if (group != null)
            {
                if (Groups == null)
                {
                    Groups = new HashSet<CatalogGroup>();
                }

                Groups.Add(group);
            }
        }        
        
        public bool BelongsToAnyGroup()
        {
            return Groups != null && Groups.Count > 0;
        }

        public bool GetPermissionRequested()
        {
            if (Groups != null)
            {
                // If the permission has been requested for any groups then it's requested
                foreach (CatalogGroup group in Groups)
                {
                    if (group.PermissionRequested)
                        return true;
                }
            }

            return false;
        }

        public bool GetPermissionOverCarrierGranted()
        {            
            if (Groups != null)
            {
                // If the permission has been granted for any groups then it's granted
                foreach (CatalogGroup group in Groups)
                {
                    if (group.PermissionOverCarrierGranted)
                        return true;
                }                
            }

            return false;
        }
    }    
}
