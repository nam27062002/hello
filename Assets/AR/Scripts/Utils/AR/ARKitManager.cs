using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
using UnityEngine.XR.iOS;
#elif UNITY_ANDROID
#if ARCORE_SDK_ENABLED
		using GoogleARCore;
#endif
#endif

public class ARKitManager : MonoBehaviour {
    // Singleton ///////////////////////////////////////////////////////////

    public static ARKitManager s_pInstance = null;

    public static ARKitManager SharedInstance {
        get {
            if (s_pInstance == null) {
                s_pInstance = GameContext.AddMainComponent<ARKitManager>();
            }

            return s_pInstance;
        }
    }

    // Delegates /////////////////////////////////////////////////////////////

    public class ARKitListenerBase {
        public virtual void onCameraPermissionGranted(bool bGranted) { }
    };

    private ARKitListenerBase m_pARKitListener = null;

    //////////////////////////////////////////////////////////////////////////

    public enum eARState {
        E_AR_SEARCHING_SURFACES,
        E_AR_SELECTING_ZOOM,
        E_AR_PLAYING,
        E_AR_FINISH,

        E_AR_UNKNOWN
    }

    public class ARConfig {
        public string m_strARConnectionPrefab;

        public string m_strARTrackingCameraPrefab;

        public string m_strARContentCameraPrefab;

        public string m_strSceneContentObject;

        public string m_strSurfaceBasePrefab;

        public string m_strSurfaceSelectorPrefab;

        public string m_strARHitLayer;

        private int m_iARHitLayerMask;

        public ARConfig() {
            m_iARHitLayerMask = -1;
        }

        public int GetLayerMask() {
            if (m_iARHitLayerMask == -1) {
                m_iARHitLayerMask = (1 << LayerMask.NameToLayer(m_strARHitLayer));
            }

            return m_iARHitLayerMask;
        }
    }

    private class ARAffectedObject {
        public GameObject m_kObjectGO;

        public Vector3 m_kOriginalOffsetPos;

        public Quaternion m_kOriginalOffsetRot;
    }

    private List<ARAffectedObject> m_kAffectedARObjects = new List<ARAffectedObject>();

    //////////////////////////////////////////////////////////////////////////

    private class PermissionsListener : PermissionsManager.PermissionsListenerBase {
        public override void onIOSPermissionResult(PermissionsManager.EIOSPermission ePermission, PermissionsManager.EPermissionStatus iStatus) {
            if (ePermission == PermissionsManager.EIOSPermission.Camera) {
                if (iStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_GRANTED) {
                    ARKitManager.SharedInstance.ProcessWithGrantedCamera(true);
                } else if (iStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED ||
                          iStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_RESTRICTED) {
                    ARKitManager.SharedInstance.ProcessWithGrantedCamera(false);
                }
            }
        }

        public override void onAndroidPermissionResult(string strPermission, PermissionsManager.EPermissionStatus iStatus) {
            if (strPermission.CompareTo("android.permission.CAMERA") == 0) {
                if (iStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_GRANTED) {
                    ARKitManager.SharedInstance.ProcessWithGrantedCamera(true);
                } else if (iStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_DENIED ||
                          iStatus == PermissionsManager.EPermissionStatus.E_PERMISSION_RESTRICTED) {
                    ARKitManager.SharedInstance.ProcessWithGrantedCamera(false);
                }
            }
        }
    }

    private PermissionsListener m_kPermissionsListener = null;

    //////////////////////////////////////////////////////////////////////////

    public static readonly float c_fDefaultAffectedARObjectsScale = 0.1f;

    private GameObject m_kARKitGO = null;

    private GameObject m_kARKitSurfaceSelector = null;

    private GameObject m_kARKitTrackingCameraGO = null;

    private GameObject m_kARKitContentCameraGO = null;

    private ARKitContentScaleManager m_kContentScalerManager = null;

    private GameObject m_kSceneContentObject = null;

    private Camera m_kSceneContentCamera = null;

    private Camera m_kSceneContentCameraUI = null;

    private Camera m_kARKitTrackingCamera = null;

#if (UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX)
    private ARKitAnchorManager m_kARAnchorManager = null;
#endif

    private List<GameObject> m_kHiddenARObjects = null;

    private List<GameObject> m_kToShowARObjects = null;

    private float m_fAffectedARObjectsScale = c_fDefaultAffectedARObjectsScale;



