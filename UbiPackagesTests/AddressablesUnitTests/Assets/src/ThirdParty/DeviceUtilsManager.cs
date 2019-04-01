using UnityEngine;

public class DeviceUtilsManager
{
    private static DeviceUtilsManager sm_instance;

    public static DeviceUtilsManager SharedInstance
    {
        get
        {
            if (sm_instance == null)
            {
                sm_instance = new DeviceUtilsManager();
            }

            return sm_instance;
        }
    }

#if UNITY_IOS
    [DllImport ("__Internal", CallingConvention=CallingConvention.Cdecl)] private static extern long DeviceUtils_GetDeviceFreeDiskSpace ([In] string strPath);
#endif

    public long GetDeviceFreeDiskSpace ()
    {
#if UNITY_EDITOR
#if UNITY_EDITOR_WIN
		return 1024 * 1024 * 1024;
#elif UNITY_EDITOR_OSX
        try
        {
            System.IO.DriveInfo[] kAllDrives = System.IO.DriveInfo.GetDrives ();
            foreach (DriveInfo kDriveInfo in kAllDrives)
            {
                if (!Application.persistentDataPath.StartsWith (kDriveInfo.Name))
                {
                    continue;
                }

                return kDriveInfo.AvailableFreeSpace;
            }
        }
        catch (NotImplementedException e)
        {
        }
#endif
#else
#if UNITY_ANDROID
		using (AndroidJavaObject kStatFs = new AndroidJavaObject("android.os.StatFs", Application.persistentDataPath))
		{
			return kStatFs.Call<long>("getBlockSizeLong") * kStatFs.Call<long>("getAvailableBlocksLong");
		}
#elif UNITY_IOS
		return DeviceUtils_GetDeviceFreeDiskSpace (Application.persistentDataPath);
#else
		return 0;
#endif
#endif

		return 0;
    }    
}
