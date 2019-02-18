using SimpleJSON;
using System.IO;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for storing a downloadable entry status
    /// </summary>
    public class CatalogEntryStatus
    {
        public static float TIME_TO_WAIT_AFTER_ERROR = 3f;
        public static float TIME_TO_WAIT_BETWEEN_SAVES = 3f;
        private static float BYTES_TO_MB = 1024 * 1024;

        public static Disk sm_disk;
        private static CatalogEntry sm_entryHelper = new CatalogEntry();

        public string Id { get; private set; }        

        public enum EState
        {
            None,
            ReadingManifest,
            ReadingDataInfo,
            DownloadingData,
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
                m_state = value;

                // Latest error and save are reseted so the income state will be executed and will be able to save 
                // data right away
                m_latestErrorAt = -1f;
                m_latestSaveAt = -1f;

                switch (m_state)
                {
                    case EState.ReadingDataInfo:
                        m_dataInfo.Size = 0;
                        m_dataInfo.CRC = 0;
                        break;
                }
            }
        }

        private CatalogEntryManifest m_manifest;
        private CatalogEntry m_dataInfo;
        public CatalogEntry DataInfo { get { return m_dataInfo; } }

        private float m_latestErrorAt;
        private float m_latestSaveAt;

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
        }

        public void LoadManifest(string id, JSONNode json)
        {
            Id = id;
            m_manifest.Load(json);
            State = EState.ReadingManifest;
        }
        
        public void Update()
        {
            bool canUpdate = true;            
            if (m_latestErrorAt >= 0f)
            {
                float timeSinceLatestError = Time.realtimeSinceStartup - m_latestErrorAt;
                canUpdate = timeSinceLatestError >= TIME_TO_WAIT_AFTER_ERROR;
            }

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

                    case EState.DownloadingData:
                        break;

                    case EState.CalculatingCRC:
                        ProcessCalculatingCRC();
                        break;

                    case EState.DealingWithCRCMismatch:
                        ProcessDealingWithCRCMismatch();
                        break;
                }
            }

            if (m_manifest.NeedsToSave)
            {
                bool canSave = true;

                if (m_latestSaveAt >= 0f)
                {
                    float timeSinceLatestSave = Time.realtimeSinceStartup - m_latestSaveAt;
                    canSave = timeSinceLatestSave >= TIME_TO_WAIT_BETWEEN_SAVES;                    
                }

                if (canSave)
                {
                    Save();
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
            }
        }

        public CatalogEntryManifest GetManifest()
        {
            return m_manifest;
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
                m_latestErrorAt = Time.realtimeSinceStartup;
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
                            State = EState.DownloadingData;
                        }
                    }
                    else
                    {
                        State = EState.DownloadingData;
                    }
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
                            State = EState.DealingWithCRCMismatch;
                        }
                    }
                }
                else
                {
                    State = EState.ReadingDataInfo;
                }
            }            
        }

        private void ProcessDealingWithCRCMismatch()
        {
            Error error = null;
            sm_disk.File_Delete(Disk.EDirectoryId.Downloads, Id, out error);
            if (error == null)
            {
                State = EState.ReadingDataInfo;
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

        public float GetMbLeftToDownload()
        {
            return GetBytesLeftToDownload() / BYTES_TO_MB;
        }

        public bool Compare(CatalogEntryStatus other)
        {
            return other != null && other.m_manifest.Compare(other.m_manifest) && m_dataInfo.Compare(other.m_dataInfo);
        }
    }
}
