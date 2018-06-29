using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using UnityEngine.SceneManagement;

public class IARSurface : MonoBehaviour
{
    public enum eARSurfaceState
    {
		INIT,
        DETECTING_SURFACE,
        DETECTED_SURFACE,
        AR,
		FINISH
    }

    // ======================================================================================================================
    // ATTRIBUTES
    // ======================================================================================================================
	eARSurfaceState mARSurfaceState;

	GameObject mGOAR;
    GameObject mGODetectingSurface;
    GameObject mGODetectedSurface;
	GameObject mGOBackground;
    
    GameObject mGOStatus;
    GameObject mGOStatusText;

	Transform mZoomIndicator = null;

	GameObject mGOARButtonOptions;
	GameObject mButtonsContainer;
	Button mButtonSurfaceDetected;
	Button mButtonSelectSurface;
    Button mButtonClose;
	Slider mZoomSlider;

	Camera[] mMainSceneCameras = null;

	private DeltaTimer mBackgroundTimer = null;

	private const float c_fDefaultZoomValue = 2.0f;

	private bool mAvoidARUI = false;

	private bool m_bNeedToSetSelectSurfaceButtonEnabled = false;
	private bool m_bNeedToSetSelectSurfaceButtonEnabledValue = false;
	private bool mCanHideAffectedObjects = false;

    // ======================================================================================================================
    // LISTENERS
    // ======================================================================================================================
	public class ARSurfacePrefabListenerBase
	{
		public virtual void onBackPressed () {}

		public virtual void onSurfaceDetectedPressed () {}

		public virtual void onARChangedState (eARSurfaceState eState) {}
	}

	private ARSurfacePrefabListenerBase mListener;

	public void back () {
		SetMainCamerasEnabled (true);

		if (mListener != null) {
			mListener.onBackPressed ();
		}
	}

    private void __buttonDetectedPressed()
    {
		ARKitManager.SharedInstance.SelectCurrentPositionAsARPivot ();

		if (mListener != null)
		{
			mListener.onSurfaceDetectedPressed ();
		}

		setState (eARSurfaceState.DETECTED_SURFACE);
    }

    private void __sliderValueChanged(float value)
    {
		ZoomChanged (value);

		ARKitManager.SharedInstance.ChangeZoom (value);
    }

