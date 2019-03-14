using UnityEngine;

namespace Downloadables
{
    /// <summary>
    /// This class is responsible for handling the progress of a collection of downloadables.
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
        public abstract bool IsAvailable();

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

        public abstract float GetDownloadedBytes();
        public abstract float GetTotalBytes();

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
