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
[RequireComponent(typeof(PopupController))]
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
	[SerializeField] private Transform m_layoutContainer = null;
	[FileListAttribute("Resources/UI/Popups/DragonInfoLayouts", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string[] m_layoutPrefabs = new string[(int)DragonTier.COUNT];
	[Space]
	[SerializeField] private float m_timeBetweenLoaders = 0.5f;	// From FGOL
	[SerializeField] private int m_framesBetweenLoaders = 5;	// From FGOL

	// Other setup
	[Separator]
	[SerializeField] private Color m_highlightTextColor = Color.yellow;
	[SerializeField] private Color m_fireRushTextColor = Colors.orange;

	// Internal
	private GameObject m_layoutInstance = null;
	private UI3DLoader[] m_loaders = null;
	private int m_loaderIdx = 0;
	private bool m_openAnimFinished = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to popup controller events
		PopupController popup = GetComponent<PopupController>();
		popup.OnOpenPostAnimation.AddListener(OnOpenPostAnimation);
		popup.OnClosePreAnimation.AddListener(OnClosePreAnimation);
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
	/// <param name="_dragonData">Data of the dragon whose info we want to display.</param>
	public void Init(DragonData _dragonData) {
		// Ignore if dragon data not valid
		if(_dragonData == null) return;

		// Tier icon
		string tierIcon = _dragonData.tierDef.GetAsString("icon");
		m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, tierIcon);

		// Dragon name
		m_dragonNameText.Localize(_dragonData.def.Get("tidName"));

		// HP
		m_healthText.text = StringUtils.FormatNumber(_dragonData.maxHealth, 0);

		// Boost
		m_energyText.text = StringUtils.FormatNumber(_dragonData.baseEnergy, 0);

		// Tier description
		// %U0 dragons can equip <color=%U1>%U2 pets</color> and give a <color=%U1>%U3</color> 
		// multiplier during <color=%U4>Fire Rush</color>
		m_tierInfoText.Localize("TID_DRAGON_INFO_TIER_DESCRIPTION", 
			UIConstants.GetSpriteTag(tierIcon),
			m_highlightTextColor.ToHexString("#"),
			StringUtils.FormatNumber(_dragonData.pets.Count),	// Dragon data has as many slots as defined for this dragon
			"x" + StringUtils.FormatNumber(_dragonData.def.GetAsFloat("furyScoreMultiplier", 2), 0),
			m_fireRushTextColor.ToHexString("#")
		);

		// Edible/destructible layout corresponding to this dragon's tier
		LoadLayout(_dragonData.tier);
	}

	/// <summary>
	/// Loads the layout linked to the given tier.
	/// </summary>
	/// <param name="_tier">Tier whose layout we want.</param>
	private void LoadLayout(DragonTier _tier) {
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
		m_layoutInstance = GameObject.Instantiate<GameObject>(layoutPrefab, m_layoutContainer, false);
		if(m_layoutInstance == null) return;

		// Apply some extra properties
		m_layoutInstance.SetLayerRecursively(m_layoutContainer.gameObject.layer);

		// Find out all loaders within the newly instantiated layout
		m_loaders = m_layoutInstance.GetComponentsInChildren<UI3DLoader>();

		// Perpare for asynchronous loading!
		// If the popup's not open, wait until the open animiation has finished
		m_loaderIdx = 0;
		if(m_openAnimFinished) {
			StartCoroutine(StartLoader(m_loaderIdx));
		};
	}

	/// <summary>
	/// Coroutine to start loading the loader with the given index,
	/// provided minimum frames and time have been reached.
	/// Nothing will happen if index out of bounds or loaders array not initialized.
	/// </summary>
	/// <param name="_idx">Index of the loader to be started.</param>
	private IEnumerator StartLoader(int _idx) {
		// Do some checks
		if(m_loaders == null) yield break;
		if(_idx < 0 || _idx >= m_loaders.Length) yield break;
		if(m_loaders[_idx] == null) yield break;

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
		// Do any initialization required in the loaded 3D object
		//_loader.loadedInstance;
		// [AOC] Nothing to do for now

		// Remove listener
		_loader.OnLoadingComplete.RemoveListener(OnLoaderCompleted);

		// Start next loader
		m_loaderIdx++;
		StartCoroutine(StartLoader(m_loaderIdx));
	}

	/// <summary>
	/// The open animation has finished.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Update flag
		m_openAnimFinished = true;

		// Start loading! (We don't do it earlier because popup's animation could affect the 3D scalers.
		StartCoroutine(StartLoader(m_loaderIdx));
	}

	/// <summary>
	/// The close animation is about to start.
	/// </summary>
	public void OnClosePreAnimation() {
		m_openAnimFinished = false;
	}
}