    private void __buttonGoPressed()
    {
		setState (eARSurfaceState.AR);
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

    public void selectSurfacePressed()
    {
        // AR_Start
		ARKitManager.SharedInstance.StartSurfaceDetection ();

		setState (eARSurfaceState.DETECTING_SURFACE);
    }

	private void ZoomChanged (float fValue)
	{
		float fInvScale = 1.0f / (ARKitManager.SharedInstance.GetAffectedARObjectsScale () / ARKitManager.c_fDefaultAffectedARObjectsScale);

		float fNewFarCameraPlane = (12.0f - ((fValue / 10.0f) * 4.0f)) * fInvScale;

		ARKitManager.SharedInstance.GetTrackingCamera ().farClipPlane = fNewFarCameraPlane;

		Camera[] kContentCameras = ARKitManager.SharedInstance.GetSceneContentObject ().GetComponentsInChildren<Camera> ();
		if (kContentCameras != null)
		{
			for (int i = 0; i < kContentCameras.Length; ++i)
			{
				if (!kContentCameras [i].orthographic)
				{
					kContentCameras [i].farClipPlane = fNewFarCameraPlane;
				}
			}
		}
	}

	// ======================================================================================================================
	// HELPERS
	// ======================================================================================================================
	public void SetListener (ARSurfacePrefabListenerBase _listener)
	{
		mListener = _listener;
	}

	public void CanHideAffectedObjects (bool bEnabled)
	{
		mCanHideAffectedObjects = bEnabled;
	}

	public void EnableARTab (bool bEnabled, bool bAvoidARUI = false)
	{
		CanvasGroup mCanvasGroup = this.GetComponent<CanvasGroup> ();
		if (mCanvasGroup != null) {
			mCanvasGroup.interactable = bEnabled;
			mCanvasGroup.blocksRaycasts = bEnabled;
			mCanvasGroup.alpha = (bEnabled ? 1:0);
		}

		mAvoidARUI = bAvoidARUI;
	}

	public void setState (eARSurfaceState _state)
    {
		if (mExpanded)
			__buttonOptionsPressed ();
		
		switch (_state) {
			case eARSurfaceState.INIT: {
				mCanHideAffectedObjects = false;

				break;
			}

			case eARSurfaceState.DETECTING_SURFACE: {
				if (mZoomIndicator != null) {
					mZoomIndicator.gameObject.SetActive (false);
				}

				if (mGOBackground != null) {
					mGOBackground.gameObject.SetActive (true);
				}

				if (mAvoidARUI && mGOARButtonOptions != null) {
					mGOARButtonOptions.SetActive (true);
				}

				SetSelectSurfaceButtonEnabled (false);

				mGOStatusText.transform.parent.gameObject.SetActive (true);

				break;
			}

			case eARSurfaceState.DETECTED_SURFACE: {
				SetSelectSurfaceButtonEnabled (true);

				ARKitManager.SharedInstance.ChangeZoom (c_fDefaultZoomValue);

				mZoomSlider.value = c_fDefaultZoomValue;

				ZoomChanged (mZoomSlider.value);

				break;
			}

			case eARSurfaceState.AR: {
				if (mAvoidARUI && mGOARButtonOptions != null) {
					mGOARButtonOptions.SetActive (false);
				}

				mGOStatusText.transform.parent.gameObject.SetActive (false);

				ARKitManager.SharedInstance.SelectedZoom ();

				break;
			}

			case eARSurfaceState.FINISH: {
				mCanHideAffectedObjects = false;

				if (mGOBackground != null) {
					mGOBackground.gameObject.SetActive (true);
				}

				mBackgroundTimer = new DeltaTimer ();
				mBackgroundTimer.Start (1000.0f);

				Time.timeScale = 10.0f;
				Time.fixedDeltaTime = 0.005f * Time.timeScale;

				ARKitManager.SharedInstance.FinishingARSession ();

				break;
			}
		}

		if (_state != eARSurfaceState.INIT) {
			UIUtils.setGOVisible (ref mGODetectingSurface, _state == eARSurfaceState.DETECTING_SURFACE, _state == eARSurfaceState.DETECTING_SURFACE);
			UIUtils.setGOVisible (ref mGODetectedSurface, _state == eARSurfaceState.DETECTED_SURFACE, _state == eARSurfaceState.DETECTED_SURFACE);
			UIUtils.setGOVisible (ref mGOStatus, _state != eARSurfaceState.AR, _state != eARSurfaceState.AR);
			UIUtils.setGOVisible (ref mGOAR, _state == IARSurface.eARSurfaceState.AR, _state == IARSurface.eARSurfaceState.AR);
		}

		if (mListener != null) {
			mListener.onARChangedState (_state);
		}

		mARSurfaceState = _state;
    }

	public eARSurfaceState getState () {
		return mARSurfaceState;
	}

	public void SetMainCamerasEnabled (bool bEnabled) {
		if (mMainSceneCameras != null && mMainSceneCameras.Length > 0) {
			for (int i = 0; i < mMainSceneCameras.Length; ++i) {
				if (mMainSceneCameras [i] != null) {
					mMainSceneCameras [i].enabled = bEnabled;
				}
			}
		}
	}
		
	private bool SetSelectSurfaceButtonEnabled_Internal (bool bEnabled)
	{
		if (mButtonSelectSurface != null)
		{
			mButtonSelectSurface.interactable = bEnabled;
		}

		return true;
	}
	public void SetSelectSurfaceButtonEnabled (bool bEnabled) {
		m_bNeedToSetSelectSurfaceButtonEnabled = SetSelectSurfaceButtonEnabled_Internal (bEnabled);
		m_bNeedToSetSelectSurfaceButtonEnabledValue = bEnabled;
	}

	// ======================================================================================================================
	// INIT
	// ======================================================================================================================
	public void Start ()
	{
		configure ();
	}

	protected void configure () {

		GameObject kMainCamerasGO = GameObject.Find ("Scene Cameras");
		if (kMainCamerasGO != null)
		{
			mMainSceneCameras = kMainCamerasGO.GetComponentsInChildren<Camera> ();

			SetMainCamerasEnabled (false);
		}

        Transform[] ts = gameObject.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in ts)
        {
			if (child.name == "AR")
			{
				mGOAR = child.gameObject;
			}
			else if (child.name == "ARBackground")
			{
				mGOBackground = child.gameObject;
			}
            else if (child.name == "DetectingSurface")
            {
                mGODetectingSurface = child.gameObject;
            }
            else if (child.name == "DetectedSurface")
            {
                mGODetectedSurface = child.gameObject;
            }
            else if (child.name == "ButtonGo")
            {
                child.GetComponent<Button>().onClick.RemoveListener(__buttonGoPressed);
                child.GetComponent<Button>().onClick.AddListener(__buttonGoPressed);
            }
            else if (child.name == "ButtonDetected")
            {
				mButtonSurfaceDetected = child.GetComponent<Button>();

				mButtonSurfaceDetected.onClick.RemoveListener(__buttonDetectedPressed);
				mButtonSurfaceDetected.onClick.AddListener(__buttonDetectedPressed);
				mButtonSurfaceDetected.gameObject.SetActive (false);
            }
			else if (child.name == "ZoomIndicator")
			{
				mZoomIndicator = child;
			}
            else if (child.name == "Slider")
            {
				mZoomSlider = child.GetComponent<Slider> ();

				mZoomSlider.onValueChanged.RemoveListener(__sliderValueChanged);
				mZoomSlider.onValueChanged.AddListener(__sliderValueChanged);
            }
            else if (child.name == "Status")
            {
                mGOStatus = child.gameObject;
            }
            else if (child.name == "StatusText")
            {
                mGOStatusText = child.gameObject;
			}
			else if (child.name == "ButtonsContainer")
			{
				mButtonsContainer = child.gameObject;
			}
			else if (child.name == "ButtonAROptions")
			{
				mGOARButtonOptions = child.gameObject;
                mGOARButtonOptions.GetComponent<Button>().onClick.RemoveListener(__buttonOptionsPressed);
                mGOARButtonOptions.GetComponent<Button>().onClick.AddListener(__buttonOptionsPressed);
            }
			else if (child.name == "ButtonClose")
			{
				mButtonClose = child.GetComponent<Button>();

				mButtonClose.onClick.RemoveListener(back);
				mButtonClose.onClick.AddListener(back);
			}
			else if (child.name == "ButtonSelectSurface")
			{
				mButtonSelectSurface = child.GetComponent<Button>();

				mButtonSelectSurface.onClick.RemoveListener(selectSurfacePressed);
				mButtonSelectSurface.onClick.AddListener(selectSurfacePressed);
			}
		}

		setState (eARSurfaceState.DETECTING_SURFACE);
	}

