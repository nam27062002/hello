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

		public virtual void onARIsAvailableResult (bool bAvailable) {}
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
			if (m_kARGameManager.IsAskingForCameraPermission ())
			{
				m_kARGameManager.ProceedWithARSurfaceSelector (bGranted);
			}
		}

		public override void onARIsAvailableResult (bool bAvailable)
		{
			m_kARGameManager.OnARIsAvailableResult (bAvailable);
		}

		public override void onARIsInstalledResult (bool bInstalled)
		{
			m_kARGameManager.OnARIsInstalledResult (bInstalled);
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

	public void onPressedButtonAR ()
	{
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
			kConfig.m_strSceneContentObject = "ARCameras";
			kConfig.m_strSurfaceSelectorPrefab = "AR/ARKitSurfaceSelector";
			kConfig.m_strARHitLayer = "ARHitLayer";

			kConfig.m_strAndroidARSessionConfigPath = "AR/Configurations/DefaultSessionConfig";
			kConfig.m_strAndroidARBackgroundMaterialPath = "AR/Materials/ARBackground";

			ARKitManager.SharedInstance.Initialise (kConfig);
		}

		bool bForceAR = false;

		#if UNITY_EDITOR_OSX
		bForceAR = true;
		#endif

#if (UNITY_IOS || UNITY_EDITOR_OSX)
		PermissionsManager.EPermissionStatus eCameraStatus = PermissionsManager.SharedInstance.GetIOSPermissionStatus (PermissionsManager.EIOSPermission.Camera);
#elif UNITY_ANDROID
		PermissionsManager.EPermissionStatus eCameraStatus = PermissionsManager.SharedInstance.GetAndroidPermissionStatus ("android.permission.CAMERA");
#elif UNITY_EDITOR_WIN
		PermissionsManager.EPermissionStatus eCameraStatus = PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED;
#endif

		Debug.Log ("onPressedButtonAR eCameraStatus: " + eCameraStatus);

		if (bForceAR || eCameraStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_GRANTED)
		{
			StartCoroutine (ARKitManager.SharedInstance.RequestARInstallation ());
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
	
	public void OnARIsAvailableResult (bool bAvailable)
	{
		if (m_pARGameListener != null)
		{
			m_pARGameListener.onARIsAvailableResult (bAvailable);
		}
        else
        {
            Debug.Log("ARGameListener is null!!!!!!");
        }
	}

	public void OnARIsInstalledResult (bool bInstalled)
	{
		if (bInstalled)
		{
			ProceedWithARSurfaceSelector (true);
		}
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



	// METHODS ///////////////////////////////////////////////////////////////

	public void Initialise ()
	{
		if (!m_bInitialised)
		{
			m_pARKitListener = new ARKitListener (this);
			ARKitManager.SharedInstance.SetARKitListener (m_pARKitListener);

			m_bInitialised = true;
		}
	}

	public void UnInitialise ()
	{
		if (m_bInitialised)
		{
			m_pARKitListener = null;

			ARKitManager.SharedInstance.UnInitialise ();

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
