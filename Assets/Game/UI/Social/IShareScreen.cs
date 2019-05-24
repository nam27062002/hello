// ShareScreenSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual layout for a specific share screen.
/// Can be inherited for setups requiring special initializations.
/// </summary>
public abstract class IShareScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected const int QR_SIZE = 128;
	protected const int CAMERA_DEPTH = 20;  // [AOC] This will hide UI and other elements we don't want to capture




	public enum CaptureMode {
		RENDER_TEXTURE,	// Capture using only the prefab camera. Less memory usage, but requires more setup
		SCREEN_CAPTURE	// Capture background with a snapshot of the current screen content, UI using prefab camera. Requires one temp texture of the size of the screen, and objects that shouldn't be rendered must be hidden manually.
	};

	protected class CameraData {
		public bool ortographic = false;
		public float ortographicSize = 10f;
		public float near = 1f;
		public float far = 1000f;

		public void InitFromCamera(Camera _camera) {
			ortographic = _camera.orthographic;
			ortographicSize = _camera.orthographicSize;
			near = _camera.nearClipPlane;
			far = _camera.farClipPlane;
		}

		public void ApplyToCamera(Camera _camera) {
			_camera.orthographic = ortographic;
			_camera.orthographicSize = ortographicSize;
			_camera.nearClipPlane = near;
			_camera.farClipPlane = far;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] protected Camera m_camera = null;
	[SerializeField] protected RawImage m_qrCodeHolder = null;
	[SerializeField] protected Localizer m_callToActionText = null;
	[Space]
	[SerializeField] protected Texture2D m_qrLogoTex = null;

    // Amount of frames to wait before taking the screenshot
    protected int captureDelayInFrames = 10; // 330 ms aprox.

    // Internal references
    protected DefinitionNode m_shareLocationDef = null;
	protected string m_url = null;
	protected Texture2D m_qrCodeTex = null;

	protected Camera m_refCamera = null;
	protected CameraData m_cameraDataBackup = new CameraData();

	// Internal logic
	protected Coroutine m_coroutine = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		m_camera.enabled = false;
	}

    /// <summary>
    /// Operations that need to be made before taking the screenshot
    /// </summary>
    protected virtual void CapturePreprocess() { }

	/// <summary>
	/// Take a picture!
	/// </summary>
	/// <param name="_captureMode">Technique use to take the screenshot.</param>
	public void TakePicture(CaptureMode _captureMode = CaptureMode.RENDER_TEXTURE) {
		// Ignore if a capture is ongoing
		if(m_coroutine != null) return;

		// Do it in a coroutine to wait until the end of the frame
		m_coroutine = StartCoroutine(TakePictureInternal(_captureMode));
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
		// Clear external references
		m_refCamera = null;

		// Remove ourselves from the manager
		ShareScreensManager.RemoveScreen(this);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize using a specific location definition.
	/// </summary>
	/// <param name="_shareLocationSku">The location where the setup has been triggered.</param>
	protected void SetLocation(string _shareLocationSku) {
		// Get location definition
		m_shareLocationDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHARE_LOCATIONS, _shareLocationSku);
		Debug.Assert(m_shareLocationDef != null, "Share Location Definition for " + _shareLocationSku + " couldn't be found!"); 

		// Figure out URL to use
		if(PlatformUtils.Instance.IsChina()) {
			m_url = m_shareLocationDef.GetAsString("urlChina");
		} else {
			m_url = m_shareLocationDef.GetAsString("url");
		}

		// Generate and initialize QR code
		if(m_qrCodeHolder != null) {
			m_qrCodeTex = QRGenerator.GenerateQR(
				m_url, QR_SIZE,
				Color.black, Color.white,
				QRGenerator.ErrorTolerance.PERCENT_30,
				m_qrLogoTex, 0.3f
			);
			m_qrCodeHolder.texture = m_qrCodeTex;
			m_qrCodeHolder.color = Color.white; // Remove any placeholder tint
		}

		// Initialize call to action text
		if(m_callToActionText != null) {
			m_callToActionText.Localize(m_shareLocationDef.GetAsString("tidCallToAction"));
		}
	}

	/// <summary>
	/// Define the camera used as reference to capture the background.
	/// </summary>
	/// <param name="_camera">Camera to be used as reference.</param>
	protected void SetRefCamera(Camera _camera) {
		// Just store it for now
		m_refCamera = _camera;
	}

	/// <summary>
	/// Clone the position and camera parameters of the reference.
	/// </summary>
	protected void ApplyRefCamera() {
		// Ignore if null
		if(m_refCamera == null) return;

		// Copy camera's position and rotation
		this.transform.CopyFrom(m_refCamera.transform);

		// Copy background color
		m_camera.backgroundColor = m_refCamera.backgroundColor;

		// Copy FOV
		m_camera.fieldOfView = m_refCamera.fieldOfView;

		// Copy planes
		m_camera.nearClipPlane = m_refCamera.nearClipPlane;
		m_camera.farClipPlane = m_refCamera.farClipPlane;

		// Copy camera type
		m_camera.orthographic = m_refCamera.orthographic;
		if(m_refCamera.orthographic) {
			m_camera.orthographicSize = m_refCamera.orthographicSize;
		}

		// Render camera on top of any other camera
		// [AOC] This will hide UI and other elements we don't want to capture
		m_camera.depth = CAMERA_DEPTH;
	}

	/// <summary>
	/// Does a screenshot and saves it into the picture texture, overriding its previous content.
	/// </summary>
	/// <returns>The coroutine.</returns>
	/// <param name="_captureMode">Technique use to take the screenshot.</param>
	private IEnumerator TakePictureInternal(CaptureMode _captureMode) {

		// With the new OTA features, we have to wait until the 3d icon is loaded.
		for(int i = 0; i < captureDelayInFrames; ++i) {
			yield return new WaitForEndOfFrame();
		}

        // Do some stuff if needed before the screenshot
        CapturePreprocess();

		// Take the screenshot!
		// [AOC] We're not using Application.Screenshot() since we want to have the screenshot in a texture rather than on an image in disk, for sharing and previewing it
		Texture2D pictureTex = null;
		switch(_captureMode) {
			case CaptureMode.RENDER_TEXTURE: pictureTex = TakeRenderTexture(); break;
			case CaptureMode.SCREEN_CAPTURE: pictureTex = TakeScreenCapture(); break;
		}

		// Trigger Flash FX!
		PopupManager.OpenPopupInstant(PopupFlashFX.PATH);

		// Give it some time
		yield return new WaitForSeconds(0.25f);

		// Clear coroutine reference
		m_coroutine = null;

		// Clean up memory
		m_qrCodeHolder.texture = null;
		DestroyImmediate(m_qrCodeTex);
		m_qrCodeTex = null;

		// Disable ourselves
		this.gameObject.SetActive(false);

		// Open "Share" popup
		PopupPhotoShare popup = PopupManager.OpenPopupInstant(PopupPhotoShare.PATH).GetComponent<PopupPhotoShare>();
		popup.Init(
			pictureTex, 
			EmojiManager.ReplaceEmojis(GetPrewrittenCaption()),		// Replace emoji tags! 
			string.Empty, 
			m_shareLocationDef.sku
		);	// Don't care about the popup's title
	}

	/// <summary>
	/// Take a screen capture using a render texture to render the contents of the camera.
	/// </summary>
	/// <returns>The capture.</returns>
	private Texture2D TakeRenderTexture() {
		// Process is explained here: https://docs.unity3d.com/ScriptReference/Camera.Render.html

		// Re-use manager's textures
		RenderTexture renderTex = ShareScreensManager.renderTex;
		Texture2D pictureTex = ShareScreensManager.captureTex;

		// Use a temporal render texture and make the camera render to it
		m_camera.targetTexture = renderTex;
		RenderTexture currentRT = RenderTexture.active; // Backup
		RenderTexture.active = renderTex;

		// [AOC] Because we want the UI to render on top of the 3D background, we'll do 2 render passes changing the camera layer mask
		// 1st pass: Background ------------------------------------------------
		// Backup original camera values
		m_cameraDataBackup.InitFromCamera(m_camera);

		// Apply reference camera
		ApplyRefCamera();

		// Change some extra camera params
		m_camera.clearFlags = CameraClearFlags.Color;
		m_camera.cullingMask = GetBackgroundCullingMask();

		// Capture!
		m_camera.Render();

		// 2nd pass: UI --------------------------------------------------------
		// Restore camera params
		m_cameraDataBackup.ApplyToCamera(m_camera);

		// Change some extra camera params
		m_camera.clearFlags = CameraClearFlags.Depth;
		m_camera.cullingMask = LayerMask.GetMask("UI");

		// Capture!
		m_camera.Render();

		// ---------------------------------------------------------------------

		// Read pixels from the render texture to our saved texture 2D
		pictureTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		pictureTex.Apply();

		// Clean up
		m_camera.targetTexture = null;
		RenderTexture.active = currentRT; // Restore

		return pictureTex;
	}

	/// <summary>
	/// Take a screen capture doing a full screen capture and automatically resampling it.
	/// </summary>
	/// <returns>The capture.</returns>
	private Texture2D TakeScreenCapture() {
		// Capture central area of the screen
		Texture2D sourceTex = CaptureScreen();

		// Dump capture into a render texture of the target size
		sourceTex.filterMode = FilterMode.Point;
		RenderTexture renderTex = ShareScreensManager.renderTex;	// Re-use manager's textures
		renderTex.filterMode = FilterMode.Point;
		RenderTexture.active = renderTex;
		Graphics.Blit(sourceTex, renderTex);

		// Now render UI on top, using this share screen's camera
		m_camera.targetTexture = renderTex;
		RenderTexture activeRTBackup = RenderTexture.active; // Backup
		RenderTexture.active = renderTex;

		m_camera.clearFlags = CameraClearFlags.Depth;
		m_camera.cullingMask = LayerMask.GetMask("UI");
		m_camera.Render();

		// Dump render texture's content into a Texture2D
		Texture2D targetTex = ShareScreensManager.captureTex;	// Reuse manager's textures
		targetTex.ReadPixels(new Rect(0, 0, ShareScreensManager.CAPTURE_SIZE.x, ShareScreensManager.CAPTURE_SIZE.y), 0, 0);
		targetTex.Apply();

		// Clean up
		m_camera.targetTexture = null;
		RenderTexture.active = activeRTBackup; // Restore
		DestroyImmediate(sourceTex);
		sourceTex = null;

		return targetTex;
	}

	/// <summary>
	/// Capture the central area of the screen and store it into a texture.
	/// </summary>
	/// <returns>The capture.</returns>
	private Texture2D CaptureScreen() {
		// Compute which area of the screen to read
		// Fit photo size into screen size
		Vector2 scaleRatio = new Vector2(
			(float)Screen.width / (float)ShareScreensManager.CAPTURE_SIZE.x,
			(float)Screen.height / (float)ShareScreensManager.CAPTURE_SIZE.y
		);

		Vector2 captureSize = new Vector2();
		if(scaleRatio.x < scaleRatio.y) {
			captureSize.x = scaleRatio.x * ShareScreensManager.CAPTURE_SIZE.x;
			captureSize.y = scaleRatio.x * ShareScreensManager.CAPTURE_SIZE.y;
		} else {
			captureSize.x = scaleRatio.y * ShareScreensManager.CAPTURE_SIZE.x;
			captureSize.y = scaleRatio.y * ShareScreensManager.CAPTURE_SIZE.y;
		}

		Rect captureRect = new Rect(
			Screen.width / 2f - captureSize.x / 2f,  // Centered to the screen
			Screen.height / 2f - captureSize.y / 2f, // Centered to the screen
			captureSize.x,
			captureSize.y
		);

		// Read screen contents into a tmp texture
		// [AOC] We can't read directly into our texture cause they have different sizes!
		Texture2D captureTex = new Texture2D((int)captureSize.x, (int)captureSize.y, TextureFormat.RGB24, false);
		captureTex.ReadPixels(captureRect, 0, 0);
		captureTex.Apply();

		return captureTex;
	}

	//------------------------------------------------------------------------//
	// VIRTUAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the prewritten caption for the current setup.
	/// Override for custom formatting.
	/// </summary>
	/// <returns>The prewritten caption, already localized.</returns>
	protected virtual string GetPrewrittenCaption() {
		if(m_shareLocationDef == null) return string.Empty;
		return m_shareLocationDef.GetLocalized("tidPrewrittenCaption", m_url);	// URL is always a parameter
	}

	/// <summary>
	/// Layer mask for the background render.
	/// </summary>
	/// <returns>The culling mask to be assigned to the camera for the background render.</returns>
	protected virtual int GetBackgroundCullingMask() {
		return LayerMask.GetMask("Ground");
	}
}