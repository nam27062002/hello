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
        private static int TIMEOUT = 10000;

        private string m_urlBase;
        private Disk m_disk;
        public NetworkDriver NetworkDriver { get; set; }        
        private Logger m_logger;

        private Thread m_downloadThread = null;

        private NetworkReachability CurrentNetworkReachability { get; set; }
        private int ThrottleSleepTime { get; set; }

        public Downloader(NetworkDriver networkDriver, Disk disk, Logger logger)
        {
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
            if (IsDownloading)
            {
                m_downloadThread.Abort();
            }

            m_downloadThread = null;
            m_urlBase = null;
        }        

        public bool IsDownloading { get { return m_downloadThread != null && m_downloadThread.IsAlive; } }

        public bool ShouldDownloadWithCurrentConnection(CatalogEntryStatus entryStatus)
        {
            return GetErrorTypeIfDownloadWithCurrentConnection(entryStatus) == Error.EType.None;
        }

        public Error.EType GetErrorTypeIfDownloadWithCurrentConnection(CatalogEntryStatus entryStatus)
        {
            Error.EType returnValue = Error.EType.None;
            switch (CurrentNetworkReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    // Wifi = Always Ok                    
                    break;

                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    // Over the carrier only if the user has granted permission
                    if (!entryStatus.GetPermissionOverCarrierGranted())
                    {
                        returnValue = Error.EType.Network_Unauthorized_Reachability;
                    }
                    break;

                default:
                    returnValue = Error.EType.Network_No_Reachability;
                    break;
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

                entryStatus.OnDownloadStart();
                m_downloadThread = new Thread(() => DoDownload(entryStatus));
                m_downloadThread.Start();                
            }
        }

        private void DoDownload(CatalogEntryStatus entryStatus)
        {                                    
            FileStream saveFileStream = null;
            Error error = null;            

            string fileName = entryStatus.Id;            
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

                    string downloadURL = m_urlBase + fileName;

                    if (CanLog())
                    {
                        Log("AssetBundler DoDownload: Resuming incomplete DL. " + existingLength + " bytes downloaded already. URL = " + downloadURL);
                    }                    

                    HttpWebRequest request = NetworkDriver.CreateHttpWebRequest(downloadURL);
                    request.Proxy = NetworkManager.SharedInstance.GetCurrentProxySettings();
                    request.Timeout = TIMEOUT;
                    request.ReadWriteTimeout = TIMEOUT;
                    request.AddRange((int)existingLength);//, (int)entryStatus.GetTotalBytes());

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
                            m_disk.File_Delete(Disk.EDirectoryId.Downloads, fileName, out error);
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

                                while (totalReceived < serverFileSize)
                                {

                                    //#if !PRODUCTION && !PREPRODUCTION 

                                    //debug throttling logic
                                    int sleepTime = ThrottleSleepTime;
                                    if (sleepTime > 0)
                                    {                                        
                                        Thread.Sleep(sleepTime);
                                    }

                                    /*
                                    int internetThrottleStatus = Assets.Code.Common.DebugOptions.GetFieldValue(Assets.Code.Common.DebugField.spoofSlowInternet);
                                    if (internetThrottleStatus == 1)
                                    {
                                        //16ms sleep =  250kb/sec max speed
                                        Thread.Sleep(16);
                                    }
                                    else if (internetThrottleStatus == 2)
                                    {
                                        //80ms sleep =  50kb/sec max speed
                                        Thread.Sleep(80);
                                    }
                                    else if (internetThrottleStatus == 3)
                                    {
                                        //800ms sleep = 5kb/sec max speed
                                        Thread.Sleep(800);
                                    }
                                    */
//#endif

                                    //if connection has downgraded to a non-allowed network, 
                                    //or if user has changed the current prioritised download, this pauses the download
                                    Error.EType errorType = GetErrorTypeIfDownloadWithCurrentConnection(entryStatus);
                                    if (errorType != Error.EType.None
                                        //|| AssetBundleManager.CheckAndResetCurrentPriorityChanged()
                                        //|| AssetBundleManager.CheckAndResetDownloadTimeout())
                                        )
                                    {
                                        error = new Error(errorType);                                        
                                        //AssetBundleManager.Instance.ResetPollingTime();  //we reset the poll to start the next download immediately
                                        stopwatch.Stop();
                                        stream.Close();
                                        return;
                                    }

                                    int byteSize = stream.Read(downBuffer, 0, downBuffer.Length);
                                    m_disk.DiskDriver.File_Write(saveFileStream, downBuffer, 0, byteSize);
                                    totalReceived += byteSize;
                                    sessionReceived += byteSize;

                                    //float currentSpeed = sessionReceived / (float)stopwatch.Elapsed.TotalSeconds;
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
                        LogError("AssetBundler DoDownload: Exception caused assetbundle download failure. Performing full file/CRC to work out file status: " + we.ToString());
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