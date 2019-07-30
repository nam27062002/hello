// ShowcaseScene.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Demo scene to showcase art assets.
/// </summary>
public class ShowcaseScene : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	public MenuDragonLoader[] m_dragonLoaders = new MenuDragonLoader[0];
	public MenuPetLoader[] m_petLoaders = new MenuPetLoader[0];

	[Space]
	public Camera[] m_cameras = new Camera[0];
	private int m_cameraIdx = 0;
	private bool m_init = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Toggle initial camera
		SelectCamera(0);

		// Prepare initialization
		m_init = false;
		if(!ContentManager.ready) {
			ContentManager.InitContent(true, false);
		}
		HDAddressablesManager.Instance.Initialize();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Something changed on the inspector.
	/// </summary>
	private void OnValidate() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Is initialization pending?
		if(!m_init) {
			if(ContentManager.ready && HDAddressablesManager.Instance.IsInitialized()) {
				InternalInit();
				m_init = true;
			}
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the scene when all managers are ready.
	/// </summary>
	private void InternalInit() {
		SelectCamera(0);

		for(int i = 0; i < m_dragonLoaders.Length; ++i) {
			m_dragonLoaders[i].Reload(true);
		}

		for(int i = 0; i < m_petLoaders.Length; ++i) {
			m_petLoaders[i].Reload();
		}
	}

	/// <summary>
	/// Select active camera.
	/// </summary>
	/// <param name="_cameraIdx">Index of the camera to be activated. Will be looped through the total number of cameras.</param>
	public void SelectCamera(int _cameraIdx) {
		// Make sure target camera is valid
		while(_cameraIdx < 0) _cameraIdx += m_cameras.Length;
		while(_cameraIdx >= m_cameras.Length) _cameraIdx -= m_cameras.Length;
		m_cameraIdx = _cameraIdx;

		for(int i = 0; i < m_cameras.Length; ++i) {
			m_cameras[i].gameObject.SetActive(i == m_cameraIdx);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Next camera button.
	/// </summary>
	public void OnNextCamera() {
		SelectCamera(m_cameraIdx + 1);
	}

	/// <summary>
	/// Previous camera button.
	/// </summary>
	public void OnPrevCamera() {
		SelectCamera(m_cameraIdx - 1);
	}

	/// <summary>
	/// Reset current camera to original setup.
	/// </summary>
	public void OnCameraReset() {
		m_cameras[m_cameraIdx].GetComponent<WASDController>().Reset();
	}
}