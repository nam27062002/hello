using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for downloading a downloadable
    /// </summary>
    public class Downloader
    {
        // Due to strict Apple Store compliance guidelines the user needs to grant permission to download over wifi too
#if UNITY_IOS
        private static bool REQUEST_PERMISSION_OVER_WIFI_ENABLED = true;
#else
        private static bool REQUEST_PERMISSION_OVER_WIFI_ENABLED = false;
#endif    

        public static bool USE_CRC_IN_URL = true;

        // The default value of HttpWebRequest.Timeout is 100 seconds. That's the value that we want to use since all asset bundles are downloaded from the same source, so if one triggers timeout
        // then all will. It must be bigger than 15 seconds since according to documentation (https://docs.microsoft.com/en-us/dotnet/api/system.net.httpwebrequest.timeout?view=netframework-4.8#System_Net_HttpWebRequest_Timeout) 
        // DNS name resolution can take up to 15 seconds to return or timeout.
        private static int TIMEOUT = 100000; 

        private Manager m_manager;
        private string m_urlBase;
        private Disk m_disk;
        public NetworkDriver NetworkDriver { get; set; }        
        private Logger m_logger;

        private Thread m_downloadThread = null;

        private NetworkReachability CurrentNetworkReachability { get; set; }
        private int ThrottleSleepTime { get; set; }

        private CatalogEntryStatus m_currentEntryStatus;

        public Downloader(Manager manager, NetworkDriver networkDriver, Disk disk, Logger logger)
        {
            m_manager = manager;
            NetworkDriver = networkDriver;
            m_disk = disk;            
            m_logger = logger;
            CurrentNetworkReachability = NetworkReachability.NotReachable;
            ThrottleSleepTime = 0;
        }

        public void Initialize(string urlBase)
        {
            m_urlBase = urlBase;
        }

        public bool IsInitialized()
        {
            return m_urlBase != null;
        }

        public void Reset()
        {
            AbortDownload();             
            m_urlBase = null;
        }        

        public void AbortDownload()
        {
            if (IsDownloading)
            {
                if (m_currentEntryStatus != null)
                {
                    m_currentEntryStatus.OnDownloadFinish(new Error(Error.EType.Internal_Download_Aborted));
                    m_currentEntryStatus = null;
                }

                m_downloadThread.Abort();
                m_downloadThread = null;
            }
        }

        public bool IsDownloading { get { return m_downloadThread != null && m_downloadThread.IsAlive; } }

        public bool ShouldDownloadWithCurrentConnection(CatalogEntryStatus entryStatus)
        {
            return GetErrorTypeIfDownloadWithCurrentConnection(entryStatus) == Error.EType.None;
        }
        
        private Error.EType GetErrorTypeWhileDownloading(CatalogEntryStatus entryStatus)
        {
            //if connection has downgraded to a non-allowed network, 
            //or if downloading is not allowed
            Error.EType returnValue = GetErrorTypeIfDownloadWithCurrentConnection(entryStatus);
            if (returnValue == Error.EType.None)
            {
                if (m_manager != null && !m_manager.IsEnabled)
                {
                    returnValue = Error.EType.Internal_Download_Disabled;
                }
            }

            return returnValue;
        }

        public bool IsDownloadAllowed(bool permissionRequested, bool permissionOverCarrierGranted)        
        {
            bool returnValue = true; ;
            switch (CurrentNetworkReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    if (REQUEST_PERMISSION_OVER_WIFI_ENABLED)
                    {
                        if (!permissionRequested)
                        {
                            returnValue = false;
                        }
                    }
                    break;

                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    // Over the carrier only if the user has granted permission
                    if (!permissionOverCarrierGranted)
                    {
                        returnValue = false;
                    }
                    break;                
            }

            return returnValue;
        }

        public Error.EType GetErrorTypeIfDownloadWithCurrentConnection(CatalogEntryStatus entryStatus)
        {
            Error.EType returnValue = Error.EType.None;
            if (CurrentNetworkReachability == NetworkReachability.NotReachable)
            {
                returnValue = Error.EType.Network_Unauthorized_Reachability;
            }
            else
            {
                if (!IsDownloadAllowed(entryStatus.GetPermissionRequested(), entryStatus.GetPermissionOverCarrierGranted()))
                {
                    returnValue = Error.EType.Network_Unauthorized_Reachability;
                }
            }
           
            return returnValue;                        
        }

        public void StartDownloadThread(CatalogEntryStatus entryStatus)
        {           
            if (IsInitialized() && entryStatus != null)
            {
                if (CanLog())
                {
                    Log("Downloader Starting Download: " + entryStatus.Id);
                }

                m_currentEntryStatus = entryStatus;
                entryStatus.OnDownloadStart();
                m_downloadThread = new Thread(() => DoDownload(entryStatus));
                m_downloadThread.Start();
                m_currentEntryStatus = null;
            }
        }

        private void DoDownload(CatalogEntryStatus entryStatus)
        {                                    
            FileStream saveFileStream = null;
            Error error = null;            

            string fileName = entryStatus.Id;
            string downloadURL = m_urlBase;

            try
            {
                // Checks if the downloads directory has to be created                
                if (!m_disk.Directory_Exists(Disk.EDirectoryId.Downloads, out error))
                {
                    if (error == null)
                    {
                        m_disk.Directory_CreateDirectory(Disk.EDirectoryId.Downloads, out error);
                    }
                }

                if (error == null)
                {                    
                    long existingLength = 0;

                    FileInfo fileInfo = m_disk.File_GetInfo(Disk.EDirectoryId.Downloads, fileName, out error);

                    if (error != null)
                    {
                        return;
                    }

                    if (fileInfo.Exists)
                    {
                        existingLength = fileInfo.Length;
                    }
                    
                    if (Manager.USE_CRC_IN_URL && USE_CRC_IN_URL)
                    {
                        downloadURL += entryStatus.GetManifest().CRC + "/";
                    }
                    downloadURL += fileName;

                    if (CanLog())
                    {
                        Log("AssetBundler DoDownload: Resuming incomplete DL. " + existingLength + " bytes downloaded already. URL = " + downloadURL);
                    }                    

                    HttpWebRequest request = NetworkDriver.CreateHttpWebRequest(downloadURL);
                    request.Proxy = NetworkManager.SharedInstance.GetCurrentProxySettings();
                    request.Timeout = TIMEOUT;
                    request.ReadWriteTimeout = TIMEOUT;
                    request.AddRange((int)existingLength, (int)entryStatus.GetTotalBytes());

                    if (CanLog())
                    {
                        Log("Request URI: " + request.RequestUri.AbsoluteUri);
                    }

                    bool didComplete = false;
                    using (HttpWebResponse response = (HttpWebResponse)NetworkDriver.GetResponse(request))
                    {
                        long contentLength = NetworkDriver.GetResponseContentLength(response);
                        if (contentLength == 0)
                        {
                            //no more to download from the server
                            if (existingLength >= entryStatus.GetTotalBytes())
                            {
                                didComplete = true;
                            }
                            else
                            {
                                if (CanLog())
                                {
                                    Log("Error. Data on server not sufficient: " + existingLength + " bytes available, expected " + entryStatus.GetTotalBytes());
                                }

                                error = new Error(Error.EType.Network_Server_Size_Mismatch);
                                return;
                            }
                        }                        

                        bool downloadResumable; // need it for not sending any progress
                        int responseCode = NetworkDriver.GetResponseStatusCodeAsInt(response);

                        if (responseCode == 206) //same as: response.StatusCode == HttpStatusCode.PartialContent
                        {
                            if (CanLog())
                            {
                                Log("AssetBundler DoDownload: " + downloadURL + " is resumable (206 status)");
                            }

                            downloadResumable = true;
                        }
                        else if (responseCode >= 200 && responseCode <= 299) //2xx is success
                        {
                            if (CanLog())
                            {
                                Log("AssetBundler DoDownload: " + downloadURL + " is not resumable (" + responseCode + ")");
                            }

                            existingLength = 0;
                            downloadResumable = false;

                            // Wipe any copy of the file, as it's not resumable. Don't consider this a failure, just start downloading it anew
                            error = entryStatus.DeleteDownload();                            
                            if (error != null)
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (CanLog())
                            {
                                Log("AssetBundler DoDownload: file not accessible. Status code: " + responseCode);
                            }

                            error = new Error(Error.EType.Network_Web_Exception_No_Access_To_Content, "Status Code: " + responseCode);
                            return;                            
                        }

                        using (saveFileStream = m_disk.DiskDriver.File_Open(fileInfo, downloadResumable ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                stream.ReadTimeout = 5000;

                                byte[] downBuffer = new byte[4096];
                                long totalReceived = existingLength;
                                long sessionReceived = 0;
                                Stopwatch stopwatch = Stopwatch.StartNew();

                                long serverFileSize = existingLength + contentLength; //response.ContentLength gives me the size that is remaining to be downloaded

                                long latestBytesReceivedAt = stopwatch.ElapsedMilliseconds;
                                while (totalReceived < serverFileSize)
                                {
                                    //debug throttling logic
                                    int sleepTime = ThrottleSleepTime;
                                    if (sleepTime > 0)
                                    {                                        
                                        Thread.Sleep(sleepTime);
                                    }                                 
                                                                        
                                    // Check if something requires the download to be paused
                                    Error.EType errorType = GetErrorTypeWhileDownloading(entryStatus);

                                    // If no error has been found then checks data reception timeout 
                                    if (errorType == Error.EType.None &&
                                        stopwatch.ElapsedMilliseconds - latestBytesReceivedAt > TIMEOUT)
                                    {
                                        errorType = Error.EType.Network_Web_Exception_Timeout;
                                    }

                                    if (errorType != Error.EType.None
                                        //|| AssetBundleManager.CheckAndResetCurrentPriorityChanged()                                        
                                        )
                                    {
                                        error = new Error(errorType);                                        
                                        //AssetBundleManager.Instance.ResetPollingTime();  //we reset the poll to start the next download immediately
                                        stopwatch.Stop();
                                        stream.Close();
                                        return;
                                    }

                                    int byteSize = stream.Read(downBuffer, 0, downBuffer.Length);

                                    if (byteSize > 0)
                                    {
                                        m_disk.DiskDriver.File_Write(saveFileStream, downBuffer, 0, byteSize);
                                        totalReceived += byteSize;
                                        sessionReceived += byteSize;
                                        latestBytesReceivedAt = stopwatch.ElapsedMilliseconds;
                                    }

                                    if (m_manager != null)
                                    {
                                        float totalSeconds = (float)stopwatch.Elapsed.TotalSeconds;
                                        if (totalSeconds == 0f)
                                        {
                                            totalSeconds = 0.01f;
                                        }

                                        float currentSpeed = sessionReceived / totalSeconds;
                                        m_manager.SetSpeed(currentSpeed);
                                    }
                                    
                                    entryStatus.DataInfo.Size = totalReceived;
                                    //DownloadProgressChanged.Invoke(this, new DownloadProgressChangedEventArgs(assetBundleQueueInfo.AssetBundleName, totalReceived, (long)currentSpeed));
                                }

                                didComplete = true;
                                stopwatch.Stop();
                                stream.Close();
                            }
                        }

                        response.Close();
                        saveFileStream.Close();
                        saveFileStream = null;
                    }

                    if (didComplete)
                    {
                        if (CanLog())
                        {
                            Log("AssetBundler DoDownload. Completed.");
                        }
                        
                        //download completed
                        //AssetBundleStateChanged.Invoke(this, new AssetBundleStateChangedEventArgs(assetBundleQueueInfo.AssetBundleName, AssetBundleDownloadState.CRCCheck));
                        //AssetBundleManager.Instance.ResetPollingTime(); //we reset the poll to start the next download immediately
                    }
                }
            }
            catch (WebException we)
            {                
                //416 error means we requested some bytes which don't exist.
                //this means the local game is expecting more data than the server has.
                //in this case, the file must be finished (no more data on the server), so we throw it to a CRC check to confirm
                /*if ((int)we.Status == 416)
                {
                    if (CanLog())
                    {
                        LogWarning("AssetBundler DoDownload: Error 416. Assuming File is completed");
                    }

                    //this is considered an error for retry purposes                    
                    //AssetBundleStateChanged.Invoke(this, new AssetBundleStateChangedEventArgs(assetBundleQueueInfo.AssetBundleName, AssetBundleDownloadState.CRCCheck, "ERROR_416_FROM_SERVER"));
                }
                else
                */
                {                    
                    if (CanLog())
                    {
                        LogError("AssetBundler DoDownload: Exception caused assetbundle download failure url = " + downloadURL + ". Performing full file/CRC to work out file status: " + we.ToString());
                    }
                    error = new Error(we);                    
                }
            }
            catch (IOException ioe)
            {                
                //Debug.LogException(ioe);
                if (CanLog())
                {
                    LogError("AssetBundler DoDownload: IO/Write Exception. Deleting offending file: " + fileName + ": " + ioe.ToString());
                }
                error = new Error(ioe);                
            }
            catch (ThreadAbortException e)
            {
                if (CanLog())
                {
                    LogWarning("AssetBundler DoDownload: Thread aborted. Assuming intentionally.");
                }
                error = new Error(e);                
            }
            catch (Exception e)
            {                
                if (CanLog())
                LogError("AssetBundler DoDownload: Exception caused assetbundle download failure. Performing full file/CRC to work out file status. Error:  " + e.ToString());             
                error = new Error(e);
            }
            finally
            {
                entryStatus.OnDownloadFinish(error);

                if (saveFileStream != null)
                {
                    saveFileStream.Close();
                    saveFileStream = null;
                }                
            }
        }    

        public void Update()
        {
            // These variables need to be stored so the downloader thread can access them as
            // there's only access to the source values in the main thread
            CurrentNetworkReachability = NetworkDriver.CurrentNetworkReachability;
            ThrottleSleepTime = NetworkDriver.GetThrottleSleepTime();
        }

        private bool CanLog()
        {
            return m_logger != null && m_logger.CanLog();
        }

        private void Log(string msg)
        {
            if (CanLog())
            {
                m_logger.Log(msg);
            }
        }

        private void LogWarning(string msg)
        {
            if (CanLog())
            {
                m_logger.LogWarning(msg);
            }
        }

        private void LogError(string msg)
        {
            if (CanLog())
            {
                m_logger.LogError(msg);
            }
        }
    }
}