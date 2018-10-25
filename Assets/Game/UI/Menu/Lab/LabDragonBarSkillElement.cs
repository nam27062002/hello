using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarSkillElement : LabDragonBarLockedElement {
    [Separator("Skill")]
    [SerializeField] private Image m_icon = null;

	[SerializeField] private ParticleSystem m_unlockFX = null;

    private DefinitionNode m_def;
	private LabDragonBarTooltip m_tooltip;

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
        UITooltipTrigger trigger = GetComponent<UITooltipTrigger>();
        trigger.tooltip = _tooltip;
        m_tooltip = _tooltip;
    }

    public void OnTooltipOpen() {
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
		Debug.Log(Color.cyan.Tag(m_def.sku + " | " + _dragonData.biggestPowerDef.sku));
		if(m_def.sku == _dragonData.biggestPowerDef.sku) {
			// Show some VFX
			if(m_unlockFX != null) {
				m_unlockFX.Play();
			}

			// SFX
			AudioController.Play("hd_lab_power_upgraded");
		}
	}
}
