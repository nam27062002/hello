﻿// MenuDragonLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Helper class to easily instantiate dragon models within the menu.
/// </summary>
public class MenuDragonLoader : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Mode {
		CURRENT_DRAGON,		// Automatically loads and updates CURRENT dragon (UserProfile.currentDragon)
		SELECTED_DRAGON,	// Automatically loads and updates SELECTED dragon (MenuSceneController.selectedDragon)
		MANUAL				// Manual control via the LoadDragon() method and the exposed m_dragonSku parameter
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Mode m_mode = Mode.CURRENT_DRAGON;
	public Mode mode {
		get { return m_mode; }
		set {
			m_mode = value; 
			RefreshDragon();
		}
	}

	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_dragonSku = "";
	public string dragonSku {
		get { return m_dragonSku; }
		set {
			m_dragonSku = value;
			RefreshDragon();
		}
	}

	[SerializeField] private string m_disguiseSku = "";
	public string disguiseSku {
		get { return m_disguiseSku; }
		set {
			m_disguiseSku = value;
			RefreshDragon();
		}
	}

	[Space]
	[HideEnumValues(false, true)]
	[SerializeField] private MenuDragonPreview.Anim m_anim = MenuDragonPreview.Anim.IDLE;
	public MenuDragonPreview.Anim anim {
		get { return m_anim; }
		set { m_anim = value; }
	}

	[SerializeField] private bool m_resetDragonScale = true;
	public bool resetDragonScale {
		get { return m_resetDragonScale; }
		set { m_resetDragonScale = value; }
	}

	[SerializeField] private bool m_showPets = false;
	public bool showPets {
		get { return m_showPets; }
		set { m_showPets = value; }
	}

	[SerializeField] private bool m_hideResultsEquipment = false;
	public bool hideResultsEquipment {
		get { return m_hideResultsEquipment; }
		set { m_hideResultsEquipment = value; }
	}

	[SerializeField] private bool m_removeFresnel = false;
	public bool removeFresnel {
		get { return m_removeFresnel; }
		set { m_removeFresnel = value; }
	}

	[SerializeField] private bool m_keepLayers = false;
	public bool keepLayers {
		get { return m_keepLayers; }
		set { m_keepLayers = value; }
	}

	[SerializeField] private bool m_useResultsScreen = false;
	public bool useResultsScreen {
		get { return m_useResultsScreen; }
		set { m_useResultsScreen = value; }
	}

	[SerializeField] private bool m_allowAltAnimations = true;
	public bool allowAltAnimations {
		get { return m_allowAltAnimations; }
		set { m_allowAltAnimations = value; }
	}

	private bool m_useShadowMaterial = false;
	public bool useShadowMaterial {
		get { return m_useShadowMaterial; }
		set { m_useShadowMaterial = value; RefreshDragon(); }
	}

	// Debug
	[SkuList(DefinitionsCategory.DRAGONS, false)]
	[SerializeField] private string m_placeholderDragonSku = "dragon_classic";	// If the game is not running, we don't have any data on current dragon/skin, so load a placeholder one manually instead

	// Internal
	private MenuDragonPreview m_dragonInstance = null;
	public MenuDragonPreview dragonInstance {
		get { return m_dragonInstance; }
	}

