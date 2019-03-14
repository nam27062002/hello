using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for handling the progress of a list of downloadables.
    /// </summary>
    public abstract class Handle
    {
        //------------------------------------------------------------------------//
        // CONSTANTS															  //
        //------------------------------------------------------------------------//
        public enum EError
        {
            NO_WIFI,        // and no data permission granted
            NO_CONNECTION,  // neither wifi nor data
            STORAGE,
            STORAGE_PERMISSION,
            UNKNOWN,

            NONE
        }

        //------------------------------------------------------------------------//
        // MEMBERS AND PROPERTIES												  //
        //------------------------------------------------------------------------//
        public float Progress
        {
            get { return Mathf.Clamp01(GetDownloadedBytes() / GetTotalBytes()); }
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
        public abstract float GetDownloadedBytes();

        /// <summary>
        /// Returns the total size in bytes of this list of downloadables.
        /// </summary>
        /// <returns></returns>
        public abstract float GetTotalBytes();

        /// <summary>
        /// Returns the most important error afecting whe whole downloading process. Returns EError.NONE is everything is ok.
        /// </summary>        
        public EError GetError()
        {
            if (IsAvailable())
            {
                return EError.NONE;
            }
            else
            {
                return ExtendedGetError();
            }
        }

        protected abstract bool ExtendedNeedsToRequestPermission();

        protected abstract bool ExtendedGetIsPermissionGranted();        

        protected abstract EError ExtendedGetError();

        /// <summary>
        /// To be called manually to refresh group state, progress, errors, etc.
        /// </summary>
        public virtual void Update()
        {
        }
    }
}
