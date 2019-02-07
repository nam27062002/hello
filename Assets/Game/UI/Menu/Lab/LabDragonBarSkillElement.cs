﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarSkillElement : LabDragonBarLockedElement {
    [Separator("Skill")]
    [SerializeField] private Image m_icon = null;

	[SerializeField] private ParticleSystem m_unlockFX = null;

    private DefinitionNode m_def;
	private LabDragonBarTooltip m_tooltip;
	private UITooltipTrigger m_trigger;

	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Subscribe to external events
		Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, OnSkillUpgraded);
	}

	protected override void OnDisable() {
		// Call parent
		base.OnDisable();

		// Unsubscribe from external events
		// Subscribe to external events
		Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, OnSkillUpgraded);
	}

	public void SetDefinition(DefinitionNode _def) {
        m_def = _def;
		m_icon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + m_def.Get("icon"));
    }

	public void SetTooltip(LabDragonBarTooltip _tooltip) {
		m_trigger = GetComponent<UITooltipTrigger>();
        m_trigger.tooltip = _tooltip;
        m_tooltip = _tooltip;
    }

    public void OnTooltipOpen() {
		// Show the tooltip to the left or to the right based on its position on 
		// screen, trying to avoid the player's fingers covering it.

		// Find out best direction (Multidirectional tooltip makes it easy for us)
		UITooltipMultidirectional.ShowDirection bestDir = m_tooltip.CalculateBestDirection(
			m_trigger.anchor.position,
			UITooltipMultidirectional.BestDirectionOptions.HORIZONTAL_ONLY
		);

		// Adjust offset based on best direction
		Vector2 offset = m_trigger.offset;
		if(bestDir == UITooltipMultidirectional.ShowDirection.LEFT) {
			offset.x = -Mathf.Abs(offset.x);
		} else if(bestDir == UITooltipMultidirectional.ShowDirection.RIGHT) {
			offset.x = Mathf.Abs(offset.x);
		}

		// Apply new offset and direction
		m_trigger.offset = offset;
		m_tooltip.SetupDirection(bestDir);

		m_tooltip.Init(
			m_def.GetLocalized("tidName"), 
			m_def.GetLocalized("tidDesc"), 
			m_icon.sprite
		);

		m_tooltip.SetRequiredTier(
			m_requiredTier,
			m_state != State.LOCKED
		);

		m_tooltip.SetUnlockLevel(
			m_unlockLevel,
			m_state == State.AVAILABLE || m_state == State.OWNED
		);
    }

	private void OnSkillUpgraded(DragonDataSpecial _dragonData) {
		// Unlocked skill upgrade matches the one represented by this element?
		if(_dragonData.biggestPowerDef == null) return;
		if(m_def.sku == _dragonData.biggestPowerDef.sku) {
			// Show some VFX
			if(m_unlockFX != null) {
				m_unlockFX.Play();
			}

			// SFX
			AudioController.Play("hd_lab_power_upgraded");

			// Lock input (to prevent any other stat upgrade while the popup is opened)
			Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, true);

			// Open info popup (after some delay)
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					PopupController popup = PopupManager.LoadPopup(PopupLabSkillUnlocked.PATH);
					if(popup != null) {
						PopupLabSkillUnlocked skillUnlockedPopup = popup.GetComponent<PopupLabSkillUnlocked>();
						skillUnlockedPopup.Init(m_def);
						popup.Open();
					}

					// Unlock input
					Messenger.Broadcast<bool>(MessengerEvents.UI_LOCK_INPUT, false);
				}, 0.5f
			);
		}
	}
}
