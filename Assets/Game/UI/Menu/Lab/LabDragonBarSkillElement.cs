using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarSkillElement : LabDragonBarLockedElement {
    [Separator("Skill")]
    [SerializeField] private Image m_icon = null;

    private DefinitionNode m_def;
    private UITooltip m_tooltip;


    public void SetDefinition(DefinitionNode _def) {
        m_def = _def;
        //m_icon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + m_def.Get("icon");
    }

    public void SetTooltip(UITooltip _tooltip) {
        UITooltipTrigger trigger = GetComponent<UITooltipTrigger>();
        trigger.tooltip = _tooltip;
        m_tooltip = _tooltip;
    }

    public void OnTooltipOpen() {
        m_tooltip.Init(m_def.Get("tidName"), m_def.Get("tidDesc"), m_icon.sprite);
    }
}
