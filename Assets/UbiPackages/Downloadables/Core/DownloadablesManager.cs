﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for downloading remote assets (downloadables), storing them in disk and retrieving them on demand.
    /// </summary>
    public class Manager
    {
        public static JSONNode GetCatalogFromAssetsLUT(JSONNode assetsLUTJson)
        {
            JSONNode returnValue = null;
            if (assetsLUTJson != null)
            {
                Catalog downloadablesCatalog = new Catalog();

                Catalog assetsLUTCatalog = new Catalog();
                assetsLUTCatalog.Load(assetsLUTJson, null);

#if UNITY_EDITOR
                string runtimePlatform = UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString();
#else
                string runtimePlatform = (Application.platform == RuntimePlatform.Android) ? "Android" : "iOS";
#endif
                string prefix = "AssetBundles/" + runtimePlatform + "/";
            
                downloadablesCatalog.UrlBase = assetsLUTCatalog.UrlBase;

                string  key = "release";
                if (assetsLUTJson.ContainsKey(key))
                {
                    downloadablesCatalog.UrlBase += assetsLUTJson[key] + "/";
                }

                downloadablesCatalog.UrlBase += prefix;

                // Deletes all asset bundle entries because we are going to reenter them
                Dictionary<string, CatalogEntry> entries = assetsLUTCatalog.GetEntries();
                foreach (KeyValuePair<string, CatalogEntry> pair in entries)
                {
                    if (pair.Key.Contains(prefix))
                    {
                        downloadablesCatalog.AddEntry(pair.Key.Replace(prefix, ""), pair.Value);
                    }
                }

                returnValue = downloadablesCatalog.ToJSON();                
            }

            return returnValue;
        }       
             
        public static readonly string DESKTOP_DEVICE_STORAGE_PATH_SIMULATED = "DeviceStorageSimulated/";

        public static readonly string DOWNLOADABLES_FOLDER_NAME = "Downloadables";

        public static readonly string MANIFESTS_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Metadata");
        public static readonly string MANIFESTS_ROOT_PATH = FileUtils.GetDeviceStoragePath(MANIFESTS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

        public static readonly string DOWNLOADS_FOLDER_NAME = Path.Combine(DOWNLOADABLES_FOLDER_NAME, "Downloads");
        public static readonly string DOWNLOADS_ROOT_PATH = FileUtils.GetDeviceStoragePath(DOWNLOADS_FOLDER_NAME, DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
        public static readonly string DOWNLOADS_ROOT_PATH_WITH_SLASH = DOWNLOADS_ROOT_PATH + "/";

        private bool IsInitialized { get; set; }

        private NetworkDriver m_network;
        private Disk m_disk;
        private Cleaner m_cleaner;
        private Downloader m_downloader;
        private Tracker m_tracker;

        /// <summary>
        /// When <c>true</c> all downloads will be downloaded automatically. Otherwise a downloadable will be downloaded only on demand (by calling Request)
        /// </summary>
        public bool IsAutomaticDownloaderEnabled { get; set; }
        
        public Manager(NetworkDriver network, DiskDriver diskDriver, Disk.OnIssue onDiskIssueCallbak, Tracker tracker, Logger logger)
        {
            sm_logger = logger;

            m_network = network;
            m_disk = new Disk(diskDriver, MANIFESTS_ROOT_PATH, DOWNLOADS_ROOT_PATH, 180, onDiskIssueCallbak);
            m_tracker = tracker;

            CatalogEntryStatus.StaticSetup(m_disk, tracker);
            m_cleaner = new Cleaner(m_disk, 180);            
            m_downloader = new Downloader(network, m_disk, logger);

            Reset();
        }

        public void Reset()
        {
            IsInitialized = false;
            IsAutomaticDownloaderEnabled = false;
            m_cleaner.Reset();
            Catalog_Reset();
            m_downloader.Reset();        
        }

        public void Initialize(JSONNode catalogJSON, bool isAutomaticDownloaderEnabled)
        {
            Reset();          

            if (CanLog())
            {                
                Log("Initializing Downloadables manager..." );
            }
            
            ProcessCatalog(catalogJSON);

            IsInitialized = true;
            IsAutomaticDownloaderEnabled = isAutomaticDownloaderEnabled;

            if (catalogJSON != null)
            {
                string urlBase = catalogJSON[Catalog.CATALOG_ATT_URL_BASE];
                m_downloader.Initialize(urlBase);
            }
        } 

        private void ProcessCatalog(JSONNode catalogJSON)
        {    
            if (catalogJSON != null)
            {                
                JSONClass assets = (JSONClass)catalogJSON[Catalog.CATALOG_ATT_ENTRIES];
                if (assets != null)
                {
                    List<string> ids = new List<string>();

                    string id;
                    ArrayList keys = assets.GetKeys();
                    int count = keys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        id = (string)keys[i];
                        ids.Add(id);

                        Catalog_AddEntryStatus(id, assets[id]);                        
                    }

                    if (ids.Count > 0)
                    {
                        m_cleaner.CleanAllExcept(ids);
                    }
                }
            }            
        }                 
        
        public bool IsIdAvailable(string id, bool track = false)
        {
            bool returnValue = false;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry != null && entry.State == CatalogEntryStatus.EState.Available);                

                if (track)
                {
                    float existingSize = GetIdMbDownloadedSoFar(id);
                    NetworkReachability reachability = m_network.CurrentNetworkReachability;
                    Error.EType result = (returnValue) ? Error.EType.None : Error.EType.NotAvailable;
                    m_tracker.TrackActionEnd(Tracker.EAction.Load, id, existingSize, existingSize, GetIdTotalMb(id), 0, reachability, reachability, result, false);
                }
            }

            return returnValue;
        }

        public bool IsIdsListAvailable(List<string> ids, bool track = false)
        {
            bool returnValue = true;
            if (ids != null)
            {
                int count = ids.Count;
                for (int i = 0; i < count && returnValue; i++)
                {
                    // IsIdAvailable() must be called for every id so that the result will be tracked if it needs to
                    returnValue = IsIdAvailable(ids[i], track) && returnValue;
                }
            }

            return returnValue;
        }

        private float GetIdMbDownloadedSoFar(string id)
        {
            float returnValue = float.MaxValue;
            if (IsInitialized)
            {
                returnValue = GetIdTotalMb(id) - GetIdMbLeftToDownload(id);                
            }

            return returnValue;
        }

        public float GetIdMbLeftToDownload(string id)
        {           
            float returnValue = float.MaxValue;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry == null) ? 0 : entry.GetMbLeftToDownload();
            }

            return returnValue;
        }

        public float GetIdTotalMb(string id)
        {
            float returnValue = float.MaxValue;
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                returnValue = (entry == null) ? 0 : entry.GetTotalMb();
                if (entry != null)
                {
                    entry.GetTotalMb();
                }                
            }

            return returnValue;
        }

        public string GetPathToDownload(string id)
        {
            return DOWNLOADS_ROOT_PATH_WITH_SLASH + id;
        }

        /// <summary>
        /// Request a downloadable with <c>id</c> as an identifier to be downloaded.
        /// </summary>
        /// <param name="id">Downloadable identifier to download</param>
        public void RequestId(string id)
        {
            if (IsInitialized)
            {
                CatalogEntryStatus entry = Catalog_GetEntryStatus(id);
                if (entry == null && CanLog())
                {
                    LogError("Downloadable id " + id + " requested but it doesn't exist in catalog");
                }
                else if (entry.CanBeRequested())
                {
                    entry.Request();
                }
            }
        }

        /// <summary>
        /// Request a list of downloadables to be downloaded.
        /// </summary>
        /// <param name="id">List of downloadable identifiers to download</param>
        public void RequestIdList(List<string> ids)
        {
            if (IsInitialized)
            {
                if (ids != null)
                {
                    int count = ids.Count;
                    for (int i = 0; i < count; i++)
                    {
                        RequestId(ids[i]);
                    }
                }
            }
        }

        public List<string> GetIds()
        {
            List<string> returnValue = new List<string>();
            foreach (KeyValuePair<string, CatalogEntryStatus> pair in m_catalog)
            {
                returnValue.Add(pair.Key);
            }

            return returnValue;
        }

        public void Update()
        {
            if (IsInitialized)
            {
                m_downloader.CurrentNetworkReachability = m_downloader.NetworkDriver.CurrentNetworkReachability;
                m_disk.Update();
                m_cleaner.Update();
                Catalog_Update();                
            }
        }        

