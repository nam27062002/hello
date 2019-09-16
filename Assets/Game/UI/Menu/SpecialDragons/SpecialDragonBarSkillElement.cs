using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialDragonBarSkillElement : SpecialDragonBarElement
{

    [Separator("Skill")]
    [SerializeField] private Image[] m_icons = new Image[0];

    [SerializeField] private ParticleSystem m_unlockFX = null;


    private DefinitionNode m_def;
	private SpecialDragonBarTooltip m_tooltip;
	private UITooltipTrigger m_trigger;

	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Subscribe to external events
		Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, OnPowerUpgraded);
	}

	protected override void OnDisable() {
		// Call parent
		base.OnDisable();

		// Unsubscribe from external events
		// Subscribe to external events
		Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, OnPowerUpgraded);
	}

	public void SetDefinition(DefinitionNode _def) {
        m_def = _def;
		Sprite sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + m_def.Get("icon"));
        for (int i=0; i<m_icons.Length; i++)
        {
            m_icons[i].sprite = sprite;
        }
    }

	public void SetTooltip(SpecialDragonBarTooltip _tooltip) {
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
			m_icons[0].sprite
		);

		m_tooltip.SetUnlockLevel(
			m_unlockLevel,
			m_state == State.OWNED
		);
    }

	private void OnPowerUpgraded(DragonDataSpecial _dragonData) {
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
					PopupController popup = PopupManager.LoadPopup(PopupSpecialDragonSkillUnlocked.PATH);
					if(popup != null) {
						PopupSpecialDragonSkillUnlocked skillUnlockedPopup = popup.GetComponent<PopupSpecialDragonSkillUnlocked>();
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
