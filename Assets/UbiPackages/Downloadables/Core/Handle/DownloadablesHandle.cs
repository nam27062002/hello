﻿using System;
using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for handling the progress of a list of downloadables.
    /// </summary>
    public abstract class Handle
    {
        protected static Manager sm_manager;
        protected static DiskDriver sm_diskDriver;

        public static void StaticSetup(Manager manager, DiskDriver diskDriver)
        {
            sm_manager = manager;
            sm_diskDriver = diskDriver;
        }

        //------------------------------------------------------------------------//
        // CONSTANTS															  //
        //------------------------------------------------------------------------//
        public enum EError
        {
            NO_WIFI,                        // and no data permission granted
            NO_CONNECTION,                  // neither wifi nor data
            STORAGE,
            STORAGE_PERMISSION,            
            AUTOMATIC_DOWNLOAD_DISABLED,    // This case shouldn be protected by flow design as no handle should be used before
                                            // automatic downloading is enabled.
                                            // No message will be prompted to the user but it's useful to have it separately when debugging
            AUTOMATIC_DOWNLOAD_NOT_ALLOWED, // It happens when the same file has been downloaded several times unsuccessfully 
                                            // during the current sesion or timeout since latest error hasn't expired.
                                            // No message will be prompted to the user but it's useful to have it separately when debugging
            UNKNOWN,

            NONE
        }        

        //------------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES												  //
        //------------------------------------------------------------------------//
        public float Progress
        {
            get
            {
                float returnValue = 1f;

                long totalBytes = GetTotalBytes();
                if (!IsAvailable() || totalBytes > 0)
                {
                    returnValue = Mathf.Clamp01((float)GetDownloadedBytes() / (float)totalBytes);
                }

                return returnValue;
            }
        }

        //------------------------------------------------------------------------//
        // METHODS																  //
        //------------------------------------------------------------------------//        

        /// <summary>
        /// Returns whether or not the list of downloadables handled by this class are available.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAvailable();

        /// <summary>
        /// Returns whether or not the user has already been notified about the downloading of the list of downloadables. 
        /// </summary>
        /// <returns></returns>
        public bool NeedsToRequestPermission()
        {
            if (IsAvailable())
            {
                return false;
            }
            else
            {
                return ExtendedNeedsToRequestPermission();
            }
        }
        
        public abstract void SetIsPermissionRequested(bool value);

        /// <summary>
        /// Returns whether or not the user has granted the permission to download this list of downloadables over MobileData network.
        /// </summary>        
        public bool GetIsPermissionGranted()
        {
            if (NeedsToRequestPermission())
            {
                return false;
            }
            else
            {
                return ExtendedGetIsPermissionGranted();
            }
        }

        public abstract void SetIsPermissionGranted(bool value);

        /// <summary>
        /// Returns the size in bytes of this list of downloadables that have been downloaded so far.
        /// </summary>        
        public abstract long GetDownloadedBytes();

        /// <summary>
        /// Returns the total size in bytes of this list of downloadables.
        /// </summary>
        /// <returns></returns>
        public abstract long GetTotalBytes();

        public Error.EType GetErrorType()
        {
            if (IsAvailable())
            {
                return Error.EType.None;
            }
            else
            {
                return ExtendedGetErrorType();
            }
        }

        public int GetErrorCode()
        {
            return (int)GetErrorType();
        }

        /// <summary>
        /// Returns the most important error afecting whe whole downloading process. Returns EError.NONE is everything is ok.
        /// </summary>        
        public EError GetError()
        {
            return ErrorTypeToEError(GetErrorType());
        }

        protected abstract bool ExtendedNeedsToRequestPermission();

        protected abstract bool ExtendedGetIsPermissionGranted();

        protected abstract Error.EType ExtendedGetErrorType();        

        protected Error.EType EErrorToErrorType(EError error)
        {
            Error.EType candidate;
            int count = Error.ErrorTypeValues.Length;
            for (int i = 0; i < count; i++)
            {
                candidate = (Error.EType)Error.ErrorTypeValues.GetValue(i); 
                if (ErrorTypeToEError(candidate) == error)
                {
                    return candidate;
                }
            }

            return Error.EType.Other;
        }

        protected EError ErrorTypeToEError(Error.EType errorType)
        {
            EError returnValue;

            switch (errorType)
            {
                case Error.EType.None:
                    returnValue = EError.NONE;
                    break;

                case Error.EType.Network_No_Reachability:
                    returnValue = EError.NO_CONNECTION;
                    break;

                case Error.EType.Network_Unauthorized_Reachability:
                    returnValue = EError.NO_WIFI;
                    break;

                case Error.EType.Disk_IOException:
                    returnValue = EError.STORAGE;
                    break;

                case Error.EType.Disk_UnauthorizedAccess:
                    returnValue = EError.STORAGE_PERMISSION;
                    break;

                default:
                    returnValue = EError.UNKNOWN;
                    break;
            }

            return returnValue;
        }

        public long GetDiskFreeSpaceBytes()
        {
            return sm_diskDriver.GetFreeSpaceBytes();
        }

        public long GetDiskOverflowBytes()
        {
            long returnValue = GetDiskFreeSpaceBytes() - GetTotalBytes();
            if (returnValue > 0)
            {
                returnValue = 0;
            }
            else
            {
                returnValue = -returnValue;
            }

            return returnValue;
        }

        /// <summary>
        /// Returns downloading speed in bytes/second
        /// </summary>        
        public virtual float GetSpeed()
        {
            return sm_manager.GetSpeed();
        }

        public virtual void Retry() {}

        /// <summary>
        /// To be called manually to refresh group state, progress, errors, etc.
        /// </summary>
        public virtual void Update() {}
    }
}
