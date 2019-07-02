using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierIcon : MonoBehaviour, IBroadcastListener {

	[SerializeField] private Image m_icon;
	[SerializeField] private TextMeshProUGUI m_text;

	[Space]
	[SerializeField] private bool m_shortText = true;
	[SerializeField][Range(0, 1)] private float m_tooltipArrowOffset = 0.5f;


	private UITooltipTrigger m_trigger = null;
	private UITooltipTrigger trigger {
		get {
			if(m_trigger == null) m_trigger = GetComponentInChildren<UITooltipTrigger>();
			return m_trigger;
		}
	}

	private IModifierDefinition m_def;

	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}


	public void InitFromDefinition(IModifierDefinition _def) {
		m_def = _def;

		// Load from resources
		if (m_icon != null) {
			m_icon.sprite = Resources.Load<Sprite>(UIConstants.MODIFIER_ICONS_PATH + m_def.GetIconRelativePath());
		}

		if (m_text != null) {
			if(m_shortText) {
				m_text.text = m_def.GetDescriptionShort();
			} else {
				m_text.text = m_def.GetDescription();
			}
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
			powerTooltip.InitFromDefinition(m_def);

			// Set lock state
			powerTooltip.SetLocked(false);	// Use lock icon visibility to determine whether power is locked or not
		}

		// Set arrow offset to make it point to this icon
		_tooltip.SetArrowOffset(m_tooltipArrowOffset);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An event has been broadcasted.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Event data.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			case BroadcastEventType.LANGUAGE_CHANGED: {
				if(m_def != null) m_def.RebuildTexts();
				InitFromDefinition(m_def);
			} break;
		}
	}
}
