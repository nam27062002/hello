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
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a disguise power icon.
/// </summary>
public class PowerIcon : MonoBehaviour {
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

			// Name
			if(m_nameText != null) {
				m_nameText.Localize(_powerDef.Get("tidName"));
			}

			// Short description
			if(m_shortDescriptionText != null) {
                if (m_mode == Mode.SPECIAL_DRAGON) {
                    m_shortDescriptionText.text = _powerDef.GetLocalized("tidDescShort");
                } else {
                    m_shortDescriptionText.text = DragonPowerUp.GetDescription(_powerDef, true, m_mode == Mode.PET);	// Custom formatting depending on powerup type, already localized
                }
			}

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

		// Tooltip will take care of the rest
		PowerTooltip powerTooltip = _tooltip.GetComponent<PowerTooltip>();
		if(powerTooltip != null) {
			// Initialize
			powerTooltip.InitFromDefinition(m_powerDef, m_mode);

			// Set lock state
			powerTooltip.SetLocked(m_lockIcon != null && m_lockIcon.activeSelf);	// Use lock icon visibility to determine whether power is locked or not
		}

		// Set arrow offset to make it point to this icon
		_tooltip.SetArrowOffset(m_tooltipArrowOffset);
	}
}