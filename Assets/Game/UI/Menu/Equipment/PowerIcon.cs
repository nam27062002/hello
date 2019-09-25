// DisguisePowerIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a disguise power icon.
/// </summary>
public class PowerIcon : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	public enum Mode {
		SKIN = 0,
		PET,
		MODFIER,
        SPECIAL_DRAGON
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Mode m_mode = Mode.SKIN;
	[Tooltip("Optional")] [SerializeField] private Image m_powerIcon = null;
	[Tooltip("Optional")] [SerializeField] private GameObject m_lockIcon = null;
	[Tooltip("Optional")] [SerializeField] private Localizer m_nameText = null;
	[Tooltip("Optional")] [SerializeField] private TextMeshProUGUI m_shortDescriptionText = null;

	[Space]
	[Comment("Optional, define an animator to be triggered when there is a power and another one for when there is no power to show")]
	[Tooltip("Optional")] [SerializeField] private ShowHideAnimator m_emptyAnim = null;
	[Tooltip("Optional")] [SerializeField] private ShowHideAnimator m_equippedAnim = null;

	// Exposed Setup
	[Space]
	[SerializeField][Range(0, 1)] private float m_tooltipArrowOffset = 0.5f;

    [Separator("Levels")]
    [Tooltip("Optional")] [SerializeField] private int m_level = 0;
    [Tooltip("Optional")] [SerializeField] private List<Image> m_arrows = null;
    [Tooltip("Optional")] [SerializeField] private List<Color> m_arrowColors = null;


    // Data
    private DefinitionNode m_powerDef = null;
	public DefinitionNode powerDef {
		get { return m_powerDef; }
	}

	// Internal references (shortcuts)
	private ShowHideAnimator m_anim = null;
	public ShowHideAnimator anim {
		get {
			if(m_anim == null) m_anim = GetComponent<ShowHideAnimator>();
			return m_anim;
		}
	}

