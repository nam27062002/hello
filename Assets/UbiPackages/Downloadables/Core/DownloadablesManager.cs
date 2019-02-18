using System.Collections.Generic;
using System.IO;
using SimpleJSON;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for downloading remote assets (downloadables), storing them in disk and retrieving them on demand.
    /// </summary>
    public class Manager
    {
        public static readonly string DESKTOP_DEVICE_STORAGE_PATH_SIMULATED = "DeviceStorageSimulated/";

        public static readonly string DOWNLOADABLES_FOLDER_NAME = "Downloadables";

        public static readonly string MANIFESTS_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Metadata");
        public static readonly string MANIFESTS_ROOT_PATH = FileUtils.GetDeviceStoragePath(MANIFESTS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        public static readonly string DOWNLOADS_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Downloads");
        public static readonly string DOWNLOADS_ROOT_PATH = FileUtils.GetDeviceStoragePath(DOWNLOADS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        private enum EInitializeState
        {
            Not_Initialized,
            Initializing,
            Initialized,
            Error
        };

        private EInitializeState InitializeState { get; set; }

        private Error InitializeError { get; set; }        

        private DiskDriver m_diskDriver;

        private Directory m_manifestsDirectory;

        private Directory m_downloadsDirectory;

        public delegate void OnInitialized(Error error);

        private OnInitialized m_onInitialized;

        public Manager(DiskDriver diskDriver, Logger logger)
        {
            sm_logger = logger;
            m_diskDriver = diskDriver;            

            Reset();
        }

        public void Reset()
        {
            InitializeState = EInitializeState.Not_Initialized;
            InitializeError = null;            

            CatalogStatus_Reset();

            if (m_manifestsDirectory == null)
            {
                m_manifestsDirectory = new Directory(MANIFESTS_ROOT_PATH, m_diskDriver);
            }

            if (m_downloadsDirectory == null)
            {
                m_downloadsDirectory = new Directory(DOWNLOADS_ROOT_PATH, m_diskDriver);
            }
        }

        public void Initialize(JSONNode catalogJSON, OnInitialized onInitialized)
        {
            Reset();          

            if (CanLog())
            {                
                Log("Initializing Downloadables manager..." );
            }

            InitializeState = EInitializeState.Initializing;
            m_onInitialized = onInitialized;

            Error error = ProcessCatalog(catalogJSON);
            OnInitializeDone(error);
        }

        private void OnInitializeDone(Error error)
        {
            if (error == null)
            {
                InitializeState = EInitializeState.Initialized;
            }
            else
            {
                InitializeState = EInitializeState.Error;
                InitializeError = error;
            }

            if (m_onInitialized != null)
            {
                m_onInitialized(InitializeError);
                m_onInitialized = null;
            }
        }

        private Error ProcessCatalog(JSONNode catalogJSON)
        {
            Error returnValue = null;                             
            
            // Updates catalog information in Manifests folder and it also loads these entries in catalogStatus
            returnValue = UpdateManifests(catalogJSON);

            if (returnValue == null)
            {
                returnValue = UpdateDownloads();
            }

            return returnValue;
        } 
        
        private Error UpdateManifests(JSONNode catalogJSON)
        {
            Catalog catalog = new Catalog();
            catalog.Load(catalogJSON, sm_logger);

            Error returnValue;

            // Updates catalog information in Manifests folder and it also loads these entries in catalogStatus
            List<string> fileNames = m_manifestsDirectory.Directory_GetFiles(out returnValue);
            if (returnValue == null)
            {
                Error error = null;

                int count = fileNames.Count;
                bool outdated;
                string id;
                CatalogEntry requestEntry;
                JSONNode json;
                EntryStatus manifestEntryStatus;
                string fileName;                
                for (int i = 0; i < count; i++)
                {
                    fileName = Path.GetFileName(fileNames[i]);
                    id = Path.GetFileNameWithoutExtension(fileNames[i]);

                    // Checks if the current manifest belongs to the request catalog
                    requestEntry = catalog.GetEntry(id);

                    // It's not in the catalog, which means that it won't be used anymore, so it must be deleted
                    if (requestEntry == null)
                    {
                        m_manifestsDirectory.File_Delete(fileName, out error);
                    }
                    else
                    {
                        json = m_manifestsDirectory.File_ReadJSON(fileName, out error);
                        if (json == null)
                        {
                            if (CanLog())
                            {
                                LogError("Invalid json for downloadable " + id + " error = " + ((error == null) ? "null" : error.ToString()));
                            }
                        }

                        manifestEntryStatus = new EntryStatus();
                        manifestEntryStatus.Load(json, error);
                        CatalogStatus_AddEntry(id, manifestEntryStatus);

                        if (error == null)
                        {
                            // Checks if the information stored is outdated
                            outdated = manifestEntryStatus.ManifestEntry.CRC != requestEntry.CRC || manifestEntryStatus.ManifestEntry.Size != requestEntry.Size;

                            if (outdated)
                            {                                
                                // Deletes the downloaded file if it exists as it's outdated
                                if (m_downloadsDirectory.File_Exists(fileName, out error))
                                {
                                    m_downloadsDirectory.File_Delete(fileName, out error);
                                }

                                manifestEntryStatus.ManifestEntry.CRC = requestEntry.CRC;
                                manifestEntryStatus.ManifestEntry.Size = requestEntry.Size;

                                m_manifestsDirectory.File_WriteJSON(fileName, manifestEntryStatus.ToJSON(), out error);                                                      
                            }
                        }
                    }
                }

                // Adds entries in catalog that weren't found in Manifests folder to catalogStatus
                Dictionary<string, CatalogEntry> entries = catalog.GetEntries();
                foreach (KeyValuePair<string, CatalogEntry> pair in entries)
                {
                    if (!m_catalogStatus.ContainsKey(pair.Key))
                    {
                        manifestEntryStatus = new EntryStatus();
                        manifestEntryStatus.Load(pair.Value.ToJSON(), null);
                        CatalogStatus_AddEntry(pair.Key, manifestEntryStatus);
                    }
                }
            }

            return returnValue;
        }

        private Error UpdateDownloads()
        {
            Error returnValue;

            List<string> fileNames = m_downloadsDirectory.Directory_GetFiles(out returnValue);
            if (returnValue == null)
            {
                Error error;
                string id;
                string fileName;
                int count = fileNames.Count;
                FileInfo fileInfo;
                EntryStatus entryStatus;
                long length;
                for (int i = 0; i < count; i++)
                {
                    id = Path.GetFileNameWithoutExtension(fileNames[i]);
                    fileName = Path.GetFileName(fileNames[i]);

                    entryStatus = CatalogStatus_GetEntry(id);

                    // If this id is not supported by the catalog then it needs to be deleted
                    if (entryStatus == null)
                    {
                        m_downloadsDirectory.File_Delete(fileName, out error);
                    }
                    else
                    {
                        fileInfo = m_downloadsDirectory.File_GetInfo(fileName, out error);
                        if (error != null)
                        {
                            entryStatus.DataError = error;
                        }
                        else if (fileInfo != null)
                        {
                            length = fileInfo.Length;

                            // Deletes this file because it's size doesn't match the one required by catalog
                            if (length > entryStatus.ManifestEntry.Size)
                            {
                                m_downloadsDirectory.File_Delete(fileName, out error);
                                length = 0;
                            }

                            entryStatus.DataEntry.Size = length;
                        }
                    }                    
                }
            }

            return returnValue;
        }            

        #region catalog_status
        private Dictionary<string, EntryStatus> m_catalogStatus;

        private void CatalogStatus_Reset()
        {
            if (m_catalogStatus == null)
            {
                m_catalogStatus = new Dictionary<string, EntryStatus>();
            }
            else
            {
                m_catalogStatus.Clear();
            }
        }

        private void CatalogStatus_AddEntry(string id, EntryStatus entry)
        {
            m_catalogStatus.Add(id, entry);
        }

        private EntryStatus CatalogStatus_GetEntry(string id)
        {
            EntryStatus entry = null;
            m_catalogStatus.TryGetValue(id, out entry);
            return entry;
        }

        public Dictionary<string, EntryStatus> CatalogStatus_GetCatalog()
        {
            return m_catalogStatus;
        }
        #endregion

        #region logger
        private static Logger sm_logger;

        public bool CanLog()
        {
            return sm_logger != null && sm_logger.CanLog();
        }

        public void Log(string msg)
        {
            if (CanLog())
            {
                sm_logger.Log(msg);
            }
        }

        public void LogWarning(string msg)
        {
            if (CanLog())
            {
                sm_logger.LogWarning(msg);
            }
        }

        public void LogError(string msg)
        {
            if (CanLog())
            {
                sm_logger.LogError(msg);
            }
        }        
        #endregion
    }
}
