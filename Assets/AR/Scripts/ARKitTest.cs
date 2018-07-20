using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ARKitTest : MonoBehaviour
{
	private GameObject m_kARStartTab = null;
	private GameObject m_kARSurfaceSelectTab = null;
	private GameObject m_kARContentZoomTab = null;
	private GameObject m_kARGameTab = null;
	private Slider m_kARZoomSlider = null;

	private const float c_fDefaultZoomValue = 1.0f;
	private bool m_bAskingForCameraPermission = false;



	private GameObject    mGOScreenContainer;
	private GameObject	  mGOARSurfacePrefab;
	private IARSurface    mARSurfaceComponent;

	private GameObject	  mButtonsContainer;
	private GameObject	  mGOARButtonOptions;
	private GameObject	  mGOARButtonPhoto;

	private GameObject    mButtonSelectSurface;
	private GameObject    mButtonClose;

	private bool m_bARToBeActive = false;



	public class ARGameListener : ARGameManager.ARGameListenerBase
	{
		private ARKitTest m_kSource = null;

		public ARGameListener (ARKitTest kSource)
		{
			m_kSource = kSource;
		}

		public override void onProceedWithARSurfaceSelector (bool bCameraIsGranted)
		{
			m_kSource.onProceedWithARSurfaceSelector (bCameraIsGranted);	
		}

		public override void onNeedToAskForCameraPermission ()
		{
			Debug.Log ("onNeedToAskForCameraPermission");

			ARGameManager.SharedInstance.RequestNativeCameraPermission ();
		}

		public override void onARIsAvailableResult (bool bAvailable)
		{
			Debug.Log ("onARIsAvailableResult: " + bAvailable);

			if (bAvailable)
			{

                if (m_kSource != null)
                {
                    m_kSource.m_bARToBeActive = true;

                    m_kSource.mGOARButtonOptions.SetActive(m_kSource.m_bARToBeActive);
                    m_kSource.mGOARButtonOptions.GetComponent<Button>().onClick.RemoveListener(m_kSource.__arButtonPressed);
                    m_kSource.mGOARButtonOptions.GetComponent<Button>().onClick.AddListener(m_kSource.__arButtonPressed);

                    m_kSource.mGOARButtonPhoto.SetActive(true);
                    m_kSource.mGOARButtonPhoto.GetComponent<Button>().onClick.RemoveListener(m_kSource.__arPhotoButtonPressed);
                    m_kSource.mGOARButtonPhoto.GetComponent<Button>().onClick.AddListener(m_kSource.__arPhotoButtonPressed);
                }
                else
                {
                    Debug.Log("m_kSource = null !!!!!");
                }
			}
		}
	};

	private ARGameListener m_pARGameListener = null;



	public class ARSurfacePrefabListener : IARSurface.ARSurfacePrefabListenerBase
	{
		private ARKitTest mSource;

		public ARSurfacePrefabListener (ARKitTest _source)
		{
			mSource = _source;
		}

		public override void onBackPressed () {
			mSource.onPressedARSurfaceBack ();
		}

		public override void onSurfaceDetectedPressed () {
			mSource.onSurfaceDetectedPressed ();
		}

		public override void onARChangedState (IARSurface.eARSurfaceState eState) {
			mSource.onChangeState (eState);
		}
	}

	private ARSurfacePrefabListener mARSurfaceListener;



	public void onSurfaceDetectedPressed ()
	{
		ARKitManager.SharedInstance.RotateAffectedARObjects (new Vector3(0.0f, 20.0f, 0.0f), false);
	}

	public void loadARSurfacePrefab () {
		string arSurfacePrefab = "ARSurfacePrefab";
		if (arSurfacePrefab != null && arSurfacePrefab != "") {

			mGOARSurfacePrefab = (GameObject) GameObject.Instantiate (Resources.Load ("UI/Screens/" + arSurfacePrefab));
			mGOARSurfacePrefab.transform.SetParent (this.transform, false);
		}
	}

	public void onProceedWithARSurfaceSelector (bool bCameraIsGranted)
	{
		if (bCameraIsGranted) {
			if (mGOARButtonOptions != null) {
				mGOARButtonOptions.SetActive (false);
			}
			if (mGOARButtonPhoto != null) {
				mGOARButtonPhoto.SetActive (false);
			}

			if (mGOScreenContainer != null) {
				mGOScreenContainer.GetComponent<CanvasGroup> ().alpha = 0;
				mGOScreenContainer.GetComponent<CanvasGroup> ().interactable = false;
				mGOScreenContainer.GetComponent<CanvasGroup> ().blocksRaycasts = false;
			}

			loadARSurfacePrefab ();

			if (mGOARSurfacePrefab != null) {
				mARSurfaceComponent = mGOARSurfacePrefab.GetComponent<IARSurface> ();
				mARSurfaceComponent.EnableARTab (true, true);

				mARSurfaceListener = new ARSurfacePrefabListener (this);
				mARSurfaceComponent.SetListener (mARSurfaceListener);
			}

			if (mARSurfaceComponent != null) {
				mARSurfaceComponent.setState (IARSurface.eARSurfaceState.INIT);
			}



			List<GameObject> kHiddenARObjects = new List<GameObject> ();
			kHiddenARObjects.Add (GameObject.Find ("Arena/Model/Env_skybox"));

			ARKitManager.SharedInstance.SetHiddenARObjects (kHiddenARObjects);



			List<GameObject> kAffectedARObjects = new List<GameObject> ();

			GameObject kArena = GameObject.Find ("Arena");
			if (kArena != null)
			{
				kAffectedARObjects.Add (kArena);
			}

			ARKitManager.SharedInstance.SetAffectedARObjects (kAffectedARObjects, 0.01f);



			ARKitManager.SharedInstance.StartSurfaceDetection ();
		} else {
			ARGameManager.SharedInstance.UnInitialise ();
		}
	}

	public void onPressedARSurfaceBack ()
	{
		if (ARKitManager.s_pInstance != null) {
			ARKitManager.SharedInstance.ResetAffectedARObjectsTransform ();
		}

		ARGameManager.SharedInstance.UnInitialise ();

		if (mGOARButtonOptions != null) {
			mGOARButtonOptions.SetActive (m_bARToBeActive);
		}
		if (mGOARButtonPhoto != null) {
			mGOARButtonPhoto.SetActive (m_bARToBeActive);
		}

		if (mGOScreenContainer != null) {
			mGOScreenContainer.GetComponent<CanvasGroup> ().alpha = 1;
			mGOScreenContainer.GetComponent<CanvasGroup> ().interactable = true;
			mGOScreenContainer.GetComponent<CanvasGroup> ().blocksRaycasts = true;
		}

		mGOARSurfacePrefab.GetComponent<IARSurface> ().SetMainCamerasEnabled (true);
		Destroy (mGOARSurfacePrefab);

		onChangeState (IARSurface.eARSurfaceState.INIT);
	}

	public void onChangeState (IARSurface.eARSurfaceState eState)
	{
		bool bContentCamerasEnabled = false;

		switch (eState)
		{
		case IARSurface.eARSurfaceState.DETECTING_SURFACE:
			{
				if (mARSurfaceComponent != null) {
					mARSurfaceComponent.CanHideAffectedObjects (false);
				}

				Time.timeScale = 10.0f;
				Time.fixedDeltaTime = 0.005f * Time.timeScale;

				break;
			}

		case IARSurface.eARSurfaceState.DETECTED_SURFACE:
			{
				bContentCamerasEnabled = true;

				break;
			}

		case IARSurface.eARSurfaceState.AR:
			{
				bContentCamerasEnabled = true;

				if (mGOARButtonOptions != null) {
					mGOARButtonOptions.SetActive (true);
				}
				if (mGOARButtonPhoto != null) {
					mGOARButtonPhoto.SetActive (true);
				}

				if (mGOScreenContainer != null) {
					mGOScreenContainer.GetComponent<CanvasGroup> ().alpha = 1;
					mGOScreenContainer.GetComponent<CanvasGroup> ().interactable = true;
					mGOScreenContainer.GetComponent<CanvasGroup> ().blocksRaycasts = true;
				}

				break;
			}
		}

		if (ARKitManager.SharedInstance.GetSceneContentObject () != null) {
			Camera[] kContentCameras = ARKitManager.SharedInstance.GetSceneContentObject ().GetComponentsInChildren<Camera> ();
			if (kContentCameras != null) {
				for (int i = 0; i < kContentCameras.Length; ++i) {
					kContentCameras [i].enabled = bContentCamerasEnabled;
				}
			}
		}
	}



	private enum eARUITabs
	{
		E_TAB_INIT = 0,
		E_TAB_SURFACE_SELECT,
		E_TAB_CONTENT_ZOOM,
		E_TAB_GAME,

		E_TAB_UNKNOWN
	}

	private eARUITabs m_eCurrentTab = eARUITabs.E_TAB_UNKNOWN;

	private enum eARButtonActions
	{
		BUTTON_AR_START = 0,
		BUTTON_AR_SURFACE_SELECT_BACK,
		BUTTON_AR_SURFACE_SELECT_PIVOT,
		BUTTON_AR_CONTENT_ZOOM_BACK,
		BUTTON_AR_CONTENT_ZOOM_SELECTED,
		BUTTON_AR_FROM_GAME
	};

	public void onPressedButtonReturnToMainMenu()
	{
		ARKitManager.SharedInstance.UnInitialise ();
	}



	private void __arButtonPressed()
	{
		if (ARKitManager.SharedInstance.GetARState () != ARKitManager.eARState.E_AR_PLAYING) {
			ARGameManager.SharedInstance.onPressedButtonAR ();
		} else {
			__buttonOptionsPressed ();
		}
	}

	protected virtual bool IsFileUnavailable(string path)
	{
		if (!File.Exists(path))
			return true;

		FileInfo file = new FileInfo(path);
		FileStream stream = null;

		try
		{
			stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
		}
		catch (IOException)
		{
			return true;
		}
		finally
		{
			if (stream != null)
				stream.Close();
		}

		return false;
	}

	IEnumerator ShareCoroutine (string strFilePath)
	{
		yield return new WaitForEndOfFrame();

		Texture2D kTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

		kTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		kTexture.Apply();

		yield return 0;

		byte[] bytes = kTexture.EncodeToPNG();

		File.WriteAllBytes(strFilePath, bytes);

		DestroyObject(kTexture);



		while (IsFileUnavailable(strFilePath))
		{
			yield return new WaitForSeconds(.05f);
		}

		NativeSharingManager.ShareContentConfig kShareConfig = new NativeSharingManager.ShareContentConfig ();
		kShareConfig.m_strFilePaths.Add (strFilePath);

		NativeSharingManager.SharedInstance.Share (kShareConfig);

		yield return null;
	}

	private void __arPhotoButtonPressed ()
	{
		string strCaptureFolder = "captures";
		string strCapturePath = FileUtils.GetDeviceStoragePath (strCaptureFolder, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);

		if (!Directory.Exists (strCapturePath))
		{
			Directory.CreateDirectory (strCapturePath);
		}

		string strFilePath = strCapturePath + "/capture.png";

		if (File.Exists (strFilePath))
		{
			File.Delete (strFilePath);
		}

		StartCoroutine (ShareCoroutine (strFilePath));
	}

	public void surfaceSelectionAR () {
		__buttonOptionsPressed ();

		if (mGOScreenContainer != null) {
			mGOScreenContainer.GetComponent<CanvasGroup> ().alpha = 0;
			mGOScreenContainer.GetComponent<CanvasGroup> ().interactable = false;
			mGOScreenContainer.GetComponent<CanvasGroup> ().blocksRaycasts = false;
		}

		if (mGOARButtonOptions != null) {
			mGOARButtonOptions.SetActive (false);
		}
		if (mGOARButtonPhoto != null) {
			mGOARButtonPhoto.SetActive (false);
		}

		if (mARSurfaceComponent != null) {
			mARSurfaceComponent.selectSurfacePressed ();
		}
	}

	public void backAR () {
		if (mARSurfaceComponent != null) {
			__buttonOptionsPressed ();

			if (mARSurfaceComponent != null) {
				mARSurfaceComponent.setState (IARSurface.eARSurfaceState.FINISH);
			}
		}
	}

	bool mExpanded = false;
	private void __buttonOptionsPressed()
	{
		if (!mExpanded)
		{
			Animation animationButtonsContainer = mButtonsContainer.GetComponent<Animation>();

			if (animationButtonsContainer != null)
			{
				animationButtonsContainer.Play("ButtonAROptions-Scale");
			}

			mExpanded = true;
		}
		else
		{
			Animation animationButtonsContainer = mButtonsContainer.GetComponent<Animation>();

			if (animationButtonsContainer != null)
			{
				animationButtonsContainer.Play("ButtonAROptions-Scale-Inv");
			}

			mExpanded = false;
		}
	}

	void Awake ()
	{
		Transform [] ts = this.gameObject.GetComponentsInChildren <Transform> (true);
		foreach (Transform child in ts) {
			if (child.name == "Container") {
				mGOScreenContainer = child.gameObject;
			}
			else if (child.name == "ButtonsContainer")
			{
				mButtonsContainer = child.gameObject;
			}
			else if (child.name == "ButtonAROptions")
			{
				mGOARButtonOptions = child.gameObject;
			}
			else if (child.name == "ButtonARPhoto")
			{
				mGOARButtonPhoto = child.gameObject;
			}
			else if (child.name == "ButtonSelectSurface")
			{
				mButtonSelectSurface = child.gameObject;
				child.GetComponent<Button>().onClick.RemoveListener(surfaceSelectionAR);
				child.GetComponent<Button>().onClick.AddListener(surfaceSelectionAR);
			}
			else if (child.name == "ButtonClose")
			{
				mButtonClose = child.gameObject;
				child.GetComponent<Button>().onClick.RemoveListener(backAR);
				child.GetComponent<Button>().onClick.AddListener(backAR);
			}
		}
	}

	void Start ()
	{
		m_pARGameListener = new ARGameListener (this);
		ARGameManager.SharedInstance.SetListener (m_pARGameListener);

		ARGameManager.SharedInstance.Initialise ();

#if (UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR_OSX)
		StartCoroutine (ARKitManager.SharedInstance.CheckARIsAvailable ());
#endif
	}

	private float m_fImageRot = 0.0f;

	void Update ()
	{
		if (ARKitManager.SharedInstance.GetARState () == ARKitManager.eARState.E_AR_SEARCHING_SURFACES ||
			ARKitManager.SharedInstance.GetARState () == ARKitManager.eARState.E_AR_FINISH) {

			if (mARSurfaceComponent != null)
			{
				mARSurfaceComponent.CanHideAffectedObjects (true);
			}
		}
	}
}