	private ParticleScaler m_pscaler = null;
	public ParticleScaler pscaler {
		get {
			if(m_pscaler == null) {
				// Try getting a defined scaler
				m_pscaler = GetComponent<ParticleScaler>();
				if(m_pscaler == null) {
					// Instantiate a new scaler
					m_pscaler = this.gameObject.AddComponent<ParticleScaler>();
					pscaler.m_resetFirst = true;
					pscaler.m_scaleAllChildren = true;
					pscaler.m_scaleLifetime = false;
					pscaler.m_whenScale = ParticleScaler.WhenScale.MANUALLY;
					pscaler.m_scaleOrigin = ParticleScaler.ScaleOrigin.TRANSFORM_SCALE;
					pscaler.m_transform = this.transform;
				}
			}
			return m_pscaler;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		// Try to find out already instantiated previews of the dragon
		m_dragonInstance = this.GetComponentInChildren<MenuDragonPreview>();
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize loaded dragon (unless using MANUAL mode)
		if(m_mode != Mode.MANUAL) RefreshDragon();
	}


	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Configure the loader with a specific setup.
	/// Doesn't update the current view, if any.
	/// Use either LoadDragon() or RefreshDragon() for that.
	/// </summary>
	/// <param name="_mode">Dragon loading mode.</param>
	/// <param name="_initialAnim">Initial dragon animation.</param>
	/// <param name="_resetScale">Whether to respect dragon's prefab original scale or reset it.</param>
	public void Setup(Mode _mode, MenuDragonPreview.Anim _initialAnim, bool _resetScale) {
		// Store new setup
		m_mode = _mode;
		m_anim = _initialAnim;
		m_resetDragonScale = _resetScale;
	}

	/// <summary>
	/// Load the dragon with the given sku and its default disguise.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be loaded</param>
	public void LoadDragon(string _sku) {
		LoadDragon(_sku, string.Empty);
	}

	/// <summary>
	/// Load the dragon with the given sku.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be loaded</param>
	/// <param name="_disguiseSku">The sku of the disguise to be applied to this dragon.</param> 
	public void LoadDragon(string _sku, string _disguiseSku) {
		// Unload current dragon if any
		UnloadDragon();

		// Load selected dragon
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _sku);
		if(def != null) {
			string prefabColumn = "menuPrefab";
			if (  m_useResultsScreen )
				prefabColumn = "resultsPrefab";
			// Instantiate the prefab and add it as child of this object
			GameObject dragonPrefab = Resources.Load<GameObject>(DragonData.MENU_PREFAB_PATH + def.GetAsString(prefabColumn));
			if(dragonPrefab != null) {
				GameObject newInstance = GameObject.Instantiate<GameObject>(dragonPrefab);
				newInstance.transform.SetParent(this.transform, false);
				newInstance.transform.localPosition = Vector3.zero;
				newInstance.transform.localRotation = Quaternion.identity;

				// Keep layers?
				if(!m_keepLayers) {
					newInstance.SetLayerRecursively(this.gameObject.layer);
				}

				// Store dragon preview and launch the default animation
				m_dragonInstance = newInstance.GetComponent<MenuDragonPreview>();
				m_dragonInstance.SetAnim(m_anim);

				// Reset scale if required
				if(m_resetDragonScale) {
					m_dragonInstance.transform.localScale = Vector3.one;
				}

				// Apply equipment
				DragonEquip equip = m_dragonInstance.GetComponent<DragonEquip>();
				if(equip != null) {
					if ( !Application.isPlaying )
					{
						equip.Init();
					}
					// Apply disguise (if any)
					if(!string.IsNullOrEmpty(_disguiseSku)) {
						equip.EquipDisguise(_disguiseSku);
					}

					// Toggle pets
					equip.TogglePets(m_showPets, false);

					if (m_hideResultsEquipment)
						equip.HideResultsEquipment();
				}

				// Remove fresnel if required
				if(m_removeFresnel) {
					m_dragonInstance.SetFresnelColor(Color.black);
				}

				// Apply shadow material if required
				if(m_useShadowMaterial) {
					m_dragonInstance.equip.EquipDisguiseShadow();
				}

				// Allow alt animations?
				m_dragonInstance.allowAltAnimations = m_allowAltAnimations;

				// Make sure particles are properly scaled as well
				pscaler.ReloadOriginalData();
				pscaler.DoScale();
				UbiBCN.CoroutineManager.DelayedCallByFrames(() => { pscaler.DoScale(); }, 1);	// In case some initial scale transformation is performed during this frame (i.e. child of a UI3DScaler)
			}
		}

		// Update dragon and disguise skus
		m_dragonSku = _sku;
		m_disguiseSku = _disguiseSku;
	}

	/// <summary>
	/// Reload dragon preview based on mode.
	/// </summary>
	public void RefreshDragon() {
		// Load different dragons based on mode
		// If the game is not running, we don't have any data on current dragon/skin, 
		// so load a placeholder one manually instead
		switch(m_mode) {
			case Mode.CURRENT_DRAGON: {
				if(Application.isPlaying) {
					LoadDragon(UsersManager.currentUser.currentDragon);	
				} else {
					LoadDragon(m_placeholderDragonSku);
				}
			} break;

			case Mode.SELECTED_DRAGON: {
				if(Application.isPlaying) {
					LoadDragon(InstanceManager.menuSceneController.selectedDragon);
				} else {
					LoadDragon(m_placeholderDragonSku);
				}
			} break;

			case Mode.MANUAL: {
				LoadDragon(m_dragonSku, m_disguiseSku);
			} break;
		}
	}

	/// <summary>
	/// Destroy current loaded dragon, if any.
	/// </summary>
	public void UnloadDragon() {
		// Just make sure the object doesn't have anything attached
		m_dragonInstance = null;
		foreach(Transform child in transform) {
			GameObject.DestroyImmediate(child.gameObject);	// Immediate so it can be called from the editor
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void SetViewPosition( Vector3 position ){
		Transform _viewTransform = m_dragonInstance.transform.Find("view");
		Vector3 diff = position - _viewTransform.position;
		m_dragonInstance.transform.position = m_dragonInstance.transform.position + diff;
	}
}
