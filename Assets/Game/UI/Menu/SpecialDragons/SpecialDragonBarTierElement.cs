﻿using UnityEngine;
using UnityEngine.UI;

public class SpecialDragonBarTierElement : SpecialDragonBarElement {
    [Separator("Tier")]
	[SerializeField] private ParticleSystem m_unlockFX = null;

	private SpecialDragonBarTooltip m_tooltip;
    private DefinitionNode m_def;

	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Subscribe to external events
		Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnLevelUpgraded);
	}

	protected override void OnDisable() {
		// Call parent
		base.OnDisable();

		// Unsubscribe from external events
		// Subscribe to external events
		Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnLevelUpgraded);
	}

    
    public void SetTier(DefinitionNode _tier) {
        m_def = _tier;
    }

    public void SetTooltip(SpecialDragonBarTooltip _tooltip) {
		UITooltipTrigger trigger = GetComponent<UITooltipTrigger>();
		trigger.tooltip = _tooltip;
		m_tooltip = _tooltip;
	}

	public void OnTooltipOpen() {

        
        int numPets = m_def.GetAsInt("petsSlotsAvailable");
        string description = LocalizationManager.SharedInstance.Localize("TID_SPECIAL_DRAGON_INFO_TIER_DESCRIPTION",
            "x" + StringUtils.FormatNumber(numPets),
            "x" + StringUtils.FormatNumber(m_def.GetAsFloat("furyScoreMultiplier", 2), 0)
        );

        m_tooltip.Init(
			string.Empty,
            description,
			string.Empty
		);

		m_tooltip.SetUnlockLevel(
			m_unlockLevel,
			m_state == State.OWNED
		);
	}

	private void OnLevelUpgraded(DragonDataSpecial _dragonData) {
		// Unlocked tier matches the one represented by this element?
		if(m_def.GetAsInt("upgradeLevelToUnlock") == (int)_dragonData.Level) {
			// Show some VFX
			if(m_unlockFX != null) {
				m_unlockFX.Play();
			}

			// SFX
			AudioController.Play("hd_lab_tier_upgraded");

			// Lock input (to prevent any other stat upgrade while the popup is opened)
			Messenger.Broadcast<bool, float>(MessengerEvents.UI_LOCK_INPUT, true, 0f);

			// Open info popup (after some delay)
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					PopupController popup = PopupManager.LoadPopup(PopupSpecialDragonTierUnlocked.PATH);
					if(popup != null) {
						PopupSpecialDragonTierUnlocked tierUnlockedPopup = popup.GetComponent<PopupSpecialDragonTierUnlocked>();
						tierUnlockedPopup.Init(_dragonData.tierDef, m_def);
						popup.Open();
					}

					// Unlock input
					Messenger.Broadcast<bool,float>(MessengerEvents.UI_LOCK_INPUT, false, 0f);
				}, 0.5f
			);
		}
	}
}