#region catalog
        private Dictionary<string, CatalogEntryStatus> m_catalog;

        private void Catalog_Reset()
        {            
            if (m_catalog == null)
            {
                m_catalog = new Dictionary<string, CatalogEntryStatus>();
            }
            else
            {
                m_catalog.Clear();
            }
        }

        private void Catalog_AddEntryStatus(string id, JSONNode json)
        {
            CatalogEntryStatus entry = new CatalogEntryStatus();
            entry.LoadManifest(id, json);
            m_catalog.Add(id, entry);
        }        

        public CatalogEntryStatus Catalog_GetEntryStatus(string id)
        {
            CatalogEntryStatus entry = null;
            m_catalog.TryGetValue(id, out entry);
            return entry;
        }  

        public bool Catalog_ContainsEntryStatus(string id)
        {
            return m_catalog.ContainsKey(id);            
        }
        
        public Dictionary<string, CatalogEntryStatus> Catalog_GetEntryStatusList()
        {
            return m_catalog;
        }

        private void Catalog_Update()
        {
            CatalogEntryStatus.StaticUpdate(Time.realtimeSinceStartup, m_network.CurrentNetworkReachability);

            bool canDownload = !m_downloader.IsDownloading;
            CatalogEntryStatus entryToDownload = null;

            foreach (KeyValuePair<string, CatalogEntryStatus> pair in m_catalog)
            {
                pair.Value.Update();
                if (canDownload && pair.Value.State == CatalogEntryStatus.EState.InQueueForDownload && 
                    m_downloader.ShouldDownloadWithCurrentConnection(pair.Value))
                {
                    if (entryToDownload == null || !entryToDownload.IsRequestRunning())
                    {
                        if (pair.Value.IsRequestRunning())
                        {
                            entryToDownload = pair.Value;
                        }
                        else if (IsAutomaticDownloaderEnabled && pair.Value.CanAutomaticDownload())
                        {
                            entryToDownload = pair.Value;
                        }                        
                    }                    
                }
            }            

            if (entryToDownload != null)
            {                
                m_downloader.StartDownloadThread(entryToDownload);
            }
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
