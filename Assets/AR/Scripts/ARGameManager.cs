using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARGameManager : MonoBehaviour
{
	// Singleton ///////////////////////////////////////////////////////////

	private static ARGameManager s_pInstance = null;

	public static ARGameManager SharedInstance
	{
		get
		{
			if (s_pInstance == null)
			{
				s_pInstance = GameContext.AddMainComponent<ARGameManager> ();
			}

			return s_pInstance;
		}
	}

	// Delegates /////////////////////////////////////////////////////////////

	public class ARGameListenerBase
	{
		public virtual void onProceedWithARSurfaceSelector (bool bCameraIsGranted) {}

		public virtual void onNeedToAskForCameraPermission () {}
	};

	private ARGameListenerBase m_pARGameListener = null;

	//////////////////////////////////////////////////////////////////////////

	private bool m_bInitialised = false;

	private const float c_fDefaultZoomValue = 1.0f;
	private bool m_bAskingForCameraPermission = false;

	private class ARKitListener : ARKitManager.ARKitListenerBase
	{
		private ARGameManager m_kARGameManager = null;

		public ARKitListener (ARGameManager kTest)
		{
			m_kARGameManager = kTest;
		}

		public override void onCameraPermissionGranted (bool bGranted)
		{
/*
#if !IN_CALETY_TESTER
            // Tracking -> Permiso camara
            TrackingStatesManager.TrackAR (TrackingStatesManager.eARStep.AR_Start);
#endif
*/
			if (m_kARGameManager.IsAskingForCameraPermission ())
			{
				m_kARGameManager.ProceedWithARSurfaceSelector (bGranted);
			}
		}
	};

	private ARKitListener m_pARKitListener = null;

	private enum eARUITabs
	{
		E_TAB_INIT = 0,
		E_TAB_SURFACE_SELECT,
		E_TAB_CONTENT_ZOOM,
		E_TAB_GAME,

		E_TAB_UNKNOWN
	}

	private enum eARButtonActions
	{
		BUTTON_AR_START = 0,
		BUTTON_AR_SURFACE_SELECT_BACK,
		BUTTON_AR_SURFACE_SELECT_PIVOT,
		BUTTON_AR_CONTENT_ZOOM_BACK,
		BUTTON_AR_CONTENT_ZOOM_SELECTED,
		BUTTON_AR_FROM_GAME
	};
/*
    protected const string MEDIA_STORE_IMAGE_MEDIA = "android.provider.MediaStore$Images$Media";
    protected static AndroidJavaObject m_Activity;

    protected static Uri GetMediaPath()
    {
        using (AndroidJavaClass mediaClass = new AndroidJavaClass(MEDIA_STORE_IMAGE_MEDIA))
        {
            Uri uri = mediaClass.Get<Uri>("INTERNAL_CONTENT_URI");
            Debug.Log("INTERNAL_CONTENT_URI" + uri);
            uri = mediaClass.Get<Uri>("EXTERNAL_CONTENT_URI");
            Debug.Log("EXTERNAL_CONTENT_URI" + uri);
            return uri;
        }
    }
*/
/*
    protected static AndroidJavaObject Texture2DToAndroidBitmap(Texture2D a_Texture)
    {
        byte[] encodedTexture = a_Texture.EncodeToPNG();
        using (AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory"))
        {
            return bitmapFactory.CallStatic<AndroidJavaObject>("decodeByteArray", encodedTexture, 0, encodedTexture.Length);
        }
    }
*/
    private readonly string screenShotName = "Screenshot";
    public void onPressedScreenshotButton()
    {
        int count = 0;
        while (System.IO.File.Exists(Application.persistentDataPath + "/" + screenShotName + count + ".png")) count++;
        ScreenCapture.CaptureScreenshot(screenShotName + count + ".png");
    }

    public void onPressedButtonAR ()
	{
//        Uri uri = GetMediaPath();

		if (!ARKitManager.SharedInstance.CheckIfInitialised ())
		{
			ARKitManager.ARConfig kConfig = new ARKitManager.ARConfig ();
			kConfig.m_strARConnectionPrefab = "AR/ARKitRemoteConnection";

#if (UNITY_EDITOR_OSX || UNITY_IOS)
			kConfig.m_strARTrackingCameraPrefab = "AR/ARTrackingCameraBase";
			kConfig.m_strSurfaceBasePrefab = "AR/ARKitSurfaceBase";
#else
			kConfig.m_strARTrackingCameraPrefab = "AR/ARTrackingCameraBaseAndroid";
			kConfig.m_strSurfaceBasePrefab = "AR/ARCoreTrackedPlaneVisualizer";
#endif
			kConfig.m_strARContentCameraPrefab = "AR/ARContentCameraBase";
			kConfig.m_strSceneContentObject = "MenuScene3D/ARBasePrefab/ARCameras";
			kConfig.m_strSurfaceSelectorPrefab = "AR/ARKitSurfaceSelector";
            kConfig.m_strARHitLayer = "Player";// "ARHitLayer";

			ARKitManager.SharedInstance.Initialise (kConfig);
		}

		bool bForceAR = false;

#if UNITY_EDITOR_OSX
		bForceAR = true;
#endif
        PermissionsManager.EPermissionStatus eCameraStatus = PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
		eCameraStatus = PermissionsManager.SharedInstance.GetIOSPermissionStatus (PermissionsManager.EIOSPermission.Camera);
#elif UNITY_ANDROID
		eCameraStatus = PermissionsManager.SharedInstance.GetAndroidPermissionStatus ("android.permission.CAMERA");
#endif

        Debug.Log ("onPressedButtonAR eCameraStatus: " + eCameraStatus);

		if (bForceAR || eCameraStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_GRANTED)
		{
			ProceedWithARSurfaceSelector (true);
		}
		else if (eCameraStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_NOT_DETERMINED)
		{
			if (m_pARGameListener != null)
			{
				Debug.Log ("onPressedButtonAR m_pARGameListener.onNeedToAskForCameraPermission");

				m_pARGameListener.onNeedToAskForCameraPermission ();
			}
		}
		else if (eCameraStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED ||
				 eCameraStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_RESTRICTED)
		{
			ProceedWithARSurfaceSelector (false);
		}
	}

	public void ProceedWithARSurfaceSelector (bool bCameraPermissionGranted)
	{
		if (m_pARGameListener != null)
		{
			m_pARGameListener.onProceedWithARSurfaceSelector (bCameraPermissionGranted);
		}

		m_bAskingForCameraPermission = false;
	}

	public void RequestNativeCameraPermission ()
	{
		m_bAskingForCameraPermission = true;

#if (UNITY_EDITOR_OSX || UNITY_IOS)
		PermissionsManager.SharedInstance.RequestIOSPermission (PermissionsManager.EIOSPermission.Camera);
#elif UNITY_ANDROID
		PermissionsManager.SharedInstance.RequestAndroidPermission ("android.permission.CAMERA");
#endif
	}

	public bool IsAskingForCameraPermission ()
	{
		return m_bAskingForCameraPermission;
	}

	private void onZoomSliderChange (float fValue)
	{
		ARKitManager.SharedInstance.ChangeZoom (fValue);
	}

	//////////////////////////////////////////////////////////////////////////



	// SETTERS ///////////////////////////////////////////////////////////////

	public void SetListener (ARGameListenerBase kListener)
	{
		m_pARGameListener = kListener;
	}

    //////////////////////////////////////////////////////////////////////////

    private float m_backupTimeScale;
    private float m_backupFixedDeltaTime;


    // METHODS ///////////////////////////////////////////////////////////////

    public void Initialise ()
	{
		if (!m_bInitialised)
		{
            m_backupTimeScale = Time.timeScale;
            m_backupFixedDeltaTime = Time.fixedDeltaTime;

            m_pARKitListener = new ARKitListener (this);
			ARKitManager.SharedInstance.SetARKitListener (m_pARKitListener);

            Debug.Log("----->>>>>> AR Initialise ....");

			m_bInitialised = true;
		}
	}

	public void UnInitialise ()
	{
		if (m_bInitialised)
		{
			m_pARKitListener = null;

			ARKitManager.SharedInstance.UnInitialise ();

            Time.timeScale = m_backupTimeScale;
            Time.fixedDeltaTime = m_backupFixedDeltaTime;

            Debug.Log("----->>>>>> AR UnInitialise ....");

            m_bInitialised = false;
		}
	}

	//////////////////////////////////////////////////////////////////////////



	// UNITY METHODS /////////////////////////////////////////////////////////

	void Update ()
	{
	}

	//////////////////////////////////////////////////////////////////////////
}
