// PopupTierPreyInfo.cs
// 
// Created by Alger Ortín Castellví on 09/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to display information of a dragon tier.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTierPreyInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupTierPreyInfo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("All elements are optional")]
	[Separator("UI Elements")]
	[SerializeField] protected Image m_tierIcon = null;

	// Edibles/Destructibles layout
	[Separator("Entities Layout")]
	[SerializeField] protected Transform m_layoutContainer = null;
	public Transform layoutContainer { get { return m_layoutContainer; } }
	[FileListAttribute("Resources/UI/Popups/Menu/DragonInfoLayouts", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] protected string[] m_layoutPrefabs = new string[(int)DragonTier.COUNT];
	[Space]
	[SerializeField] protected float m_timeBetweenLoaders = 0.5f;   // From FGOL
	[SerializeField] protected int m_framesBetweenLoaders = 5;  // From FGOL

	// Internal
	protected DefinitionNode m_targetTierDef = null;
	protected DragonTier m_targetTier = DragonTier.TIER_0;
	protected DragonTier m_loadedTier = DragonTier.COUNT;
	protected GameObject m_layoutInstance = null;

	// Loaders logic
	protected UI3DAddressablesLoader[] m_loaders = null;
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
	}

	//------------------------------------------------------------------------//
	// SCROLLING CONTROL													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given dragon info.
	/// </summary>
	/// <param name="_tier">Tier whose info we want to display.</param>
	public void Init(DragonTier _tier) {
		// Initialize with target tier
		m_targetTier = _tier;
		m_targetTierDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGON_TIERS, IDragonData.TierToSku(m_targetTier));
		Refresh();
	}

	/// <summary>
	/// Refresh the popup with the info from the currently selected dragon (in the scroller).
	/// </summary>
	protected virtual void Refresh() {
		// Tier data (only if different than the last loaded)
		if(m_loadedTier != m_targetTier) {
			// Tier icon
			string tierIcon = m_targetTierDef.GetAsString("icon");
			if(m_tierIcon != null) {
				m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, tierIcon);
			}

			// Edible/destructible layout corresponding to this dragon's tier
			LoadLayout(m_targetTier);

			// Store new tier
			m_loadedTier = m_targetTier;
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
			Destroy(m_layoutInstance);
			m_layoutInstance = null;
			m_loaders = null;
		}

		// Load layout corresponding to the given tier
		GameObject layoutPrefab = Resources.Load<GameObject>(m_layoutPrefabs[(int)_tier]);
		if(layoutPrefab == null) return;

		// Create instance!
		m_layoutInstance = Instantiate<GameObject>(layoutPrefab, m_layoutContainer, false);
		if(m_layoutInstance == null) return;

		// Apply some extra properties
		m_layoutInstance.SetLayerRecursively(m_layoutContainer.gameObject.layer);

		// Find out all loaders within the newly instantiated layout
		m_loaders = m_layoutInstance.GetComponentsInChildren<UI3DAddressablesLoader>();

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


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A 3D loader has been complete.
	/// </summary>
	/// <param name="_loader">The loader that triggered the event.</param>
	public void OnLoaderCompleted(UI3DAddressablesLoader _loader) {
        // Do any initialization required in the loaded 3D object
        // Basically remove all components that depend on in-game stuff
        // Let's go hardcore and actually remove ALL components
        MonoBehaviour[] components = _loader.loadedInstance.GetComponents<MonoBehaviour>();
		for(int i = 0; i < components.Length; i++) {
			Destroy(components[i]);
		}

		// If the prefab has any LookAtMainCamera component (billboards), override it to look at popup's canvas camera
		LookAtMainCamera[] lookAtMainCameraComponents = _loader.loadedInstance.GetComponentsInChildren<LookAtMainCamera>();
		for(int i = 0; i < lookAtMainCameraComponents.Length; i++) {
			lookAtMainCameraComponents[i].overrideCamera = PopupManager.canvas.worldCamera;
		}

		// Disable all colliders, we don't want the physics moving stuff around
		Collider[] colliders = _loader.loadedInstance.GetComponentsInChildren<Collider>();
		for(int i = 0; i < colliders.Length; i++) {
			colliders[i].enabled = false;
		}

		// Make all animators within the prefab work with unscaled time so the popup works properly even with the game paused
		Animator[] animators = _loader.loadedInstance.GetComponentsInChildren<Animator>();
		for(int i = 0; i < animators.Length; i++) {
			animators[i].updateMode = AnimatorUpdateMode.UnscaledTime;
			animators[i].cullingMode = AnimatorCullingMode.AlwaysAnimate;   // Also make sure they are not culled!
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
