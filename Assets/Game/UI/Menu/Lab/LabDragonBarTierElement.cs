using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarTierElement : LabDragonBarLockedElement {
    [Separator("Tier icons")]
    [SerializeField] private Sprite[] m_tierIconSprites;

    [SerializeField] private Image m_icon;

	private LabDragonBarTooltip m_tooltip;

    public void SetTier(int _index) {
        m_icon.sprite = m_tierIconSprites[_index];
    }

	public void SetTooltip(LabDragonBarTooltip _tooltip) {
		UITooltipTrigger trigger = GetComponent<UITooltipTrigger>();
		trigger.tooltip = _tooltip;
		m_tooltip = _tooltip;
	}

	public void OnTooltipOpen() {
		m_tooltip.Init(
			string.Empty,
			string.Empty,
			string.Empty
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
