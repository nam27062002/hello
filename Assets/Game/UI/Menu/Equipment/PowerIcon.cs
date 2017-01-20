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
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Image m_powerIcon = null;
	[Tooltip("Optional")] [SerializeField] private GameObject m_lockIcon = null;
	[Tooltip("Optional")] [SerializeField] private Localizer m_nameText = null;
	[Tooltip("Optional")] [SerializeField] private TextMeshProUGUI m_shortDescriptionText = null;

	[Space]
	[Comment("Optional, define an object for when there is a power or a placeholder for when ther is no power to show")]
	[Tooltip("Optional")] [SerializeField] private GameObject m_emptyObj = null;
	[Tooltip("Optional")] [SerializeField] private GameObject m_equippedObj = null;

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
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_powerIcon != null, "Required field!");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	/// <param name="_locked">Whether the power is locked or not.</param>
	public void InitFromDefinition(DefinitionNode _powerDef, bool _locked) {
		// Save definition
		m_powerDef = _powerDef;
		bool show = (_powerDef != null);

		// If both main and placeholder objects are defined, toggle them accordingly
		if(m_equippedObj != null && m_emptyObj != null) {
			m_equippedObj.SetActive(show);
			m_emptyObj.SetActive(!show);
		}

		// Otherwise, hide if given definition is not valid
		else if(!show) {
			if(anim != null) {
				anim.Hide();
			} else {
				this.gameObject.SetActive(false);
			}
			return;
		}

		// If showing, initialize all visible items
		if(show) {
			// Power icon
			if(m_powerIcon != null) {
				// Load power icons spritesheet
				Sprite[] allIcons = Resources.LoadAll<Sprite>("UI/Metagame/Powers/icons_powers");

				// Pick target icon, use first one if not found
				string iconName = _powerDef.GetAsString("icon");
				m_powerIcon.sprite = Array.Find<Sprite>(allIcons, (_sprite) => { return _sprite.name == iconName; });
				if(m_powerIcon.sprite == null) {
					m_powerIcon.sprite = allIcons[0];
				}
			}

			// Name
			if(m_nameText != null) {
				m_nameText.Localize(_powerDef.Get("tidName"));
			}

			// Short description
			if(m_shortDescriptionText != null) {
				m_shortDescriptionText.text = DragonPowerUp.GetDescription(_powerDef, true);	// Custom formatting depending on powerup type, already localized
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
		m_powerIcon.color = _locked ? Color.gray : Color.white;
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
			powerTooltip.InitFromDefinition(m_powerDef);

			// Set lock state
			powerTooltip.SetLocked(m_lockIcon != null && m_lockIcon.activeSelf);	// Use lock icon visibility to determine whether power is locked or not
		}

		// Set arrow offset to make it point to this icon
		_tooltip.SetArrowOffset(m_tooltipArrowOffset);
	}
}