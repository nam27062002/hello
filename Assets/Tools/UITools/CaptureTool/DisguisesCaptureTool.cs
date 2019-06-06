// DisguisesCaptureTool.cs
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
public class DisguisesCaptureTool : CaptureTool {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected const string DRAG_CONTROL_VALUE = ".DragControlValue";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Separator("Disguise Setup")]
	[SerializeField] private Dropdown m_dragonSkuDropdown = null;
	[SerializeField] private Dropdown m_disguiseSkuDropdown = null;
	[SerializeField] private MenuDragonLoader m_dragonLoader = null;
	[SerializeField] private DragControlRotation m_dragControl = null;

	private string selectedDragonSku {
		get { return m_dragonSkuDropdown.options[m_dragonSkuDropdown.value].text; }
	}

	private string selectedDisguiseSku {
		get { return m_disguiseSkuDropdown.options[m_disguiseSkuDropdown.value].text; }
	}
	
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

		// Initialize dragon dropdown
		List<string> dragonSkuList = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DRAGONS);
		m_dragonSkuDropdown.ClearOptions();
		m_dragonSkuDropdown.AddOptions(dragonSkuList);
		m_dragonSkuDropdown.value = 0;
		OnDragonSkuChanged(0);	// Force an initialization of the disguises dropdown
		m_dragonSkuDropdown.onValueChanged.AddListener(OnDragonSkuChanged);

		// Setup drag control
		m_dragControl.value = Prefs.GetVector2Editor(GetKey(DRAG_CONTROL_VALUE));
		m_dragControl.forceInitialValue = true;
		m_dragControl.initialValue = m_dragControl.value;
		m_dragControl.InitFromTarget(m_dragonLoader.transform, true);
		m_dragControl.forceInitialValue = false;

		// Load initial dragon
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
			new Vector3(1f, 2f, -10f),
			Quaternion.Euler(0f, 0f, 0f)
		);

		m_mainCamera.fieldOfView = 30f;
	}

	/// <summary>
	/// Get the filename of the screenshot. No extension, can include subfolders.
	/// </summary>
	/// <returns>The filename of the screenshot.</returns>
	override protected string GetFilename() {
		return selectedDisguiseSku;	// Each disguise sku is already unique and contains the dragon sku
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The selected option in the dragon sku has changed.
	/// </summary>
	/// <param name="_newValue">New selected value.</param>
	public void OnDragonSkuChanged(int _newValue) {
		// Get all disguises linked to the selected dragon
		List<DefinitionNode> disguiseDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", selectedDragonSku);

		// Update disguises dropdown with new list
		m_disguiseSkuDropdown.ClearOptions();
		if(disguiseDefs != null) {
			List<string> options = new List<string>(disguiseDefs.Count);
			for(int i = 0; i < disguiseDefs.Count; i++) {
				options.Add(disguiseDefs[i].sku);
			}
			m_disguiseSkuDropdown.AddOptions(options);
		}

		// Select first skin as default
		m_disguiseSkuDropdown.value = 0;
	}

	/// <summary>
	/// Reload dragon preview button has been pressed.
	/// </summary>
	public void OnLoadButton() {
		// Just do it
		m_dragonLoader.LoadDragon(selectedDragonSku, selectedDisguiseSku);
	}
}