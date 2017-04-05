// UIScene3D.cs
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Full 3D scene setup to be rendered on a RenderTexture to be integrated into
/// the UI.
/// The game object using this component should have the following structure:
/// <c>
/// + UIScene3D
///   - Camera
///   - HierarchyToRender
/// </c>
/// If a camera is not found, a new one will be created with default parameters.
/// Either way, a new render texture will be created and assigned to the camera.
/// </summary>
public class UIScene3D : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	protected Camera m_camera = null;
	new public Camera camera {
		get { return m_camera; }
	}

	protected RenderTexture m_renderTexture = null;
	public RenderTexture renderTexture {
		get { return m_renderTexture; }
	}

	// Persist through scenes
	[SerializeField] protected bool m_persistent = false;
	public bool persistent {
		get { return m_persistent; }
		set { m_persistent = value; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Find camera within the hierarchy
		// If not found, create a new one with default parameters
		m_camera = GetComponentInChildren<Camera>(true);
		if(m_camera == null) {
			// Create container object
			GameObject cameraObj = new GameObject("Camera");
			cameraObj.transform.SetParent(this.transform, false);
			cameraObj.transform.localPosition = Vector3.back * 10f;
			cameraObj.transform.LookAt(this.transform.position);

			// Create and initialize camera component
			m_camera = cameraObj.AddComponent<Camera>();
			m_camera.orthographic = false;
			m_camera.clearFlags = CameraClearFlags.Color;
			m_camera.backgroundColor = new Color(1f, 0f, 0f, 0f);
			m_camera.cullingMask = (1 << LayerMask.NameToLayer(UIScene3DManager.LAYER_NAME));
			m_camera.nearClipPlane = 1f;
			m_camera.farClipPlane = 100f;
		}

		// Create a new render texture and set it as the camera render target
		m_renderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);	// Might need a bigger one
		m_renderTexture.Create();
		m_camera.targetTexture = m_renderTexture;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Destroy created render texture as well
		if(m_renderTexture != null) {
			m_renderTexture.Release();
			Destroy(m_renderTexture);
			m_renderTexture = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create a new game object setup to render this scene.
	/// Specifically, it will contain a RectTransform, a RawImage and an AspectRatioFitter components.
	/// As many as desired can be created, just be sure to manage their destruction as well.
	/// </summary>
	/// <param name="_name">Name to add to the new game object.</param>
	/// <returns>The newly created object.</returns>
	public GameObject CreateRawImage(string _name) {
		// Create a new game object
		GameObject newObj = new GameObject(_name);

		// Add rect transform with default settings
		RectTransform tr = newObj.AddComponent<RectTransform>();
		tr.pivot = new Vector2(0.5f, 0.5f);
		tr.localPosition = Vector2.zero;

		// Add RawImage component
		RawImage rw = newObj.AddComponent<RawImage>();
		InitRawImage(ref rw);

		// Add apect ratio fitter - since the render texture is squared, usually we want to keep proportions
		AspectRatioFitter ar = newObj.AddComponent<AspectRatioFitter>();
		ar.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
		ar.aspectRatio = 1f;

		// Done!
		return newObj;
	}

	/// <summary>
	/// Initialize a given RawImage component to render this scene.
	/// </summary>
	/// <param name="_target">The raw image to be used.</param>
	public void InitRawImage(ref RawImage _target) {
		// Check params
		if(_target == null) return;

		// Do it!
		_target.texture = this.renderTexture;
		_target.color = Color.white;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}