// PopupDragonInforInitializer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to initialize a dragon info popup.
/// To use in combination with PopupLauncher.
/// </summary>
public class PopupDragonInfoInitializer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Dragon {
		CURRENT,
		SELECTED,
		CUSTOM
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Mode
	[SerializeField] private Dragon m_targetDragon = Dragon.CURRENT;

	// Target dragon
	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_dragonSku = "";

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
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
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	/// <summary>
	/// Something has changed on the inspector.
	/// </summary>
	private void OnValidate() {
		// If selected mode is not CUSTOM, set dragon sku to empty
		if(m_targetDragon != Dragon.CUSTOM) {
			m_dragonSku = "";
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the given popup with the target dragon's info.
	/// Connect to the PopupLauncher's OnInit callback.
	/// </summary>
	/// <param name="_popup">Target popup.</param>
	public void InitPopup(PopupController _popup) {
		// Get the dragon info popup component
		PopupDragonInfo dragonInfoPopup = _popup.GetComponent<PopupDragonInfo>();
		Debug.Assert(dragonInfoPopup != null, "Connected popup doesn't have the required DragonInfoPopup component.");

		// Initialize with the target dragon
		switch(m_targetDragon) {
			case Dragon.CURRENT:	dragonInfoPopup.Init(DragonManager.CurrentDragon);	break;
			case Dragon.SELECTED:	dragonInfoPopup.Init(DragonManager.GetDragonData(InstanceManager.menuSceneController.selectedDragon));	break;
			case Dragon.CUSTOM: 	dragonInfoPopup.Init(DragonManager.GetDragonData(m_dragonSku));	break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//


}