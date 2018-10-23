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
using DG.Tweening;

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
	public const string PATH = "UI/Popups/Menu/PF_PopupDragonInfo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("All elements are optional")]
	[Separator("UI Elements")]

	[SerializeField] protected Localizer m_dragonNameText = null;
	[SerializeField] protected Localizer m_dragonDescText = null;
	[SerializeField] protected Image m_dragonIcon = null;
	[Space]
	[SerializeField] protected TextMeshProUGUI m_healthText = null;
	[SerializeField] protected TextMeshProUGUI m_energyText = null;
	[SerializeField] protected TextMeshProUGUI m_speedText = null;
	[Space]
	[SerializeField] protected Image m_tierIcon = null;
	[SerializeField] protected Localizer m_tierInfoText = null;

	// Edibles/Destructibles layout
	[Separator("Entities Layout")]
	[SerializeField] protected Transform m_layoutContainer = null;
	public Transform layoutContainer { get { return m_layoutContainer; }}
	[FileListAttribute("Resources/UI/Popups/Menu/DragonInfoLayouts", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] protected string[] m_layoutPrefabs = new string[(int)DragonTier.COUNT];
	[Space]
	[SerializeField] protected float m_timeBetweenLoaders = 0.5f;	// From FGOL
	[SerializeField] protected int m_framesBetweenLoaders = 5;	// From FGOL
	[Space]
	[SerializeField] protected Shader m_entitiesPreviewShader = null;
	[SerializeField] [Range(0f, 5f)] protected float m_fresnelFactor = 3f;
	[SerializeField] protected Color m_fresnelColor = Color.gray;

	// Internal
	protected IDragonData m_dragonData = null;
	protected DragonTier m_loadedTier = DragonTier.COUNT;
	protected GameObject m_layoutInstance = null;

	// Loaders logic
	protected UI3DLoader[] m_loaders = null;
	protected int m_loaderIdx = 0;
	protected IEnumerator m_loaderDelayCoroutine = null;
	protected bool m_openAnimFinished = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Subscribe to popup controller events
		PopupController popup = GetComponent<PopupController>();
		popup.OnOpenPostAnimation.AddListener(OnOpenPostAnimation);
		popup.OnClosePreAnimation.AddListener(OnClosePreAnimation);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDestroy() {
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
	protected void OnValidate() {
		// Layouts array has fixed size
		m_layoutPrefabs.Resize((int)DragonTier.COUNT);

		// Only while playing
		if(Application.isPlaying) {
			// Update fresnel values for all loaded entities
			if(m_loaders != null) {
				for(int i = 0; i < m_loaders.Length; i++) {
					UpdateShaders(m_loaders[i].loadedInstance);	// Nothing will happen if null
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// SCROLLING CONTROL													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given dragon info.
	/// </summary>
	/// <param name="_dragonData">Data of the dragon whose info we want to display.</param>
	public void Init(IDragonData _dragonData) {
		// Initialize with currently selected dragon
		m_dragonData = _dragonData;
		Refresh();
	}

	/// <summary>
	/// Refresh the popup with the info from the currently selected dragon (in the scroller).
	/// </summary>
	protected virtual void Refresh() {
		// Only if current data is valid
		if(m_dragonData == null) return;

		// Dragon name
		if(m_dragonNameText != null) {
			m_dragonNameText.Localize(m_dragonData.def.Get("tidName"));
		}

		// Dragon description
		if(m_dragonDescText != null) {
			m_dragonDescText.Localize(m_dragonData.def.Get("tidDesc"));
		}

		// Dragon icon
		if(m_dragonIcon != null) {
			m_dragonIcon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + m_dragonData.def.sku + "/icon_disguise_0");
		}

		// HP
		if(m_healthText != null) {
			m_healthText.text = StringUtils.FormatNumber(m_dragonData.maxHealth, 0);
		}

		// Boost
		if(m_energyText != null) {
			m_energyText.text = StringUtils.FormatNumber(m_dragonData.baseEnergy, 0);
		}

		// Speed
		if(m_speedText != null) {
			m_speedText.text = StringUtils.FormatNumber(m_dragonData.maxSpeed * 10f, 0);    // x10 to show nicer numbers
		}

		// Tier data (only if different than the last loaded)
		if(m_loadedTier != m_dragonData.tier) {
			// Tier icon
			string tierIcon = m_dragonData.tierDef.GetAsString("icon");
			if(m_tierIcon != null) {
				m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, tierIcon);
			}

			// Tier description
			if(m_tierInfoText != null) {
				// %U0 dragons can equip <color=%U1>%U2 pets</color> and give a <color=%U1>%U3</color> 
				// multiplier during <color=%U4>Fire Rush</color>
				int numPets = m_dragonData.pets.Count;  // Dragon data has as many slots as defined for this dragon
				m_tierInfoText.Localize("TID_DRAGON_INFO_TIER_DESCRIPTION",
					UIConstants.GetSpriteTag(tierIcon),
					(numPets > 1 ? LocalizationManager.SharedInstance.Localize("TID_PET_PLURAL") : LocalizationManager.SharedInstance.Localize("TID_PET")), // Singular/Plural
					StringUtils.FormatNumber(numPets),
					"x" + StringUtils.FormatNumber(m_dragonData.def.GetAsFloat("furyScoreMultiplier", 2), 0)
				);
			}

			// Edible/destructible layout corresponding to this dragon's tier
			LoadLayout(m_dragonData.tier);

			// Store new tier
			m_loadedTier = m_dragonData.tier;
		}
	}

	//------------------------------------------------------------------------//
	// LOADERS CONTROL														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Loads the layout linked to the given tier.
	/// </summary>
	/// <param name="_tier">Tier whose layout we want.</param>
	protected void LoadLayout(DragonTier _tier) {
		// Ignore if no container is defined
		if(m_layoutContainer == null) return;

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

		// Prepare for asynchronous loading!
		// If a delayed loading is already running, stop it
		if(m_loaderDelayCoroutine != null) {
			StopCoroutine(m_loaderDelayCoroutine);
			m_loaderDelayCoroutine = null;
		}

		// Start loading!
		m_loaderIdx = 0;
		m_loaderDelayCoroutine = StartLoader(m_loaderIdx);
		StartCoroutine(m_loaderDelayCoroutine);

		// Check loaders
		ToggleLoaders();
	}

	/// <summary>
	/// Coroutine to start loading the loader with the given index,
	/// provided minimum frames and time have been reached.
	/// Nothing will happen if index out of bounds or loaders array not initialized.
	/// </summary>
	/// <param name="_idx">Index of the loader to be started.</param>
	protected IEnumerator StartLoader(int _idx) {
		// Do some checks
		if(m_loaders == null) yield break;
		if(_idx < 0 || _idx >= m_loaders.Length) yield break;
		if(m_loaders[_idx] == null) yield break;

		// Wait a little bit before actually loading
		yield return new WaitForSecondsRealtime(m_timeBetweenLoaders);
		for(int i = 0; i < m_framesBetweenLoaders; ++i) {
			yield return new WaitForEndOfFrame();
		}

		// Start loading!
		m_loaders[_idx].OnLoadingComplete.AddListener(OnLoaderCompleted);
		m_loaders[_idx].LoadAsync();
	}

	/// <summary>
	/// Automatically enable/disable loaders depending on the popup's animation.
	/// Prevents instantiations while scale is not 1.
	/// </summary>
	protected void ToggleLoaders() {
		bool toggle = (m_openAnimFinished);

		if(m_loaders != null) {
			for(int i = 0; i < m_loaders.Length; i++) {
				// If the behaviour is not updated, it will never be instantiated
				m_loaders[i].enabled = toggle;
			}
		}
	}

	/// <summary>
	/// Apply the entities shaders modifications to the given game object.
	/// </summary>
	/// <param name="_go">Target game object.</param>
	protected void UpdateShaders(GameObject _go) {
		// Ignore if object not valid
		if(_go == null) return;

		// Find all renderers in the target game object
		Material m = null;
		string fresnelFactorID;
		string fresnelColorID;
		Renderer[] renderers = _go.GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++) {
			for(int j = 0; j < renderers[i].materials.Length; j++) {
				// Shorter notation for clearer code
				m = renderers[i].materials[j];

				// Default IDs
				fresnelFactorID = "_FresnelPower";
				fresnelColorID = "_FresnelColor";

				// If the material doesn't have fresnel properties, replace by default material
				if(!m.HasProperty(fresnelFactorID) || !m.HasProperty(fresnelColorID)) {
					// Except dragon materials, which have their own special names
					if(m.shader.name.Contains("/Dragon/")) {	// [AOC] Hacky as hell!
						fresnelFactorID = "_Fresnel";
					} 

					// And transparent material, used for glows and some other VFX
					else if(m.shader.name.Contains("Transparent")) {
						// Nothing to do
					}

					// Standard material, replace it
					else {
						m.shader = m_entitiesPreviewShader;
					}
				}

				// Everything ok! Apply fresnel
				m.SetFloat(fresnelFactorID, m_fresnelFactor);
				m.SetColor(fresnelColorID, m_fresnelColor);
			}
		}
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
		// Basically remove all components that depend on in-game stuff
		// Let's go hardcore and actually remove ALL components
		MonoBehaviour[] components = _loader.loadedInstance.GetComponents<MonoBehaviour>();
		for(int i = 0; i < components.Length; i++) {
			GameObject.Destroy(components[i]);
		}

		// Update materials shaders so the prefab is properly rendered in the UI
		UpdateShaders(_loader.loadedInstance);

		// If the prefab has any LookAtMainCamera component (billboards), override it to look at popup's canvas camera
		LookAtMainCamera[] lookAtMainCameraComponents = _loader.loadedInstance.GetComponentsInChildren<LookAtMainCamera>();
		for(int i = 0; i < lookAtMainCameraComponents.Length; i++) {
			lookAtMainCameraComponents[i].overrideCamera = PopupManager.canvas.worldCamera;
		}

		// Make all animators within the prefab work with unscaled time so the popup works properly even with the game paused
		Animator[] animators = _loader.loadedInstance.GetComponentsInChildren<Animator>();
		for(int i = 0; i < animators.Length; i++) {
			animators[i].updateMode = AnimatorUpdateMode.UnscaledTime;
			animators[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;	// Also make sure they are not culled!
		}

		// Remove listener
		_loader.OnLoadingComplete.RemoveListener(OnLoaderCompleted);

		// Start next loader
		m_loaderIdx++;
		m_loaderDelayCoroutine = StartLoader(m_loaderIdx);
		StartCoroutine(m_loaderDelayCoroutine);
	}

	/// <summary>
	/// The open animation has finished.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Update flag
		m_openAnimFinished = true;

		// Check loaders
		ToggleLoaders();
	}

	/// <summary>
	/// The close animation is about to start.
	/// </summary>
	public void OnClosePreAnimation() {
		m_openAnimFinished = false;
	}
}