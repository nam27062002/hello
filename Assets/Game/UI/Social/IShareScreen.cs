// ShareScreenSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

#define RENDER_TEXTURE

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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] protected Camera m_camera = null;
	[SerializeField] protected RawImage m_qrCodeHolder = null;
	[SerializeField] protected Localizer m_callToActionText = null;
	[Space]
	[SerializeField] protected Texture2D m_qrLogoTex = null;

	// Internal references
	protected DefinitionNode m_shareLocationDef = null;
	protected string m_url = null;
	protected Texture2D m_qrCodeTex = null;

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
	/// Take a picture!
	/// </summary>
	public void TakePicture() {
		// Ignore if a capture is ongoing
		if(m_coroutine != null) return;

		// Do it in a coroutine to wait until the end of the frame
		m_coroutine = StartCoroutine(TakePictureInternal());
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected virtual void OnDestroy() {
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
	/// Initialize by cloning the position and camera parameters of the given camera.
	/// </summary>
	/// <param name="_camera">Camera to be used as reference.</param>
	protected void SetRefCamera(Camera _camera) {
		// Copy camera's position and rotation
		this.transform.CopyFrom(_camera.transform);

		// Copy background color
		m_camera.backgroundColor = _camera.backgroundColor;

		// Copy FOV
		m_camera.fieldOfView = _camera.fieldOfView;

		// Copy planes
		m_camera.nearClipPlane = _camera.nearClipPlane;
		m_camera.farClipPlane = _camera.farClipPlane;

		// Render camera on top of any other camera
		// [AOC] This will hide UI and other elements we don't want to capture
		m_camera.depth = CAMERA_DEPTH;
	}

	/// <summary>
	/// Does a screenshot and saves it into the picture texture, overriding its previous content.
	/// </summary>
	/// <returns>The coroutine.</returns>
	private IEnumerator TakePictureInternal() {
		// Trigger Flash FX!
		PopupManager.OpenPopupInstant(PopupFlashFX.PATH);

		// Wait until the end of the frame so everything is refreshed
		//yield return new WaitForEndOfFrame();
		// [AOC] For some reason, waiting just one frame doesn't give enough time for everything to get properly setup. Wait a couple of frames instead.
		for(int i = 0; i < 2; ++i) {
			yield return new WaitForEndOfFrame();
		}

		// Take the screenshot!
		// [AOC] We're not using Application.Screenshot() since we want to have the screenshot in a texture rather than on an image in disk, for sharing and previewing it
		// [AOC] We have 2 options here Texture.ReadPixels() or RenderTexture
#if RENDER_TEXTURE
		// Process is explained here: https://docs.unity3d.com/ScriptReference/Camera.Render.html

		// Re-use manager's textures
		RenderTexture renderTex = ShareScreensManager.renderTex;
		Texture2D pictureTex = ShareScreensManager.captureTex;

		// Use a temporal render texture and make the camera render to it
		m_camera.targetTexture = renderTex;
		RenderTexture currentRT = RenderTexture.active; // Backup
		RenderTexture.active = renderTex;

		// [AOC] Because we want the UI to render on top of the 3D background, we'll do 2 render passes changing the camera layer mask
		// 1st pass: Background
		m_camera.clearFlags = CameraClearFlags.Color;
		m_camera.cullingMask = GetBackgroundCullingMask();
		m_camera.Render();

		// 2nd pass: UI
		m_camera.clearFlags = CameraClearFlags.Depth;
		m_camera.cullingMask = LayerMask.GetMask("UI");
		m_camera.Render();

		// Read pixels from the render texture to our saved texture 2D
		pictureTex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
		pictureTex.Apply();

		// Clean up
		m_camera.targetTexture = null;
		RenderTexture.active = currentRT; // Restore
#else
		// Re-use manager's textures
		Texture2D pictureTex = ShareScreensManager.captureTex;

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
			Screen.width - captureSize.x / 2f,	// Centered to the screen
			Screen.height - captureSize.y / 2f, // Centered to the screen
			captureSize.x,
			captureSize.y
		);

		// Read screen contents into a tmp texture
		// [AOC] We can't read directly into our texture cause they have different sizes!
		Texture2D captureTex = new Texture2D((int)captureSize.x, (int)captureSize.y, TextureFormat.RGB24, false);
		captureTex.ReadPixels(captureRect, 0, 0);

		// Resize image to the target size
		// [AOC] TODO!! Use a nicer algorithm (http://blog.collectivemass.com/2014/03/resizing-textures-in-unity/)
		Texture2D sourceTex = captureTex;
		Texture2D targetTex = pictureTex;
		Color[] sourceTexData = sourceTex.GetPixels();
		Color[] targetTexData = targetTex.GetPixels();
		Vector2 sourcePos = new Vector2();
		Color pixel = new Color();
		for(int x = 0; x < targetTex.width; ++x) {
			for(int y = 0; y < targetTex.height; ++y) {
				// Figure out matching position in source texture
				sourcePos.y = Mathf.InverseLerp(0, targetTex.width, x) * (sourceTex.width - 1);
				sourcePos.x = Mathf.InverseLerp(0, targetTex.height, y) * (sourceTex.height - 1);

				// Get matching color in the source texture
				pixel = sourceTexData[(int)((sourcePos.y * sourceTex.width) + sourcePos.x)];

				// Copy pixel
				targetTexData[(y * targetTex.width) + x] = pixel;
			}
		}

		// Upload to the GPU
		targetTex.SetPixels(targetTexData);
		targetTex.Apply();
#endif

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
		popup.Init(pictureTex, GetPrewrittenCaption(), string.Empty);	// Don't care about the popup's title
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