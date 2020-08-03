// PetsCaptureTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
/// Specialized capture tool to take snapshots of dragon skins.
/// </summary>
public class PetsCaptureTool : CaptureTool {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected const string DRAG_CONTROL_VALUE = ".DragControlValue";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Pet Setup")]
	[SerializeField] private Dropdown m_petsDropdown = null;
	[SerializeField] private MenuPetLoader m_petLoader = null;
	[SerializeField] private DragControlRotation m_dragControl = null;

	private string selectedPetSku {
		get { return m_petDefs[m_petsDropdown.value].sku; }
	}

	// Internal
	private List<DefinitionNode> m_petDefs = new List<DefinitionNode>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Call parent
		base.Start();

		// Same for localization
		LocalizationManager.SharedInstance.SetLanguage("lang_english");

		// Gather all pet definitions and sort them alphabetically
		m_petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		DefinitionsManager.SharedInstance.SortByProperty(ref m_petDefs, "sku", DefinitionsManager.SortType.ALPHANUMERIC);

		// Initialize dropdown
		// For clarity, show pet name next to its sku
		List<string> options = new List<string>(m_petDefs.Count);
		for(int i = 0; i < m_petDefs.Count; ++i) {
			options.Add(m_petDefs[i].sku + " (" + m_petDefs[i].GetLocalized("tidName") + ")");
		}
		m_petsDropdown.ClearOptions();
		m_petsDropdown.AddOptions(options);
		m_petsDropdown.value = 0;

		// Setup drag control
		m_dragControl.value = Prefs.GetVector2Editor(GetKey(DRAG_CONTROL_VALUE));
		m_dragControl.forceInitialValue = true;
		m_dragControl.initialValue = m_dragControl.value;
		m_dragControl.InitFromTarget(m_petLoader.transform, true);
		m_dragControl.forceInitialValue = false;

		// Load initial pet
		OnLoadButton();
	}

	/// <summary>
	/// Component is about to be disabled.
	/// </summary>
	override protected void OnDisable() {
		// Store some values to prefs
		Prefs.SetVector2Editor(GetKey(DRAG_CONTROL_VALUE), m_dragControl.value);

		// Call parent
		base.OnDisable();
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
		return selectedPetSku;	// Each pet sku is already unique
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reload dragon preview button has been pressed.
	/// </summary>
	public void OnLoadButton() {
		m_petLoader.Load(selectedPetSku);
	}
}