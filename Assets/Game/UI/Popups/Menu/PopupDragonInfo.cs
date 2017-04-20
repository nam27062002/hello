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
	public const string PATH = "UI/Popups/PF_PopupDragonInfo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("UI Elements")]
	[SerializeField] private Image m_dragonIcon = null;
	[SerializeField] private Localizer m_dragonNameText = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_healthText = null;
	[SerializeField] private TextMeshProUGUI m_energyText = null;
	[Space]
	[SerializeField] private Image m_tierIcon = null;
	[SerializeField] private Localizer m_tierInfoText = null;

	// Edibles/Destructibles layout
	[Separator("Entities Layout")]
	[SerializeField] private Transform m_layoutContainer = null;
	public Transform layoutContainer { get { return m_layoutContainer; }}
	[FileListAttribute("Resources/UI/Popups/DragonInfoLayouts", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string[] m_layoutPrefabs = new string[(int)DragonTier.COUNT];
	[Space]
	[SerializeField] private float m_timeBetweenLoaders = 0.5f;	// From FGOL
	[SerializeField] private int m_framesBetweenLoaders = 5;	// From FGOL
	[Space]
	[SerializeField] private Shader m_entitiesPreviewShader = null;
	[SerializeField] [Range(0f, 5f)] private float m_fresnelFactor = 3f;
	[SerializeField] private Color m_fresnelColor = Color.gray;

	// Scrolling
	[Separator("Scrolling")]
	[SerializeField] private GameObject m_panel = null;
	[SerializeField] private GameObject m_arrowPrevious = null;
	[SerializeField] private GameObject m_arrowNext = null;
	[SerializeField] private PopupDragonInfoScroller m_scroller = null;
	[SerializeField] private float m_scrollAnimOffset = 1000f;
	[SerializeField] private float m_scrollAnimDuration = 0.25f;

	// Other setup
	[Separator("Other Setup")]
	[SerializeField] private Color m_highlightTextColor = Color.yellow;
	[SerializeField] private Color m_fireRushTextColor = Colors.orange;

	// Internal
	private GameObject m_layoutInstance = null;
	private UI3DLoader[] m_loaders = null;
	private int m_loaderIdx = 0;
	private IEnumerator m_loaderDelayCoroutine = null;

	// Scroll anim
	private bool m_openAnimFinished = false;
	private DragonTier m_loadedTier = DragonTier.COUNT;
	private Sequence m_scrollSequence = null;

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
		m_scroller.OnSelectionIndexChanged.AddListener(OnDragonChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		m_scroller.OnSelectionIndexChanged.RemoveListener(OnDragonChanged);
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

		// Only while playing
		if(Application.isPlaying) {
			// Update fresnel values for all loaded entities
			if(m_loaders != null) {
				for(int i = 0; i < m_loaders.Length; i++) {
					UpdateShaders(m_loaders[i].loadedInstance);	// Nothing will happen if null
				}
			}

			// Re-create scroll animation sequence
			RecreateScrollSequence();
		}
	}

	//------------------------------------------------------------------------//
	// SCROLLING CONTROL													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given dragon info.
	/// </summary>
	/// <param name="_dragonData">Data of the dragon whose info we want to display.</param>
	public void Init(DragonData _dragonData) {
		// Ignore if dragon data not valid
		if(_dragonData == null) return;

		// If the scroller is not initialized, do it now
		if(m_scroller.items.Count == 0) {
			// Disable selection change events
			m_scroller.enableEvents = false;

			// Init list of dragons to scroll around
			m_scroller.Init(DragonManager.dragonsByOrder);

			// Select target def
			m_scroller.SelectItem(_dragonData);

			// Restore selection change events
			m_scroller.enableEvents = true;
		}

		// Initialize with currently selected dragon
		Refresh();
	}

	/// <summary>
	/// Refresh the popup with the info from the currently selected dragon (in the scroller).
	/// </summary>
	private void Refresh() {
		// Only if current data is valid
		DragonData dragonData = m_scroller.selectedItem;
		if(dragonData == null) return;

		// Dragon name and icon
		m_dragonNameText.Localize(dragonData.def.Get("tidName"));
		m_dragonIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.DISGUISE_ICONS_PATH + dragonData.def.sku, "icon_disguise_0");	// [AOC] HARDCODED!!

		// HP
		m_healthText.text = StringUtils.FormatNumber(dragonData.maxHealth, 0);

		// Boost
		m_energyText.text = StringUtils.FormatNumber(dragonData.baseEnergy, 0);

		// Tier data (only if different than the last loaded)
		if(m_loadedTier != dragonData.tier) {
			// Tier icon
			string tierIcon = dragonData.tierDef.GetAsString("icon");
			m_tierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, tierIcon);

			// Tier description
			// %U0 dragons can equip <color=%U1>%U2 pets</color> and give a <color=%U1>%U3</color> 
			// multiplier during <color=%U4>Fire Rush</color>
			m_tierInfoText.Localize("TID_DRAGON_INFO_TIER_DESCRIPTION", 
				UIConstants.GetSpriteTag(tierIcon),
				m_highlightTextColor.ToHexString("#"),
				StringUtils.FormatNumber(dragonData.pets.Count),	// Dragon data has as many slots as defined for this dragon
				"x" + StringUtils.FormatNumber(dragonData.def.GetAsFloat("furyScoreMultiplier", 2), 0),
				m_fireRushTextColor.ToHexString("#")
			);

			// Edible/destructible layout corresponding to this dragon's tier
			LoadLayout(dragonData.tier);

			// Store new tier
			m_loadedTier = dragonData.tier;
		}

		// Arrows visibility
		m_arrowNext.SetActive(m_scroller.selectedIdx < m_scroller.items.Count - 1);	// Hide for last item in the list
		m_arrowPrevious.SetActive(m_scroller.selectedIdx > 0);	// Hide for first item in the list
	}

	/// <summary>
	/// Launch the scroll animation in the target direction.
	/// The popup's info will be refreshed with currently selected item on the "invisible" frame.
	/// </summary>
	/// <param name="_backwards">Left or right?</param>
	private void LaunchScrollAnim(bool _backwards) {
		// If not already programmed, do it now
		if(m_scrollSequence == null) {
			RecreateScrollSequence();
		}

		// Launch the animation in the proper direction
		if(_backwards) {
			m_scrollSequence.Goto(m_scrollSequence.Duration());
			m_scrollSequence.PlayBackwards();
		} else {
			m_scrollSequence.Goto(0f);
			m_scrollSequence.PlayForward();
		}

		// Stop any loaders (we don't want any instantiation while scale is not 1)
		ToggleLoaders();
	}

	/// <summary>
	/// Creates scroll sequence.
	/// Kills any existing sequence.
	/// </summary>
	private void RecreateScrollSequence() {
		// If a sequence already exists, kill it
		if(m_scrollSequence != null) {
			m_scrollSequence.Kill(true);
			m_scrollSequence = null;
		}

		// Do it!
		m_scrollSequence = DOTween.Sequence()
			// Out
			.Append(m_panel.transform.DOLocalMoveX(-m_scrollAnimOffset, m_scrollAnimDuration).SetEase(Ease.InCubic))
			.Join(m_panel.transform.DOScale(0f, m_scrollAnimDuration).SetEase(Ease.InExpo))

			// Refresh once hidden
			.AppendCallback(Refresh)

			// In
			.Append(m_panel.transform.DOLocalMoveX(m_scrollAnimOffset, 0.01f))	// [AOC] Super-dirty: super-fast teleport to new position, no other way than via tween
			.Append(m_panel.transform.DOLocalMoveX(0f, m_scrollAnimDuration).SetEase(Ease.OutCubic))
			.Join(m_panel.transform.DOScale(1f, m_scrollAnimDuration).SetEase(Ease.OutExpo))

			// Start paused
			.SetAutoKill(false)
			.Pause()
			.OnStepComplete(() => {
				ToggleLoaders();
			});
	}

	//------------------------------------------------------------------------//
	// LOADERS CONTROL														  //
	//------------------------------------------------------------------------//
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
		m_loaders[_idx].LoadAsync();
	}

	/// <summary>
	/// Automatically enable/disable loaders depending on the popup's animation.
	/// Prevents instantiations while scale is not 1.
	/// </summary>
	private void ToggleLoaders() {
		bool sequencePlaying = (m_scrollSequence != null && m_scrollSequence.IsPlaying());
		bool toggle = (m_openAnimFinished && !sequencePlaying);

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
	private void UpdateShaders(GameObject _go) {
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

	/// <summary>
	/// New dragon selected!
	/// </summary>
	/// <param name="_oldIdx">Previous selected dragon index.</param>
	/// <param name="_newIdx">New selected dragon index.</param>
	/// <param name="_looped">Have we looped to do the new selection?
	public void OnDragonChanged(int _oldIdx, int _newIdx, bool _looped) {
		// Ignore if animating
		//if(m_scrollSequence != null && m_scrollSequence.IsPlaying()) return;

		// Figure out animation direction and launch it!
		bool backwards = _oldIdx > _newIdx;
		if(_looped) backwards = !backwards;	// Reverse animation direction if a loop was completed
		LaunchScrollAnim(backwards);
	}

	/// <summary>
	/// Scroll to next dragon!
	/// </summary>
	public void OnNextDragon() {
		// UISelector will do it for us
		m_scroller.SelectNextItem();
	}

	/// <summary>
	/// Scroll to previous dragon!
	/// </summary>
	public void OnPreviousDragon() {
		// UISelector will do it for us
		m_scroller.SelectPreviousItem();
	}
}