// UINotification.cs
// 
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(ScrollRectSnapPoint))]
public class DisguisePill : MonoBehaviour, IPointerClickHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Parametrized event
	public class DisguisePillEvent : UnityEvent<DisguisePill>{}

	// Resources constants
	private const string NOTIFICATION_PREFAB_PATH = "UI/Common/PF_UINotificationFlag";


    [System.Serializable]
	private class SkinShadowEffect {
		public float brightness = -0.8f;
		public float saturation = -0.7f;
		public float contrast   = -0.6f;  
	}

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    [SerializeField] private Image m_icon = null;
    public Image icon { get { return m_icon; } }
    [SerializeField] private Localizer m_infoText;
    [SerializeField] private GameObject m_lockIcon;
	[Space]
    [SerializeField] private ShowHideAnimator m_equippedFrame;
    [SerializeField] private DOTweenAnimation m_equippedFX;
    [Space]
	[SerializeField] private Image m_seasonalIcon = null;
	[SerializeField] private GameObject m_seasonalIconRoot = null;
    [SerializeField] private Transform m_seasonalParticlesOrigin = null;
    [Space]
    [SerializeField] private Transform m_notificationAnchor = null;
	[Space]
	[SerializeField] private Color m_equippedTextColor = Color.white;
	[SerializeField] private Color m_getNowTextColor = Colors.gray;
	[SerializeField] private Color m_lockedTextColor = Color.red;
	[Space]
	[SerializeField] private SkinShadowEffect m_shadowEffect;
	[SerializeField] private UIColorFX m_colorFX;


    // Events
    public DisguisePillEvent OnPillClicked = new DisguisePillEvent();

	// Data
	private DefinitionNode m_def;
	public DefinitionNode def {
		get { return m_def; }
	}

	private IDragonData m_dragonData = null;
	public IDragonData dragonData {
		get { return m_dragonData; }
	}

	private DefinitionNode m_seasonDef = null;
	public DefinitionNode seasonDef {
		get { return m_seasonDef; }
	}

	// State
	private Wardrobe.SkinState m_state = Wardrobe.SkinState.LOCKED;
	public Wardrobe.SkinState state {
		get { return m_state; }
	}

	public bool owned {
		get { return m_state == Wardrobe.SkinState.OWNED; }
	}

	public bool locked {
		get { return m_state == Wardrobe.SkinState.LOCKED; }
	}

	private bool m_equipped = false;
	public bool equipped {
		get { return m_equipped; }
	}

	// References
	private ScrollRectSnapPoint m_snapPoint = null;
	public ScrollRectSnapPoint snapPoint {
		get {
			if(m_snapPoint == null) {
				m_snapPoint = GetComponent<ScrollRectSnapPoint>();
			}
			return m_snapPoint;
		}
	}

    
    // Internal
    private static bool m_useAsycLoading = true;
    private AddressablesOp m_previewRequest = null;
    private UINotification m_newNotification = null;
    private GameObject m_seasonParticlesInstance = null;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    public void Update()
    {
        // Update the icon sprite when the async load is completed
        if (m_useAsycLoading)
        {
            if (m_previewRequest != null)
            {
                if (m_previewRequest.isDone)
                {
                    m_icon.sprite = m_previewRequest.GetAsset<Sprite>();
                    m_icon.enabled = true;
                    m_previewRequest = null;
                }
            }
        }
    }


    //------------------------------------------------------------------------//
    // METHODS														          //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the pill with the given skin definition, state and preview image.
    /// </summary>
    /// <param name="_def">Skin definition to be used to initialize the pill.</param>
    /// <param name="_state">State of the skin.</param>
    public void Load(DefinitionNode _def, Wardrobe.SkinState _state) {
		// Store data
		m_def = _def;
		m_dragonData = DragonManager.GetDragonData(_def.GetAsString("dragonSku"));
		m_state = _state;
		m_seasonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SEASONS, m_def.GetAsString("unlockSeason", SeasonManager.NO_SEASON_SKU));

		// Equipped status - start unequipped
		m_equipped = false;
		m_equippedFrame.ForceHide(false);
		m_equippedFX.gameObject.SetActive(false);

        // Load icon sprite for this skin
        if (m_icon != null)
        {
            if (m_useAsycLoading)
            {
                m_icon.sprite = null;
                m_icon.enabled = false;
                m_previewRequest = HDAddressablesManager.Instance.LoadAssetAsync(m_def.Get("icon"));
            }
            else
            {
                m_icon.sprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(m_def.Get("icon"));
            }
        }

		// Set initial state
		SetState(_state);
	}

	/// <summary>
	/// Update the pill with a given new skin state.
	/// </summary>
	/// <param name="_state">New state of the skin.</param>
	public void SetState(Wardrobe.SkinState _state) {
		// Store new state
		m_state = _state;

		// Locked?
		if (locked) {
			m_colorFX.brightness = m_shadowEffect.brightness;
			m_colorFX.saturation = m_shadowEffect.saturation;
			m_colorFX.contrast   = m_shadowEffect.contrast;
			m_lockIcon.SetActive(m_seasonDef == null);  // Except for seasonal skins!
		} else {
			m_colorFX.brightness = 0f;
			m_colorFX.saturation = 0f; 
			m_colorFX.contrast   = 0f;
			m_lockIcon.SetActive(false);
		}

		// "New" notification
		bool isNew = (_state == Wardrobe.SkinState.NEW);
		if(m_newNotification != null) {
			m_newNotification.Set(isNew);
		} else if(isNew) {
			// Need to instantiate a notification?
			GameObject prefab = Resources.Load<GameObject>(NOTIFICATION_PREFAB_PATH);
			m_newNotification = Instantiate<GameObject>(prefab, m_notificationAnchor, false).GetComponent<UINotification>();
			m_newNotification.Show();
		}

		// Season icon
		if(m_seasonDef != null ) {

			m_seasonalIconRoot.SetActive(true);
			m_seasonalIcon.sprite = Resources.Load<Sprite>(UIConstants.SEASON_ICONS_PATH + m_seasonDef.Get("icon"));


            // Seasonal particles FX. Show only if the season is active
            if (m_seasonDef.GetAsString("sku") == SeasonManager.activeSeason)
            {
                GameObject particlesPrefab = Resources.Load<GameObject>(UIConstants.SEASONAL_PARTICLES_PATH + m_seasonDef.Get("pillParticles"));
                if (particlesPrefab != null && m_seasonParticlesInstance == null)
                {
                    // Instantiate the particles prefab in the pill
                    m_seasonParticlesInstance = Instantiate(particlesPrefab, m_seasonalParticlesOrigin);
                    m_seasonParticlesInstance.SetActive(true);

                }
            } else
            {
                // If this is not the active season, remove the seasonal FX
                // remember that we are reusing the pills in the horizontal layout
                Destroy(m_seasonParticlesInstance);
                m_seasonParticlesInstance = null;
            }

        } else {
			m_seasonalIconRoot.SetActive(false);
		}



		// Texts
		RefreshText();
	}

	/// <summary>
	/// Show visual feedback when equipping/unequipping this skin.
	/// </summary>
	/// <param name="_equip">Equip?</param>
	/// <param name="_animate">Show animations?</param>
	public void Equip(bool _equip, bool _animate = true) {
		// Can't be equipped if not owned!
		if(_equip && m_state == Wardrobe.SkinState.OWNED) {
			m_equippedFrame.Show(_animate);
			m_equippedFX.gameObject.SetActive(true);
			m_equippedFX.DORestart();
			m_equipped = true;
		} else {
			m_equippedFrame.Hide(_animate);
			m_equippedFX.gameObject.SetActive(false);
			m_equipped = false;
		}

		RefreshText();
	}

	/// <summary>
	/// Properly set pill's text and color based on its state.
	/// </summary>
	private void RefreshText() {
		// Check by priority
		if(m_state == Wardrobe.SkinState.LOCKED) {
			// Locked, can't be neither owned nor equipped
			// Locked by season or by level?
			if(m_seasonDef != null) {
				m_infoText.Localize(string.Empty);
			} else {
				m_infoText.Localize("TID_LEVEL", (m_def.GetAsInt("unlockLevel") + 1).ToString());
				m_infoText.text.color = m_lockedTextColor;
			}
		} else if(m_state == Wardrobe.SkinState.OWNED) {
			// Can't be equipped if it's not owned!
			if(m_equipped) {
				// Equipped
				m_infoText.Localize("TID_DISGUISES_EQUIPPED");
				m_infoText.text.color = m_equippedTextColor;
			} else {
				// Owned but not equipped
				m_infoText.Localize("");
				m_infoText.text.color = Color.white;
			}
		} else {
			// Not owned
			m_infoText.Localize("TID_DISGUISES_GET_NOW");
			m_infoText.text.color = m_getNowTextColor;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// This object has been clicked.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	public void OnPointerClick(PointerEventData _eventData) {
		OnPillClicked.Invoke(this);
	}
}
