// MissionIconCaptureTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialized capture tool to take snapshots of 3D mission icons.
/// </summary>
public class MissionIconsCaptureTool : CaptureTool {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Icon Setup")]
	[SerializeField] private Dropdown m_dropdown = null;
	[SerializeField] private BaseIcon m_iconLoader = null;

	private string selectedIconSku {
		get { return m_iconDefs[m_dropdown.value].sku; }
	}

	// Internal
	private List<DefinitionNode> m_iconDefs = new List<DefinitionNode>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Call parent
		base.Start();

		// If definitions are not loaded, do it now
		if(!ContentManager.ready){
			ContentManager.InitContent(true, false);
		}
		HDAddressablesManager.Instance.Initialize();

		// Same for localization
		LocalizationManager.SharedInstance.SetLanguage("lang_english");

		// Gather all pet definitions and sort them alphabetically
		m_iconDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.ICONS);
		DefinitionsManager.SharedInstance.SortByProperty(ref m_iconDefs, "sku", DefinitionsManager.SortType.ALPHANUMERIC);

		// Initialize dropdown
		// For clarity, show pet name next to its sku
		List<string> options = new List<string>(m_iconDefs.Count);
		for(int i = 0; i < m_iconDefs.Count; ++i) {
			bool is3d = m_iconDefs[i].GetAsBool("icon3d", false);
			options.Add(m_iconDefs[i].sku + " (" + (is3d ? "3D" : "2D") + ")");
		}
		m_dropdown.ClearOptions();
		m_dropdown.AddOptions(options);
		m_dropdown.value = 0;

		// Initialize icon loader
		m_iconLoader.OnLoadFinished.AddListener(OnIconLoaded);

		// Load icon
		OnLoadButton();
	}

	/// <summary>
	/// Component is about to be disabled.
	/// </summary>
	override protected void OnDisable() {
		// Call parent
		base.OnDisable();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_iconLoader.OnLoadFinished.RemoveListener(OnIconLoaded);
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Put the camera at the default position, rotation, fov.
	/// </summary>
	protected override void ResetCamera() {
		m_mainCamera.transform.SetPositionAndRotation(
			new Vector3(0f, 0f, -10f),
			Quaternion.Euler(0f, 0f, 0f)
		);

		m_mainCamera.fieldOfView = 30f;
	}

	/// <summary>
	/// Get the filename of the screenshot. No extension, can include subfolders.
	/// </summary>
	/// <returns>The filename of the screenshot.</returns>
	override protected string GetFilename() {
		return selectedIconSku;	// Each sku is already unique
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reload icon button has been pressed.
	/// </summary>
	public void OnLoadButton() {
		m_iconLoader.LoadIcon(selectedIconSku);
	}

	/// <summary>
	/// Icon has finished loading.
	/// </summary>
	public void OnIconLoaded() {
		UbiBCN.CoroutineManager.DelayedCallByFrames(
			() => {
				// Post process: give support for timescale!
				Animator[] animators = m_iconLoader.GetComponentsInChildren<Animator>(true);
				for(int i = 0; i < animators.Length; ++i) {
					animators[i].updateMode = AnimatorUpdateMode.Normal;
				}
			},
			5
		);
	}
}