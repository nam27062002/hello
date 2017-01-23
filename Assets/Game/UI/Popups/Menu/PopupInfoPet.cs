// PopupInfoPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to show extra info of a pet in the pets screen.
/// </summary>
public class PopupInfoPet : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/PF_PopupInfoPet";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UIScene3DLoader m_preview = null;
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Localizer m_infoText = null;
	[SerializeField] private PowerTooltip m_powerInfo = null;
	[Space]
	[SerializeField] private GameObject m_lockedInfo = null;
	[SerializeField] private GameObject m_ownedInfo = null;

	// Internal
	private DefinitionNode m_def = null;
	
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
	/// Called every frame
	/// </summary>
	private void Update() {

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
	/// Initialize the popup with the given pet info.
	/// </summary>
	/// <param name="_petDef">Pet definition used for the initialization.</param>
	public void InitFromDef(DefinitionNode _petDef) {
		// Skip if definition is not valid
		if(_petDef == null) return;

		// Store definition
		m_def = _petDef;

		// Load 3D preview
		MenuPetLoader petLoader = m_preview.scene.FindComponentRecursive<MenuPetLoader>();
		if(petLoader != null) {
			petLoader.Load(m_def.sku);
			//petLoader.petInstance.SetAnim(MenuPetPreview.Anim.IDLE);	// [AOC] TODO!! Pose the pet

			// Rotate!
			petLoader.gameObject.transform.DOLocalRotate(petLoader.gameObject.transform.localRotation.eulerAngles + Vector3.up * 360f, 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetRecyclable(true);
		}

		// Initialize name and description texts
		if(m_nameText != null) m_nameText.Localize(m_def.Get("tidName"));
		if(m_infoText != null) m_infoText.Localize(m_def.Get("tidDesc"));

		// Initialize power info
		if(m_powerInfo != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
			m_powerInfo.InitFromDefinition(powerDef);
		}

		// Initialize lock state
		bool owned = UsersManager.currentUser.petCollection.IsPetUnlocked(m_def.sku);
		if(m_ownedInfo != null) m_ownedInfo.SetActive(owned);
		if(m_lockedInfo != null) m_lockedInfo.SetActive(!owned);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}