	// ======================================================================================================================
	// UPDATE
	// ======================================================================================================================
	public void Update () {
		if (mARSurfaceState == eARSurfaceState.FINISH) {
			if (mBackgroundTimer != null && mCanHideAffectedObjects && mBackgroundTimer.IsFinished ()) {
				Time.timeScale = 1.0f;
				Time.fixedDeltaTime = 0.005f * Time.timeScale;

				back ();
			}
		} else {
			if (mBackgroundTimer == null && mCanHideAffectedObjects && mGOBackground.gameObject.activeInHierarchy) {
				mBackgroundTimer = new DeltaTimer ();
				mBackgroundTimer.Start (1000.0f);
			}
			if (mBackgroundTimer != null && mBackgroundTimer.IsFinished ()) {
				ARKitManager.SharedInstance.SetAffectedARObjectsEnabled (false);

				Time.timeScale = 1.0f;
				Time.fixedDeltaTime = 0.005f * Time.timeScale;

				ARKitManager.SharedInstance.ResetAffectedARObjectsTransform ();
				mGOBackground.gameObject.SetActive (false);
				mBackgroundTimer = null;

				if (mZoomIndicator != null) {
					mZoomIndicator.gameObject.SetActive (true);
				}
			}
		}

		if (mARSurfaceState == eARSurfaceState.DETECTING_SURFACE) {
			if (mButtonSurfaceDetected != null && mZoomIndicator != null) {
				bool bSurfacesFound = ARKitManager.SharedInstance.AreARSurfacesFound ();

				if (bSurfacesFound) {
					mButtonSurfaceDetected.gameObject.SetActive (ARKitManager.SharedInstance.IsPossibleARPivotSet ());

					mZoomIndicator.gameObject.SetActive (false);
				}
			}
		}

		if (m_bNeedToSetSelectSurfaceButtonEnabled) {
			m_bNeedToSetSelectSurfaceButtonEnabled = SetSelectSurfaceButtonEnabled_Internal (m_bNeedToSetSelectSurfaceButtonEnabledValue);
		}
	}
}
