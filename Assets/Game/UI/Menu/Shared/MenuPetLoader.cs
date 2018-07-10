﻿// MenuPetLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Helper class to easily instantiate pet models within the menu.
/// </summary>
public class MenuPetLoader : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Mode {
		MANUAL				// Manual control via the Load() method and the exposed m_petSku parameter
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Mode m_mode = Mode.MANUAL;
	public Mode mode {
		get { return m_mode; }
		set {
			m_mode = value; 
			Reload();
		}
	}

	[SkuList(DefinitionsCategory.PETS, true)]
	[SerializeField] private string m_petSku = "";
	public string petSku {
		get { return m_petSku; }
		set {
			m_petSku = value;
			Reload();
		}
	}

	[Space]
	[HideEnumValues(false, true)]
	[SerializeField] private MenuPetPreview.Anim m_anim = MenuPetPreview.Anim.IDLE;
	public MenuPetPreview.Anim anim {
		get { return m_anim; }
		set { m_anim = value; }
	}

	[SerializeField] private bool m_showRarityGlow = false;
	public bool showRarityGlow {
		get { return m_showRarityGlow; }
		set { m_showRarityGlow = value; }
	}

	[SerializeField] private bool m_resetScale = true;
	public bool resetScale {
		get { return m_resetScale; }
		set { m_resetScale = value; }
	}

	[SerializeField] private bool m_keepLayers = false;
	public bool keepLayers {
		get { return m_keepLayers; }
		set { m_keepLayers = value; }
	}

	// Internal
	private GameObject m_petInstance = null;
	public GameObject petInstance {
		get { return m_petInstance; }
	}

	private ParticleScaler m_pscaler = null;
	public ParticleScaler pscaler {
		get {
			if(m_pscaler == null) {
				// Try getting a defined scalre
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

	private Camera m_uiCamera;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialiation.
	/// </summary>
	private void Awake() {
		Canvas c = GetComponentInParent<Canvas>();
		if (c)
			m_uiCamera = c.worldCamera;

		// Try to find out already instantiated previews of the pet
		MenuPetPreview preview = this.GetComponentInChildren<MenuPetPreview>();
		if(preview != null) {
			m_petInstance = preview.gameObject;
		}

		// If this object has any children, consider first one the pet preview placeholder
		else if(transform.childCount > 0) {
			m_petInstance = transform.GetChild(0).gameObject;
		}
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize loaded pet (unless using MANUAL mode)
		if(m_mode != Mode.MANUAL) Reload();

		// Subscribe to external events
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Configure the loader with a specific setup.
	/// Doesn't update the current view, if any.
	/// Use either Load() or Reload() for that.
	/// </summary>
	/// <param name="_mode">Pet loading mode.</param>
	/// <param name="_initialAnim">Initial animation.</param>
	/// <param name="_resetScale">Whether to respect pet's prefab original scale or reset it.</param>
	public void Setup(Mode _mode, MenuPetPreview.Anim _initialAnim, bool _resetScale) {
		// Store new setup
		m_mode = _mode;
		m_anim = _initialAnim;
		m_resetScale = _resetScale;
	}

	/// <summary>
	/// Load the pet with the given sku.
	/// </summary>
	/// <param name="_sku">The sku of the pet to be loaded</param>
	public void Load(string _sku) {
		// Unload current pet if any
		Unload();

		// Load selected pet
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _sku);
		if(def != null) {
			// Instantiate the prefab and add it as child of this object
			// [AOC] TODO!! Content not ready yet
			GameObject petPrefab = Resources.Load<GameObject>(Pet.MENU_PREFAB_PATH + def.GetAsString("menuPrefab"));
			if(petPrefab != null) {
				m_petInstance = GameObject.Instantiate<GameObject>(petPrefab);
				m_petInstance.transform.SetParent(this.transform);
				m_petInstance.transform.localPosition = Vector3.zero;
				m_petInstance.transform.localRotation = Quaternion.identity;

				// Keep layers?
				if(!m_keepLayers) {
					m_petInstance.SetLayerRecursively(this.gameObject.layer);
				}

				// Initialize preview and launch the default animation
				MenuPetPreview petPreview = m_petInstance.GetComponent<MenuPetPreview>();
				petPreview.sku = _sku;
				petPreview.SetAnim(MenuPetPreview.Anim.IN);

				// Some pets need look at at the ui camera instead of main camera (3D)
				if (m_uiCamera != null){
					LookAtMainCamera[] lookAt = m_petInstance.GetComponentsInChildren<LookAtMainCamera>();
					for (int x = 0; x < lookAt.Length ; x++) {
						lookAt[x].overrideCamera = m_uiCamera;
					}
				}

				// Show rarity glow if required
				// [AOC] Only when playing, otherwise causes a null reference!
				if(Application.isPlaying) {
					petPreview.ToggleRarityGlow(m_showRarityGlow);
				}

				// Reset scale if required
				if(m_resetScale) {
					m_petInstance.transform.localScale = Vector3.one;
				}

				// Make sure particles are properly scaled as well
				pscaler.ReloadOriginalData();
				pscaler.DoScale();
			}
		}

		// Update pet sku
		m_petSku = _sku;
	}

	/// <summary>
	/// Reload pet preview based on mode.
	/// </summary>
	public void Reload() {
		// Load different pets based on mode
		switch(m_mode) {
			case Mode.MANUAL:	Load(m_petSku);	break;
		}
	}

	/// <summary>
	/// Destroy current loaded pet, if any.
	/// </summary>
	public void Unload() {
		// Destroy all childs of the loader and clear references
		foreach(Transform child in transform) {
			GameObject.DestroyImmediate(child.gameObject);	// Immediate so it can be called from the editor
			m_petInstance = null;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}
