// CaptureTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
#define CAPTURE_WHOLE_SCREEN 

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Small tool to capture a screen and save it to a file.
/// </summary>
public abstract class  CaptureTool : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected const string CAPTURE_MODE_KEY = ".CaptureMode";
	protected const string TIME_SCALE_KEY = ".TimeScale";
	protected const string PATH_KEY = ".SaveDir";
	protected const string CROP_KEY = ".Crop";

	protected const string CAMERA_SENSITIVITY_KEY = ".CameraSensitivity";
	protected const string CAMERA_POS_KEY = ".CameraPos";
	protected const string CAMERA_ROT_KEY = ".CameraRot";
	protected const string CAMERA_FOV_KEY = ".CameraFOV";

	protected enum CaptureMode {
		SIMPLE_CAPTURE = 0,
		CHROMA_CAPTURE,
		CAMERA_RENDER,

		COUNT
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Photo Setup")]
	[SerializeField] private RectTransform m_cropGuide = null;
	[SerializeField] private Dropdown m_modeDropdown = null;
	[SerializeField] private List<GameObject> m_objectsToHide = new List<GameObject>();

	[Separator("Setup")]
	[SerializeField] private InputField m_pathInput = null;
	[SerializeField] private Toggle m_cropToggle = null;
	[SerializeField] private Slider m_timeScaleSlider = null;
	[SerializeField] private Text m_timeScaleText = null;

	[Separator("Photo Preview")]
	[SerializeField] private GameObject m_picturePreviewPopup = null;
	[SerializeField] private RawImage m_picturePreview = null;
	[SerializeField] private AspectRatioFitter m_picturePreviewRatioFitter = null;

	[Separator("Camera Control")]
	[SerializeField] protected Camera m_mainCamera = null;
	[SerializeField] private float m_cameraMoveSpeed = 0.05f;
	[SerializeField] private float m_cameraRotateSpeed = 0.5f;
	[SerializeField] private Slider m_cameraSensitivitySlider = null;
	[SerializeField] private Text m_cameraSensitiviyText = null;

	[Space]
	[SerializeField] private KeyCode m_cameraLeftKey = KeyCode.A;
	[SerializeField] private KeyCode m_cameraRightKey = KeyCode.D;
	[Space]
	[SerializeField] private KeyCode m_cameraUpKey = KeyCode.W;
	[SerializeField] private KeyCode m_cameraDownKey = KeyCode.S;
	[Space]
	[SerializeField] private KeyCode m_cameraForwardKey = KeyCode.Q;
	[SerializeField] private KeyCode m_cameraBackwardsKey = KeyCode.E;

	[Space]
	[Tooltip("Hold to rotate rather than move")]
	[SerializeField] private KeyCode m_rotationModifierKey = KeyCode.LeftShift;

	[Space]
	[SerializeField] private float m_cameraFovInc = 1f;
	[SerializeField] private KeyCode m_cameraFovUpKey = KeyCode.KeypadPlus;
	[SerializeField] private KeyCode m_cameraFovDownKey = KeyCode.KeypadMinus;

	[Separator("Other Keyboard Shortcuts")]
	[SerializeField] private KeyCode m_takePictureKey = KeyCode.Space;

	[Space]
	[SerializeField] private float m_timeScaleInc = 0.01f;
	[SerializeField] private KeyCode m_timeScaleUpKey = KeyCode.RightArrow;
	[SerializeField] private KeyCode m_timeScaleDownKey = KeyCode.LeftArrow;

	// Internal
	private List<GameObject> m_objectsToShow = new List<GameObject>();
	private Texture2D m_picture = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	protected virtual void Start() {
		// Initialize path input text
		m_pathInput.text = GetSaveDirPath();
		m_pathInput.onEndEdit.AddListener(OnPathChanged);

		// Start with preview popup hidden
		m_picturePreviewPopup.SetActive(false);

		// Init crop toggle
		m_cropToggle.isOn = Prefs.GetBoolEditor(GetKey(CROP_KEY), true);
		m_cropToggle.onValueChanged.AddListener(OnCropToggle);

		// Restore last known time scale
		m_timeScaleSlider.value = Prefs.GetFloatEditor(GetKey(TIME_SCALE_KEY), Time.timeScale);
		OnTimeScaleChanged(m_timeScaleSlider.value);
		m_timeScaleSlider.onValueChanged.AddListener(OnTimeScaleChanged);

		// Restore last known camera values
		m_mainCamera.transform.position = Prefs.GetVector3Editor(GetKey(CAMERA_POS_KEY), m_mainCamera.transform.position);
		m_mainCamera.transform.eulerAngles = Prefs.GetVector3Editor(GetKey(CAMERA_ROT_KEY), m_mainCamera.transform.eulerAngles);
		m_mainCamera.fieldOfView = Prefs.GetFloatEditor(GetKey(CAMERA_FOV_KEY), m_mainCamera.fieldOfView);

		// Init camera sensitivity slider
		m_cameraSensitivitySlider.value = Prefs.GetFloatEditor(GetKey(CAMERA_SENSITIVITY_KEY), 0.5f);
		OnCameraSensitivityChanged(m_cameraSensitivitySlider.value);
		m_cameraSensitivitySlider.onValueChanged.AddListener(OnCameraSensitivityChanged);

		// Initialize capture mode list
		List<string> options = new List<string>((int)CaptureMode.COUNT);
		for(int i = 0; i < (int)CaptureMode.COUNT; ++i) {
			options.Add(((CaptureMode)i).ToString());
		}
		m_modeDropdown.ClearOptions();
		m_modeDropdown.AddOptions(options);
		m_modeDropdown.value = Prefs.GetIntEditor(GetKey(CAPTURE_MODE_KEY), (int)CaptureMode.CAMERA_RENDER);	// Default camera render
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	virtual protected void Update() {
		// Detect input shorcuts

		// Camera movement
		Vector3 offset = Vector3.zero;
		if(Input.GetKey(m_cameraForwardKey)) {
			offset.z = +1f;
		}
		if(Input.GetKey(m_cameraBackwardsKey)) {
			offset.z = -1f;
		}

		if(Input.GetKey(m_cameraRightKey)) {
			offset.x = +1f;
		}
		if(Input.GetKey(m_cameraLeftKey)) {
			offset.x = -1f;
		}

		if(Input.GetKey(m_cameraUpKey)) {
			offset.y = +1f;
		}
		if(Input.GetKey(m_cameraDownKey)) {
			offset.y = -1f;
		}

		// Apply transform
		if(Input.GetKey(m_rotationModifierKey)) {
			// To make rotation more intuitive, switch axis around
			offset = offset * m_cameraRotateSpeed;
			offset = offset * m_cameraSensitivitySlider.value * 2f;	// Scale sensitivity from [0, 1] to [0, 2]
			m_mainCamera.transform.Rotate(
				offset.y * m_cameraRotateSpeed * -1f,
				offset.x * m_cameraRotateSpeed,
				offset.z * m_cameraRotateSpeed,
				Space.Self
			);
		} else {
			offset = offset * m_cameraMoveSpeed;
			offset = offset * m_cameraSensitivitySlider.value * 2f;	// Scale sensitivity from [0, 1] to [0, 2]
			m_mainCamera.transform.Translate(offset, Space.Self);
		}

		// Camera FOV
		float fovOffset = 0f;
		if(Input.GetKey(m_cameraFovDownKey)) {
			fovOffset -= m_cameraFovInc;
		}
		if(Input.GetKey(m_cameraFovUpKey)) {
			fovOffset += m_cameraFovInc;
		}

		// Camera FOV is also influenced by the mouse wheel!
		if(Input.mouseScrollDelta.sqrMagnitude > Mathf.Epsilon) {
			// Change value size based on mouse wheel
			fovOffset += Input.mouseScrollDelta.y * m_cameraFovInc;
		}

		// Apply camera FOV
		if(Math.Abs(fovOffset) > Mathf.Epsilon) {
			m_mainCamera.fieldOfView = Mathf.Clamp(m_mainCamera.fieldOfView + fovOffset, 1f, 179f);
		}

		// Time scale
		if(Input.GetKey(m_timeScaleDownKey)) {
			m_timeScaleSlider.value -= m_timeScaleInc;
		}
		if(Input.GetKey(m_timeScaleUpKey)) {
			m_timeScaleSlider.value += m_timeScaleInc;
		}

		// Take picture
		if(Input.GetKeyDown(m_takePictureKey)) {
			OnTakePictureButton();
		}
	}

	/// <summary>
	/// Component is about to be disabled.
	/// </summary>
	virtual protected void OnDisable() {
		// Store current values to restore it next time
		Prefs.SetIntEditor(GetKey(CAPTURE_MODE_KEY), m_modeDropdown.value);
		Prefs.SetFloatEditor(GetKey(TIME_SCALE_KEY), Time.timeScale);

		Prefs.SetVector3Editor(GetKey(CAMERA_POS_KEY), m_mainCamera.transform.position);
		Prefs.SetVector3Editor(GetKey(CAMERA_ROT_KEY), m_mainCamera.transform.eulerAngles);
		Prefs.SetFloatEditor(GetKey(CAMERA_FOV_KEY), m_mainCamera.fieldOfView);
	}

	//------------------------------------------------------------------------//
	// TO BE IMPLEMENTED BY HEIRS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Put the camera at the default position, rotation, fov.
	/// </summary>
	abstract protected void ResetCamera();
	
	/// <summary>
	/// Get the filename of the screenshot. No extension, can include subfolders.
	/// </summary>
	/// <returns>The filename of the screenshot.</returns>
	abstract protected string GetFilename();

	/// <summary>
	/// Get the default save dir path for the picture.
	/// </summary>
	/// <returns>The default dir path where to store the picture.</returns>
	virtual protected string GetSaveDirPath() {
		#if UNITY_EDITOR
		if(EditorPrefs.HasKey(GetKey(PATH_KEY))) {
			return EditorPrefs.GetString(GetKey(PATH_KEY));
		}
		#endif
		return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Does a screenshot and saves it into the picture texture, overriding its previous content.
	/// </summary>
	/// <returns>The coroutine.</returns>
	private IEnumerator TakePicture() {
		// [AOC] We're not using Application.Screenshot() since we want to have the screenshot in a texture rather than on an image in disk, for sharing and previewing it

		// Aux vars
		int width = 3000;
		int height = (int)(width/m_mainCamera.aspect);	// [AOC] Keep aspect ratio!
		int croppedW = width;
		int croppedH = height;

		// Configure read rectangle area
		// Adjust if cropping
		Rect readRect = new Rect(0, 0, width, height);
		if(Prefs.GetBoolEditor(GetKey(CROP_KEY), true)) {
			// Get the corners of the crop square in world coords
			Vector3[] corners = new Vector3[4];
			m_cropGuide.GetWorldCorners(corners);

			// Convert them to viewport coords using the UI canvas camera
			Vector3[] viewportCorners = new Vector3[4];
			Canvas canv = m_cropGuide.GetComponentInParent<Canvas>();
			for(int i = 0; i < 4; ++i) {
				viewportCorners[i] = canv.worldCamera.WorldToViewportPoint(corners[i]);
			}

			// Change read rectangle
			readRect = new Rect(
				viewportCorners[0].x * width,
				viewportCorners[0].y * height,
				(viewportCorners[2].x - viewportCorners[0].x) * width,
				(viewportCorners[1].y - viewportCorners[0].y) * height
			);

			// Update vars
			croppedW = Mathf.RoundToInt(readRect.width);
			croppedH = Mathf.RoundToInt(readRect.height);
		}

		// If the texture is not created, do it now
		// If the screen size has changed, just resize the texture
		if(m_picture == null) {
			m_picture = new Texture2D(croppedW, croppedH, TextureFormat.ARGB32, false);
		} else if(m_picture.width != croppedW || m_picture.height != croppedH) {
			m_picture.Resize(croppedW, croppedH);
		}

		// Hide all UI elements
		m_objectsToShow.Clear();	// Only those that were actually active will be restored
		for(int i = 0; i < m_objectsToHide.Count; i++) {
			if(m_objectsToHide[i].activeSelf) {
				m_objectsToHide[i].SetActive(false);
				m_objectsToShow.Add(m_objectsToHide[i]);
			}
		}

		// Wait until the end of the frame so the "hide" is actually applied
		yield return new WaitForEndOfFrame();

		// Take the screenshot!
		// Use the proper capture technique
		CaptureMode mode = (CaptureMode)m_modeDropdown.value;
		switch(mode) {
			case CaptureMode.SIMPLE_CAPTURE:
			case CaptureMode.CHROMA_CAPTURE: {
				// Read screen contents into the texture
				m_picture.ReadPixels(readRect, 0, 0);

				// Only for chroma: replace background pixels with transparent
				if(mode == CaptureMode.CHROMA_CAPTURE) {
					Color backgroundColor = m_mainCamera.backgroundColor;
					Color transparentColor = Colors.transparentBlack;
					Color[] pixels = m_picture.GetPixels();
					for(int i = 0; i < pixels.Length; i++) {
						if(pixels[i] == backgroundColor) {
							pixels[i] = transparentColor;
						}
					}
					m_picture.SetPixels(pixels);
				}

				// Save texture
				m_picture.Apply();
			} break;
		
			case CaptureMode.CAMERA_RENDER: {
				// Alternative method: capture only what the camera is rendering
				// Setup camera with transparent background (backup current color first)
				Color backgroundColor = m_mainCamera.backgroundColor;
				m_mainCamera.backgroundColor = Colors.transparentBlack;

				// Create a temporal render texture and render the current camera viewport to it
				RenderTexture rt = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32);
				m_mainCamera.targetTexture = rt;
				m_mainCamera.Render();
				RenderTexture.active = rt;

				// Read pixels from the render texture to our saved texture 2D
				m_picture.ReadPixels(readRect, 0, 0);
				m_picture.Apply();

				// Clean up
				m_mainCamera.targetTexture = null;
				m_mainCamera.backgroundColor = backgroundColor;
				RenderTexture.active = null; // added to avoid errors 
				DestroyImmediate(rt);
			} break;
		}

		// Give it some time
		yield return new WaitForSecondsRealtime(0.25f);

		// Restore disabled objects
		for(int i = 0; i < m_objectsToShow.Count; i++) {
			m_objectsToShow[i].SetActive(true);
		}

		// Initialize and show picture preview popup
		m_picturePreview.texture = m_picture;
		m_picturePreview.color = Color.white;	// Remove placeholder color
		m_picturePreviewRatioFitter.aspectRatio = (float)m_picture.width / (float)m_picture.height;	// Match photo's aspect ratio
		m_picturePreviewPopup.SetActive(true);
	}

	/// <summary>
	/// Compose the prefs key for the given subkey and this object.
	/// </summary>
	/// <returns>The prefs key, composed by this object's name and the requested subkey.</returns>
	/// <param name="_subkey">Subkey to be obtained. One of the _KEY constants in this class.</param>
	protected string GetKey(string _subkey) {
		return this.name + _subkey;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Capture and save the current screen.
	/// </summary>
	public void OnTakePictureButton() {
		// Do it in a coroutine to wait until the end of the frame
		StartCoroutine(TakePicture());
	}

	/// <summary>
	/// The picture is ok, save it to disk.
	/// </summary>
	public void OnSavePictureButton() {
		// Skip if picture not valid
		if(m_picture == null) return;

		// Compose screenshot path
		string filePath = GetSaveDirPath() + "/" + GetFilename() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
		Debug.Log("Saving screenshot at " + filePath);

		// Overwrite any existing picture with the same name
		if(File.Exists(filePath)) {
			File.Delete(filePath);
		}

		// Save picture!
		byte[] bytes = m_picture.EncodeToPNG();
		File.WriteAllBytes(filePath, bytes);

		// Close popup
		m_picturePreviewPopup.SetActive(false);
	}

	/// <summary>
	/// Timescale slider has changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnTimeScaleChanged(float _newValue) {
		// Apply to time scale
		Time.timeScale = m_timeScaleSlider.value;

		// Update text
		m_timeScaleText.text = m_timeScaleSlider.value.ToString("0.00");
	}

	/// <summary>
	/// The path input field has changed.
	/// </summary>
	/// <param name="_newPath">The new path.</param>
	public void OnPathChanged(string _newPath) {
		// Store new path in prefs
		Prefs.SetStringEditor(GetKey(PATH_KEY), _newPath);
	}

	/// <summary>
	/// Crop toggle has changed.
	/// </summary>
	/// <param name="_toggle">Toggled on or off?.</param>
	public void OnCropToggle(bool _toggle) {
		Prefs.SetBoolEditor(GetKey(CROP_KEY), _toggle);
	}

	/// <summary>
	/// The camera sensitivity slider has changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnCameraSensitivityChanged(float _newValue) {
		// Store new sensitivity
		Prefs.SetFloatEditor(GetKey(CAMERA_SENSITIVITY_KEY), _newValue);

		// Update text
		m_cameraSensitiviyText.text = _newValue.ToString("0.00");
	}

	/// <summary>
	/// Reset camera's position, FOV, rotation.
	/// </summary>
	public void OnResetCameraButton() {
		ResetCamera();
	}
}