    private ARConfig m_kARConfig = null;

#if !UNITY_EDITOR
#if UNITY_IOS
	private ARKitWorldTrackingSessionConfiguration m_kARConfigChecker = new ARKitWorldTrackingSessionConfiguration ();
#endif
#endif

    private eARState m_eARState = eARState.E_AR_UNKNOWN;

    private bool m_bSurfaceSelectorWasMoving = false;

    private Vector3 m_bSurfaceSelectorHitPoint = new Vector3();

    private bool m_bARSurfacesFound = false;

    private bool m_bInitialised = false;

    //////////////////////////////////////////////////////////////////////////



    // INTERNAL METHODS //////////////////////////////////////////////////////

    public void SetCurrentAnchorsVisible(bool bVisible) {
#if (UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX)
        m_kARAnchorManager.SetCurrentAnchorsVisible(bVisible);
#endif
    }

    //////////////////////////////////////////////////////////////////////////



    // GETTERS ///////////////////////////////////////////////////////////////

    public bool IsARKitAvailable() {
#if !UNITY_EDITOR
#if UNITY_IOS
		return m_kARConfigChecker.IsSupported;
#elif UNITY_ANDROID
		return false;
#else
        return false;
#endif
#else
        return true;
#endif
    }

    public bool AreARSurfacesFound() {
        return m_bARSurfacesFound;
    }

    public Camera GetTrackingCamera() {
        return m_kARKitTrackingCamera;
    }

    public GameObject GetSceneContentObject() {
        return m_kSceneContentObject;
    }

    public Camera GetSceneContentCamera() {
        return m_kSceneContentCamera;
    }

    public Camera GetSceneContentCameraUI() {
        return m_kSceneContentCameraUI;
    }

    public bool IsPossibleARPivotSet() {
        return m_bSurfaceSelectorWasMoving;
    }

    public float GetAffectedARObjectsScale() {
        return m_fAffectedARObjectsScale;
    }

    public eARState GetARState() {
        return m_eARState;
    }

    public float GetARContentScale() {
        if (m_kContentScalerManager != null) {
            return m_kContentScalerManager.ContentScale;
        }

        return 1.0f;
    }

    //////////////////////////////////////////////////////////////////////////



    // SETTERS ///////////////////////////////////////////////////////////////

    public void SetHiddenARObjects(List<GameObject> kObjects) {
        m_kHiddenARObjects = kObjects;
    }

    public void SetToShowARObjects(List<GameObject> kObjects) {
        m_kToShowARObjects = kObjects;
    }

    public void SetAffectedARObjects(List<GameObject> kObjects, float fContentScale) {
        m_kAffectedARObjects = new List<ARAffectedObject>();

        for (int i = 0; i < kObjects.Count; ++i) {
            ARAffectedObject kARAffectedObject = new ARAffectedObject();
            kARAffectedObject.m_kObjectGO = kObjects[i];
            kARAffectedObject.m_kOriginalOffsetPos = kObjects[i].transform.localPosition;
            kARAffectedObject.m_kOriginalOffsetRot = kObjects[i].transform.localRotation;

            m_kAffectedARObjects.Add(kARAffectedObject);
        }

        m_fAffectedARObjectsScale = fContentScale;
    }

    public void SetARKitListener(ARKitListenerBase pListener) {
        m_pARKitListener = pListener;
    }