	private UITooltipTrigger m_trigger = null;
	private UITooltipTrigger trigger {
		get {
			if(m_trigger == null) m_trigger = GetComponentInChildren<UITooltipTrigger>();
			return m_trigger;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	/// <param name="_locked">Whether the power is locked or not.</param>
	/// <parma name="_animate">Whether to show animations or not.</param>
	/// <parma name="_mode">It can be a Skin or Pet power</param>
	public void InitFromDefinition(DefinitionNode _powerDef, bool _locked, bool _animate, Mode _mode) {
		m_mode = _mode;
		InitFromDefinition(_powerDef, _locked, _animate);
	}

	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	/// <param name="_locked">Whether the power is locked or not.</param>
	/// <parma name="_animate">Optional, whether to show animations or not.</param>
	public void InitFromDefinition(DefinitionNode _powerDef, bool _locked, bool _animate = true) {
		// Save definition
		m_powerDef = _powerDef;
		bool show = (_powerDef != null);

		// If defined, trigger empty/equipped animators
		if(m_equippedAnim != null && m_emptyAnim != null) {
			m_equippedAnim.Set(show, _animate);
			m_emptyAnim.Set(!show, _animate);
		}

		// Otherwise, hide if given definition is not valid
		else {
			if(anim != null) {
				anim.Set(show, _animate);
			} else {
				this.gameObject.SetActive(show);
			}
		}

		// If showing, initialize all visible items
		if(show) {
			// Power icon
			if(m_powerIcon != null) {
				// Load from resources
				m_powerIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _powerDef.GetAsString("icon"));
			}

            // Level arrows
            RefreshArrows();

			// Texts
			RefreshTexts();

			// Lock
			SetLocked(_locked);
		}
	}

	/// <summary>
	/// Sets the lock state of the power.
	/// </summary>
	/// <param name="_locked">Whether the power is locked or not.</param>
	public void SetLocked(bool _locked) {
		// Lock icon
		if(m_lockIcon != null) m_lockIcon.SetActive(_locked);

		// Image color
		if(m_powerIcon != null) m_powerIcon.color = _locked ? Color.gray : Color.white;
	}

	/// <summary>
	/// Initialize the short description textfield.
	/// </summary>
	private void RefreshTexts() {
		// Power name
		if(m_nameText != null) {
			if(m_powerDef != null) {
				m_nameText.Localize(m_powerDef.Get("tidName"));
			} else {
				m_nameText.Localize(string.Empty);
			}
		}

		// Short description
		if(m_shortDescriptionText != null) {
			if(m_powerDef == null) {
				m_shortDescriptionText.text = string.Empty;
			} else if(m_mode == Mode.SPECIAL_DRAGON) {
				m_shortDescriptionText.text = m_powerDef.GetLocalized("tidDescShort");
			} else {
				m_shortDescriptionText.text = DragonPowerUp.GetDescription(m_powerDef, true, m_mode == Mode.PET);    // Custom formatting depending on powerup type, already localized
			}
		}
	}

    /// <summary>
	/// Initialize the small arrow icons that indicate the level of the power up
	/// </summary>
	private void RefreshArrows()
    {
        m_level = m_powerDef.GetAsInt("level");

        if (m_arrows != null)
        {
            for (int i = 0; i < m_arrows.Count; i++)
            {
                // Show an amount of arrows according to the powerup level
                m_arrows[i].gameObject.SetActive(i < m_level);

                if (m_arrowColors != null && m_arrowColors[m_level - 1] != null)
                {
                    if (m_level > 0)
                    {
                        // Color the arrows according to the level
                        m_arrows[i].color = m_arrowColors[m_level - 1];
                    }
                }

            }
        }

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// A tooltip is about to be opened.
    /// If the trigger is attached to this power icon, initialize tooltip with this
    /// button's power def.
    /// Link it via the inspector.
    /// </summary>
    /// <param name="_tooltip">The tooltip about to be opened.</param>
    /// <param name="_trigger">The button which triggered the event.</param>
    public void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger) {
		// Make sure the trigger that opened the tooltip is linked to this icon
		if(_trigger != trigger) return;

        if (!_tooltip is PowerTooltip) return;

        PowerTooltip powerTooltip = (PowerTooltip)_tooltip;

		// Tooltip will take care of the rest
		if(powerTooltip != null) {
            // Initialize
            powerTooltip.InitFromDefinition(m_powerDef, m_mode);

            // Set lock state
            powerTooltip.SetLocked(m_lockIcon != null && m_lockIcon.activeSelf);	// Use lock icon visibility to determine whether power is locked or not
		}

        // Set arrow offset to make it point to this icon
        powerTooltip.SetArrowOffset(m_tooltipArrowOffset);
	}

	/// <summary>
	/// An event has been broadcasted.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Event data.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			// Language has been changed!
			case BroadcastEventType.LANGUAGE_CHANGED: {
				// Refresh some texts
				RefreshTexts();
			} break;
		}
	}
	//------------------------------------------------------------------------//
	// STATIC UTILS METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize a collection of power icons with a given dragon data setup.
	/// </summary>
	/// <param name="_powerIcons">Power icons to be initialized.</param>
	/// <param name="_dragonData">Dragon data to be used for initialization.</param>
	public static void InitPowerIconsWithDragonData(ref PowerIcon[] _powerIcons, IDragonData _dragonData) {
		// Check params
		if(_powerIcons == null) return;

		// Special case: if given dragon data is not valid, hide all icons
		if(_dragonData == null) {
			for(int i = 0; i < _powerIcons.Length; ++i) {
				_powerIcons[i].gameObject.SetActive(false);
			}
			return;
		}

		// Aux vars
		List<DefinitionNode> powerDefs = new List<DefinitionNode>();
		List<Mode> iconModes = new List<Mode>();

		// Skin
		DefinitionNode skinDef = skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _dragonData.disguise);
		if(skinDef == null) {
			powerDefs.Add(null);
		} else {
			powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, skinDef.Get("powerup")));   // Can be null
		}
		iconModes.Add(Mode.SKIN);

		// Special Dragon Powers
		if(_dragonData.type == IDragonData.Type.SPECIAL) {
			DragonDataSpecial dataSpecial = (DragonDataSpecial)_dragonData;
			for(int i = 1; i <= dataSpecial.m_powerLevel; ++i) {
				powerDefs.Add(dataSpecial.specialPowerDefsByOrder[i - 1]);
				iconModes.Add(Mode.SPECIAL_DRAGON);
			}
		}

		// Pets
		for(int i = 0; i < _dragonData.pets.Count; i++) {
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _dragonData.pets[i]);
			if(petDef == null) {
				powerDefs.Add(null);
			} else {
				powerDefs.Add(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup")));
			}
			iconModes.Add(Mode.PET);
		}

		// Initialize power icons
		for(int i = 0; i < _powerIcons.Length; i++) {
			// Get icon ref
			PowerIcon powerIcon = _powerIcons[i];

			// Hide if there are not enough powers defined
			if(i >= powerDefs.Count) {
				powerIcon.gameObject.SetActive(false);
				continue;
			}

			// Hide if there is no power associated
			if(powerDefs[i] == null) {
				// Except for classic dragon default skin, which we will leave the power for consistency
				// (we detect that knowing that skin power is idx 0 and checking dragon type)
				if(i == 0 && _dragonData.type == IDragonData.Type.CLASSIC) {
					powerIcon.InitFromDefinition(null, false, false);
				} else {
					powerIcon.gameObject.SetActive(false);
				}
				continue;   // Nothing else to do
			}

			// Everything ok! Initialize
			powerIcon.gameObject.SetActive(true);
			powerIcon.InitFromDefinition(powerDefs[i], false, false, iconModes[i]);
		}
	}
}