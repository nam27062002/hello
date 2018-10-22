using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarSkillElement : LabDragonBarLockedElement {
    [Separator("Skill")]
    [SerializeField] private Image m_icon = null;

    private DefinitionNode m_def;
	private LabDragonBarTooltip m_tooltip;


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
}
