using System.Collections.Generic;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for handling the downloading of a collection of downloadables, which may belong to
    /// one or several downloadable groups.
    /// </summary>
    public class ProductionHandle : Handle
    {
        private HashSet<string> GroupIds { get; set; }
        private List<string> DownloadableIds { get; set; }                        
        
        public void Setup(string groupId, List<string> downloadableIds)
        {
            GroupIds = new HashSet<string>();
            GroupIds.Add(groupId);

            Setup(GroupIds, downloadableIds);
        }

        public void Setup(HashSet<string> groupIds, List<string> downloadableIds)
        {
            GroupIds = groupIds;
            DownloadableIds = downloadableIds;
        }

        public override bool IsAvailable()
        {
            // It's available if all downloadables are available
            bool returnValue = true;
            if (DownloadableIds != null)
            {
                int count = DownloadableIds.Count;
                for (int i = 0; i < count && returnValue; i++)
                {
                    returnValue = sm_manager.IsIdAvailable(DownloadableIds[i]);                    
                }
            }

            return returnValue;
        }

        public override void SetIsPermissionRequested(bool value)
        {
            if (GroupIds != null)
            {                
                foreach (string groupId in GroupIds)
                {
                    sm_manager.Groups_SetIsPermissionRequested(groupId, value);                    
                }
            }
        }

        public override void SetIsPermissionGranted(bool value)
        {
            if (GroupIds != null)
            {
                foreach (string groupId in GroupIds)
                {
                    sm_manager.Groups_SetIsPermissionGranted(groupId, value);
                }
            }            
        }

        /// <summary>
        /// Returns the size in bytes of this list of downloadables that have been downloaded so far.
        /// </summary>        
        public override long GetDownloadedBytes()
        {
            long returnValue = 0;
            if (DownloadableIds != null)
            {
                int count = DownloadableIds.Count;
                for (int i = 0; i < count; i++)
                {
                    returnValue += sm_manager.GetIdBytesDownloadedSoFar(DownloadableIds[i]);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Returns the total size in bytes of this list of downloadables.
        /// </summary>
        /// <returns></returns>
        public override long GetTotalBytes()
        {
            long returnValue = 0;
            if (DownloadableIds != null)
            {
                int count = DownloadableIds.Count;
                for (int i = 0; i < count; i++)
                {
                    returnValue += sm_manager.GetIdTotalBytes(DownloadableIds[i]);
                }
            }

            return returnValue;
        }

        public override void Retry()
        {
            if (DownloadableIds != null)
            {
                int count = DownloadableIds.Count;
                for (int i = 0; i < count; i++)
                {
                    CatalogEntryStatus entryStatus = sm_manager.Catalog_GetEntryStatus(DownloadableIds[i]);
                    if (entryStatus != null)
                    {
                        entryStatus.ResetLatestError();
                    }
                }
            }
        }

        protected override Error.EType ExtendedGetErrorType()
        {
            // Several errors can happen simultaneously but only the most severe is returned, that's why they need to
            // be checked in order of severity
                     
            // 1. No Internet
            NetworkReachability reachability = sm_manager.GetCurrentNetworkReachability();
            if (reachability == NetworkReachability.NotReachable)
            {
                return Error.EType.Network_No_Reachability;
            }

            // 2. On carrier connection but no permission granted
            if (reachability == NetworkReachability.ReachableViaCarrierDataNetwork && !GetIsPermissionGranted())
            {
                return Error.EType.Network_Unauthorized_Reachability;
            }

            // 3. Downloading is disabled
            if (!sm_manager.IsEnabled)
            {
                return Error.EType.Internal_Download_Disabled;
            }

            // 4. Automatic downloading is disabled
            if (!sm_manager.IsAutomaticDownloaderEnabled)
            {
                return Error.EType.Internal_Automatic_Download_Disabled;
            }
            
            // 1., 2., 3. and 4. can be detected without looping through every downloadable. 
            // Now we need to go through every downloadable error and process the most severe one            
            return GetMostSevereErrorTypeInDownloadables();
        }

        protected override bool ExtendedNeedsToRequestPermission()
        {    
            // If any groups need to request permission then the handle needs to request permission
            if (GroupIds != null)
            {
                foreach (string groupId in GroupIds)
                {
					if (!sm_manager.Groups_GetPermissionRequested(groupId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool ExtendedGetIsPermissionGranted()
        {
            // The permission is granted for the handle if the permission has been granted for all groups
            if (GroupIds != null)
            {
                foreach (string groupId in GroupIds)
                {                    
                    if (!sm_manager.Groups_GetIsPermissionGranted(groupId))
                    {
                        return false;
                    }
                }
            }            

            return true;
        }        

        private Error.EType GetMostSevereErrorTypeInDownloadables()
        {
            Error.EType returnValue = Error.EType.None;
            if (DownloadableIds != null)
            {
                Error.EType errorType;
                CatalogEntryStatus entryStatus;
                int count = DownloadableIds.Count;
                bool isAnyDownloading = false;
                for (int i = 0; i < count; i++)
                {
                    entryStatus = sm_manager.Catalog_GetEntryStatus(DownloadableIds[i]);
                    if (entryStatus != null)
                    {
                        if (entryStatus.State == CatalogEntryStatus.EState.Downloading)
                        {
                            isAnyDownloading = true;
                        }

                        errorType = entryStatus.GetErrorBlockingDownload();

                        if (errorType == Error.EType.None && entryStatus.LatestError != null)
                        {                            
                            errorType = entryStatus.LatestError.Type;
                        }

                        if (errorType != Error.EType.None)
                        {
                            if (returnValue == Error.EType.None)
                            {
                                returnValue = errorType;
                            }
                            else if (ErrorTypeToEError(errorType) < ErrorTypeToEError(returnValue))
                            {
                                returnValue = errorType;
                            }
                        }                        
                    }
                }

                // No error is notified if there's some progress
                if (isAnyDownloading)
                {
                    returnValue = Error.EType.None;
                }

            }
            
            return returnValue;
        }

        public override float GetSpeed()
        {
            // We only take into consideration the downloading speed if it's being used to download any of the downloadable ids handled by this handle
            return (sm_manager.IsAnyIdBeingDownloaded(DownloadableIds)) ? sm_manager.GetSpeed() : 0f;            
        }
    }
}
