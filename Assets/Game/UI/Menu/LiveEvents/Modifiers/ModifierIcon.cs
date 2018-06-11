using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierIcon : MonoBehaviour {

	[SerializeField] private Image m_icon;
	[SerializeField] private TextMeshProUGUI m_text;

	[Space]
	[SerializeField][Range(0, 1)] private float m_tooltipArrowOffset = 0.5f;


	private UITooltipTrigger m_trigger = null;
	private UITooltipTrigger trigger {
		get {
			if(m_trigger == null) m_trigger = GetComponentInChildren<UITooltipTrigger>();
			return m_trigger;
		}
	}

	private DefinitionNode m_def;


	public void InitFromDefinition(IModifierDefinition _def) {
		m_def = _def.def;

		// Load from resources
		if (m_icon != null) {
			m_icon.sprite = Resources.Load<Sprite>(UIConstants.MODIFIER_ICONS_PATH + _def.GetIconRelativePath());
		}

		if (m_text != null) {
			m_text.text = _def.GetDescription();
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

		// Tooltip will take care of the rest
		PowerTooltip powerTooltip = _tooltip.GetComponent<PowerTooltip>();
		if(powerTooltip != null) {
			// Initialize
			powerTooltip.InitFromDefinition(m_def, PowerIcon.Mode.MODFIER);

			// Set lock state
			powerTooltip.SetLocked(false);	// Use lock icon visibility to determine whether power is locked or not
		}

		// Set arrow offset to make it point to this icon
		_tooltip.SetArrowOffset(m_tooltipArrowOffset);
	}
}