    public void SetAffectedARObjectsEnabled(bool bEnabled) {
        if (m_kAffectedARObjects != null) {
            for (int i = 0; i < m_kAffectedARObjects.Count; ++i) {
                m_kAffectedARObjects[i].m_kObjectGO.SetActive(bEnabled);
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////



    // METHODS ///////////////////////////////////////////////////////////////

    public bool CheckIfInitialised() {
        return m_bInitialised;
    }

    public void Initialise(ARConfig kConfig) {
        if (!m_bInitialised) {
            m_kPermissionsListener = new PermissionsListener();
            PermissionsManager.SharedInstance.AddPermissionsListener(m_kPermissionsListener);

            Debug.Log("AR API Available: " + IsARKitAvailable());

            m_kARConfig = kConfig;

#if UNITY_ANDROID
			GameObject kARKitManager = GameObject.Find ("ARKitManager");
			if (kARKitManager == null)
			{
				m_kARKitGO = new GameObject ("ARKitManager");
				DontDestroyOnLoad (m_kARKitGO);
			}
#endif

#if (UNITY_IOS && UNITY_EDITOR_OSX)
            m_kARKitGO = new GameObject("ARKitManager");
            DontDestroyOnLoad(m_kARKitGO);

            GameObject kARRemoteConnectionGO = Instantiate(Resources.Load(m_kARConfig.m_strARConnectionPrefab) as GameObject);
            kARRemoteConnectionGO.transform.SetParent(m_kARKitGO.transform);
#endif

            m_kARKitSurfaceSelector = Instantiate(Resources.Load(m_kARConfig.m_strSurfaceSelectorPrefab) as GameObject);
            CaletyUtils.SetLayer(m_kARKitSurfaceSelector, m_kARConfig.m_strARHitLayer);
            m_kARKitSurfaceSelector.SetActive(false);

            m_bInitialised = true;
        }

#if (UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX)
        if (m_kARAnchorManager == null) {
            m_kARAnchorManager = new ARKitAnchorManager(m_kARConfig.m_strSurfaceBasePrefab, m_kARConfig.m_strARHitLayer);
        }
#endif
    }

    public void UnInitialise() {
        if (m_bInitialised) {

			#if !UNITY_EDITOR
			#if UNITY_IOS
			UnityARSessionNativeInterface.GetARSessionNativeInterface().Pause();
			#endif
			#endif


            PermissionsManager.SharedInstance.RemovePermissionsListener(m_kPermissionsListener);
            m_kPermissionsListener = null;

            if (m_kARKitTrackingCameraGO != null) {
                DestroyImmediate(m_kARKitTrackingCameraGO);

                m_kARKitTrackingCameraGO = null;
            }

            if (m_kARKitContentCameraGO != null) {
                DestroyImmediate(m_kARKitContentCameraGO);

                m_kARKitContentCameraGO = null;
            }

            DestroyImmediate(m_kARKitSurfaceSelector);

            m_kARKitSurfaceSelector = null;

#if (UNITY_IOS || UNITY_EDITOR_OSX)
            if (m_kARKitGO != null) {
                DestroyImmediate(m_kARKitGO);
                m_kARKitGO = null;
            }
#endif

            m_kARKitTrackingCamera = null;

            if (m_kHiddenARObjects != null) {
                for (int i = 0; i < m_kHiddenARObjects.Count; ++i) {
                    if (m_kHiddenARObjects[i] != null) {
                        m_kHiddenARObjects[i].SetActive(true);
                    }
                }
            }
            if (m_kToShowARObjects != null) {
                for (int i = 0; i < m_kToShowARObjects.Count; ++i) {
                    if (m_kToShowARObjects[i] != null) {
                        m_kToShowARObjects[i].SetActive(false);
                    }
                }
            }

            if (m_kAffectedARObjects != null) {
                for (int i = 0; i < m_kAffectedARObjects.Count; ++i) {
                    if (m_kAffectedARObjects[i] != null && m_kAffectedARObjects[i].m_kObjectGO != null) {
                        m_kAffectedARObjects[i].m_kObjectGO.SetActive(true);
                    }
                }
            }
            m_kAffectedARObjects = null;

            SetCurrentAnchorsVisible(false);

            m_eARState = eARState.E_AR_UNKNOWN;

            m_bInitialised = false;
        }

#if (UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX)
        if (m_kARAnchorManager != null) {
            m_kARAnchorManager.Destroy();
            m_kARAnchorManager = null;
        }
#endif
    }

    public void StartSurfaceDetection() {
        if (m_bInitialised) {
            if (m_kARKitTrackingCameraGO == null) {
                m_kARKitTrackingCameraGO = Instantiate(Resources.Load(m_kARConfig.m_strARTrackingCameraPrefab) as GameObject);

#if UNITY_ANDROID
				GameObject kARCoreDevice = GameObject.Find ("ARKitManager/ARCoreDevice");
				if (kARCoreDevice == null)
				{
					GameObject kARCoreDeviceGO = new GameObject ("ARCoreDevice");

					System.Type kARCoreSessionType = CaletyUtils.GetTypeByClassName ("GoogleARCore.ARCoreSessionCustom");
					if (kARCoreSessionType != null)
					{
						MethodInfo[] kMethods = typeof(GameObject).GetMethods ();
						for (int i = 0; i < kMethods.Length; ++i)
						{
							if (kMethods [i].IsGenericMethod && kMethods [i].Name == "AddComponent")
							{
								MethodInfo kMethod = kMethods [i].MakeGenericMethod (kARCoreSessionType);

								Component kComponent = (Component) kMethod.Invoke (kARCoreDeviceGO, null);

								if (kComponent != null)
								{
									FieldInfo kFieldSessionConfig = kComponent.GetType ().GetField ("SessionConfig");

									if (kFieldSessionConfig != null)
									{
										UnityEngine.Object kDefaultConfig = Resources.Load ("AR/Configurations/DefaultSessionConfig");

										if (kDefaultConfig != null)
										{
											kFieldSessionConfig.SetValue (kComponent, kDefaultConfig);
										}
									}

									PropertyInfo kPropertyEnabled = kComponent.GetType ().GetProperty ("enabled", BindingFlags.Public | BindingFlags.Instance);

									if (kPropertyEnabled != null && kPropertyEnabled.CanWrite)
									{
										kPropertyEnabled.SetValue (kComponent, true, null);
									}
								}
							}
						}
					}

					kARCoreDeviceGO.transform.SetParent (m_kARKitGO.transform);
				}

				System.Type kARCoreBackgroundRendererType = CaletyUtils.GetTypeByClassName ("GoogleARCore.ARCoreBackgroundRendererCustom");
				if (kARCoreBackgroundRendererType != null)
				{
					Transform kARTackingCamera = m_kARKitTrackingCameraGO.transform.Find ("ARTrackingCamera");
					if (kARTackingCamera != null)
					{
						MethodInfo[] kMethods = typeof(GameObject).GetMethods ();
						for (int i = 0; i < kMethods.Length; ++i)
						{
							if (kMethods [i].IsGenericMethod && kMethods [i].Name == "AddComponent")
							{
								MethodInfo kMethod = kMethods [i].MakeGenericMethod (kARCoreBackgroundRendererType);

								Component kComponent = (Component) kMethod.Invoke (kARTackingCamera.gameObject, null);

								if (kComponent != null)
								{
									FieldInfo kFieldBGMat = kComponent.GetType ().GetField ("BackgroundMaterial");

									if (kFieldBGMat != null)
									{
										Material kMaterial = Resources.Load<Material> ("AR/Materials/ARBackground");

										if (kMaterial != null)
										{
											kFieldBGMat.SetValue (kComponent, kMaterial);
										}
									}

									PropertyInfo kPropertyEnabled = kComponent.GetType ().GetProperty ("enabled", BindingFlags.Public | BindingFlags.Instance);

									if (kPropertyEnabled != null && kPropertyEnabled.CanWrite)
									{
										kPropertyEnabled.SetValue (kComponent, true, null);
									}
								}
							}
						}
					}
				}

				if (m_kARAnchorManager != null)
				{
					m_kARAnchorManager.m_bCanUpdate = true;
				}
#endif

                m_kARKitTrackingCamera = m_kARKitTrackingCameraGO.GetComponentInChildren<Camera>();
                m_kARKitTrackingCamera.cullingMask = m_kARConfig.GetLayerMask();
            }

            if (m_kARKitContentCameraGO == null) {
                m_kARKitContentCameraGO = Instantiate(Resources.Load(m_kARConfig.m_strARContentCameraPrefab) as GameObject);

                m_kContentScalerManager = m_kARKitContentCameraGO.GetComponent<ARKitContentScaleManager>();
                m_kContentScalerManager.ContentScale = m_fAffectedARObjectsScale;

                m_kSceneContentObject = GameObject.Find(m_kARConfig.m_strSceneContentObject);
                if (m_kSceneContentObject != null) {
                    Transform kSceneContentCameraGO = m_kSceneContentObject.transform.Find("Scene Camera Models");
                    if (kSceneContentCameraGO != null) {
                        m_kSceneContentCamera = kSceneContentCameraGO.GetComponent<Camera>();
                    }
                    Transform kSceneContentCameraUIGO = m_kSceneContentObject.transform.Find("Scene Camera UI");
                    if (kSceneContentCameraUIGO != null) {
                        m_kSceneContentCameraUI = kSceneContentCameraUIGO.GetComponent<Camera>();
                    }

                    ARKitCameraScaler kContentScaler = m_kARKitContentCameraGO.GetComponentInChildren<ARKitCameraScaler>();
                    kContentScaler.SetScaledContent(m_kSceneContentObject);
                    kContentScaler.m_CameraScale = m_fAffectedARObjectsScale;
                }
            }

            if (m_kHiddenARObjects != null) {
                for (int i = 0; i < m_kHiddenARObjects.Count; ++i) {
                    if (m_kHiddenARObjects[i] != null) {
                        m_kHiddenARObjects[i].SetActive(false);
                    }
                }
            }
            if (m_kToShowARObjects != null) {
                for (int i = 0; i < m_kToShowARObjects.Count; ++i) {
                    if (m_kToShowARObjects[i] != null) {
                        m_kToShowARObjects[i].SetActive(true);
                    }
                }
            }

            m_kARKitSurfaceSelector.SetActive(false);

            SetCurrentAnchorsVisible(true);

            m_bARSurfacesFound = false;

            m_eARState = eARState.E_AR_SEARCHING_SURFACES;
        }
    }

    public void ResetScene() {
#if !UNITY_EDITOR
    #if UNITY_IOS
        ARKitWorldTrackingSessionConfiguration sessionConfig = new ARKitWorldTrackingSessionConfiguration(UnityARAlignment.UnityARAlignmentGravity, UnityARPlaneDetection.Horizontal);
        UnityARSessionNativeInterface.GetARSessionNativeInterface().RunWithConfigAndOptions(sessionConfig, UnityARSessionRunOption.ARSessionRunOptionRemoveExistingAnchors | UnityARSessionRunOption.ARSessionRunOptionResetTracking);

        m_kARAnchorManager.ClearAnchors();
    #endif
#endif
    }

	public void SelectCurrentPositionAsARPivot ()
	{
		SetCurrentAnchorsVisible (false);

		m_kARKitSurfaceSelector.SetActive (false);

		m_bSurfaceSelectorWasMoving = false;

		m_eARState = eARState.E_AR_SELECTING_ZOOM;

		if (m_kARKitContentCameraGO != null)
		{
			ARKitCameraScaler kContentScaler = m_kARKitContentCameraGO.GetComponentInChildren<ARKitCameraScaler> ();
			kContentScaler.m_ScaledObjectOrigin = m_bSurfaceSelectorHitPoint;
			kContentScaler.Update ();
		}

		if (m_kAffectedARObjects != null)
		{
			if (m_kAffectedARObjects.Count > 0)
			{
				Vector3 kNewPos = Vector3.zero;

				for (int i = 0; i < m_kAffectedARObjects.Count; ++i)
				{
					kNewPos = m_bSurfaceSelectorHitPoint;
					kNewPos += m_kAffectedARObjects [i].m_kOriginalOffsetPos;

					m_kAffectedARObjects [i].m_kObjectGO.transform.localPosition = kNewPos;
				}

				Vector3 kRelativePos = m_kSceneContentObject.transform.position - m_kAffectedARObjects [0].m_kObjectGO.transform.position;
				Quaternion kRotation = Quaternion.LookRotation (kRelativePos);

				kNewPos = kRotation.eulerAngles;
				kNewPos.x = 0.0f;
				kNewPos.z = 0.0f;

				RotateAffectedARObjects (kNewPos, false);

				for (int i = 0; i < m_kAffectedARObjects.Count; ++i)
				{
					m_kAffectedARObjects [i].m_kObjectGO.SetActive (true);
				}
			}
		}
	}

	public void ChangeZoom (float fValue)
	{
        if (m_kARKitTrackingCamera != null) {
            float fInvScale = 1.0f / (m_fAffectedARObjectsScale / c_fDefaultAffectedARObjectsScale);

            float fNewFarCameraPlane = (120.0f - ((fValue / 100.0f) * 40.0f)) * fInvScale;

            m_kARKitTrackingCamera.farClipPlane = fNewFarCameraPlane;

            Camera[] kContentCameras = m_kSceneContentObject.GetComponentsInChildren<Camera>();
            if (kContentCameras != null) {
                for (int i = 0; i < kContentCameras.Length; ++i) {
                    if (!kContentCameras[i].orthographic) {
                        kContentCameras[i].farClipPlane = fNewFarCameraPlane;
                    }
                }
            }
        }

        if (m_kContentScalerManager != null) {
            m_kContentScalerManager.ContentScale = m_fAffectedARObjectsScale * fValue;
        }
	}

	public void SelectedZoom ()
	{
		m_eARState = eARState.E_AR_PLAYING;
	}

	public void FinishingARSession ()
	{
		m_eARState = eARState.E_AR_FINISH;
	}

	public void ProcessWithGrantedCamera (bool bGranted)
	{
		if (m_pARKitListener != null)
		{
			m_pARKitListener.onCameraPermissionGranted (bGranted);
		}
	}

	public void RotateAffectedARObjects (Vector3 kRotation, bool bFinalRotation = false)
	{
		if (m_kAffectedARObjects != null)
		{
			for (int i = 0; i < m_kAffectedARObjects.Count; ++i)
			{
				if (!bFinalRotation)
				{
					m_kAffectedARObjects [i].m_kObjectGO.transform.Rotate (kRotation);
				}
				else
				{
					m_kAffectedARObjects [i].m_kObjectGO.transform.localRotation = Quaternion.Euler (kRotation);
				}

				if (i != 0)
				{
					Vector3 kNewOffsetPos = m_kAffectedARObjects [i].m_kObjectGO.transform.position;
					kNewOffsetPos = Quaternion.Euler (kRotation) * kNewOffsetPos;

					Vector3 kNewPos = m_kAffectedARObjects [i].m_kObjectGO.transform.position;

					m_kAffectedARObjects [i].m_kObjectGO.transform.position = kNewPos + kNewOffsetPos;
				}
			}
		}
	}

	public void ResetAffectedARObjectsTransform ()
	{
		if (m_kAffectedARObjects != null)
		{
			for (int i = 0; i < m_kAffectedARObjects.Count; ++i)
			{
				if (m_kAffectedARObjects [i] != null && m_kAffectedARObjects [i].m_kObjectGO != null) {
					m_kAffectedARObjects [i].m_kObjectGO.transform.localPosition = m_kAffectedARObjects [i].m_kOriginalOffsetPos;
					m_kAffectedARObjects [i].m_kObjectGO.transform.localRotation = m_kAffectedARObjects [i].m_kOriginalOffsetRot;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////



	// UNITY METHODS /////////////////////////////////////////////////////////

	void Update ()
	{
#if (UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX)
		if (m_kARAnchorManager != null)
		{
			m_kARAnchorManager.Update ();
		}


		if (m_eARState == eARState.E_AR_SEARCHING_SURFACES)
		{
#if (UNITY_IOS || UNITY_EDITOR_OSX)
			List<ARPlaneAnchorGameObject> kARAnchors = m_kARAnchorManager.GetCurrentPlaneAnchors ();
#elif UNITY_ANDROID
			List<GameObject> kARAnchors = m_kARAnchorManager.GetCurrentPlaneAnchors ();
#endif
			if (kARAnchors != null && kARAnchors.Count > 0)
			{
				m_bARSurfacesFound = true;

#if (UNITY_IOS || UNITY_EDITOR_OSX)

				Ray ray = m_kARKitTrackingCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
				RaycastHit hit;

				if (Physics.Raycast (ray, out hit, 3.0f, m_kARConfig.GetLayerMask ()))
				{
					m_kARKitSurfaceSelector.SetActive (true);

					m_bSurfaceSelectorHitPoint = hit.point;

					if (!m_bSurfaceSelectorWasMoving)
					{
						m_kARKitSurfaceSelector.transform.position = m_bSurfaceSelectorHitPoint;
					}
					else
					{
						m_kARKitSurfaceSelector.transform.position = Vector3.Lerp(m_kARKitSurfaceSelector.transform.position, m_bSurfaceSelectorHitPoint, 0.25f);
					}

					m_bSurfaceSelectorWasMoving = true;
				}
				else
				{
					m_kARKitSurfaceSelector.SetActive (false);

					m_bSurfaceSelectorWasMoving = false;
				}
					
#elif UNITY_ANDROID

#if ARCORE_SDK_ENABLED
				// Raycast against the location the player touched to search for planes.
				TrackableHit hit;
				TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinPolygon;

				if (Session.Raycast (Screen.currentResolution.width * 0.5f, Screen.currentResolution.height * 0.5f, raycastFilter, out hit))
				{
					m_kARKitSurfaceSelector.SetActive (true);

					m_bSurfaceSelectorHitPoint = hit.Pose.position;

					if (!m_bSurfaceSelectorWasMoving)
					{
						m_kARKitSurfaceSelector.transform.position = m_bSurfaceSelectorHitPoint;
					}
					else
					{
						m_kARKitSurfaceSelector.transform.position = Vector3.Lerp(m_kARKitSurfaceSelector.transform.position, m_bSurfaceSelectorHitPoint, 0.25f);
					}

					m_bSurfaceSelectorWasMoving = true;
				}
				else
				{
					m_kARKitSurfaceSelector.SetActive (false);

					m_bSurfaceSelectorWasMoving = false;
				}
#endif

#endif
			}
		}
#endif
    }

    //////////////////////////////////////////////////////////////////////////
}
