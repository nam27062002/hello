// PopupDragonInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to display information of a dragon / dragon tier.
/// </summary>
public class PopupDragonInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/PF_PopupDragonInfo";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Image m_tierIcon = null;
	[SerializeField] private Localizer m_dragonNameText = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_healthText = null;
	[SerializeField] private TextMeshProUGUI m_energyText = null;
	[SerializeField] private Localizer m_tierInfoText = null;

	// Edibles/Destructibles layout
	[Separator]
	[SerializeField] private Transform m_edibleContainer = null;
	[SerializeField] private Transform m_destructibleContainer = null;
	[Comment("One per tier!")]
	[FileListAttribute("Resources/UI/Popups/DragonInfoLayouts", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string[] m_layoutPrefabs = new string[(int)DragonTier.COUNT];
	[Space]
	[SerializeField] private float m_timeBetweenLoaders = 0.5f;	// From FGOL
	[SerializeField] private int m_framesBetweenLoaders = 5;	// From FGOL

	// Internal
	private GameObject m_layoutInstance = null;
	private UI3DLoader[] m_loaders = null;
	private int m_loaderIdx = 0;

	// Internal

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
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
		// If we have a layout loaded, destroy it
		if(m_layoutInstance != null) {
			GameObject.Destroy(m_layoutInstance);
			m_layoutInstance = null;
		}

		// Unsubscribe from external events.
		if(m_loaders != null) {
			for(int i = 0; i < m_loaders.Length; i++) {
				m_loaders[i].OnLoadingComplete.RemoveListener(OnLoaderCompleted);
			}
		}
	}

	/// <summary>
	/// Something has changed on the inspector.
	/// </summary>
	private void OnValidate() {
		// Layouts array has fixed size
		m_layoutPrefabs.Resize((int)DragonTier.COUNT);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given dragon info.
	/// </summary>
	/// <param name="_dragonSku">Sku of the dragon whose info we want to display.</param>
	public void Init(string _dragonSku) {
		//		__/\\\\\\\\\\\\\\\________/\\\\\________/\\\\\\\\\\\\___________/\\\\\___________/\\\_________/\\\____
		//		 _\///////\\\/////_______/\\\///\\\_____\/\\\////////\\\_______/\\\///\\\_______/\\\\\\\_____/\\\\\\\__
		//		  _______\/\\\__________/\\\/__\///\\\___\/\\\______\//\\\____/\\\/__\///\\\____/\\\\\\\\\___/\\\\\\\\\_
		//		   _______\/\\\_________/\\\______\//\\\__\/\\\_______\/\\\___/\\\______\//\\\__\//\\\\\\\___\//\\\\\\\__
		//		    _______\/\\\________\/\\\_______\/\\\__\/\\\_______\/\\\__\/\\\_______\/\\\___\//\\\\\_____\//\\\\\___
		//		     _______\/\\\________\//\\\______/\\\___\/\\\_______\/\\\__\//\\\______/\\\_____\//\\\_______\//\\\____
		//		      _______\/\\\_________\///\\\__/\\\_____\/\\\_______/\\\____\///\\\__/\\\________\///_________\///_____
		//		       _______\/\\\___________\///\\\\\/______\/\\\\\\\\\\\\/_______\///\\\\\/__________/\\\_________/\\\____
		//		        _______\///______________\/////________\////////////___________\/////___________\///_________\///_____

		// Get dragon data!
		DragonData data = DragonManager.GetDragonData(_dragonSku);
		if(data == null) return;

		// Tier icon

		// Dragon name

		// HP

		// Boost

		// Tier description

		// Edible layout corresponding to this dragon's tier
		LoadLayout(data.tier);

		// Destructible layout
	}

	/// <summary>
	/// Loads the layout linked to the given tier.
	/// </summary>
	/// <param name="_tier">Tier whose layout we want.</param>
	public void LoadLayout(DragonTier _tier) {
		// If we already have a layout loaded, destroy it
		if(m_layoutInstance != null) {
			GameObject.Destroy(m_layoutInstance);
			m_layoutInstance = null;
			m_loaders = null;
		}

		// Load layout corresponding to the given tier
		GameObject layoutPrefab = Resources.Load<GameObject>(m_layoutPrefabs[(int)_tier]);
		if(layoutPrefab == null) return;

		// Create instance!
		m_layoutInstance = GameObject.Instantiate<GameObject>(layoutPrefab, this.transform, false);
		if(m_layoutInstance == null) return;

		// Apply some extra properties
		m_layoutInstance.SetLayerRecursively(this.gameObject.layer);

		// Find out all loaders within the newly instantiated layout
		m_loaders = m_layoutInstance.GetComponentsInChildren<UI3DLoader>();

		// Start loading asynchronously!
		m_loaderIdx = 0;
		StartCoroutine(StartLoader(m_loaderIdx));
	}

	/// <summary>
	/// Coroutine to start loading the loader with the given index,
	/// provided minimum frames and time have been reached.
	/// Nothing will happen if index out of bounds or loaders array not initialized.
	/// </summary>
	/// <param name="_idx">Index of the loader to be started.</param>
	private IEnumerator StartLoader(int _idx) {
		// Do some checks
		if(m_loaders == null) yield return null;
		if(_idx < 0 || _idx >= m_loaders.Length) yield return null;
		if(m_loaders[_idx] == null) yield return null;

		// Wait a little bit before actually loading
		yield return new WaitForSeconds(m_timeBetweenLoaders);
		for(int i = 0; i < m_framesBetweenLoaders; ++i) {
			yield return new WaitForEndOfFrame();
		}

		// Start loading!
		m_loaders[_idx].OnLoadingComplete.AddListener(OnLoaderCompleted);
		m_loaders[_idx].Load();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A 3D loader has been complete.
	/// </summary>
	/// <param name="_loader">The loader that triggered the event.</param>
	public void OnLoaderCompleted(UI3DLoader _loader) {
		// Get loaded instance and clean it up so it works properly on the menus


		// Remove listener
		_loader.OnLoadingComplete.RemoveListener(OnLoaderCompleted);

		// Start next loader
		m_loaderIdx++;
		StartCoroutine(StartLoader(m_loaderIdx));
	}
}