using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierIcon : MonoBehaviour {

	[SerializeField] private Image m_icon;
	[SerializeField] private TextMeshProUGUI m_text;


	public void InitFromDefinition(IModifierDefinition _def) {
		// Load from resources
		if (m_icon != null) {
			m_icon.sprite = Resources.Load<Sprite>(UIConstants.MODIFIER_ICONS_PATH + _def.GetIconRelativePath());
		}

		if (m_text != null) {
			m_text.text = _def.GetDescription();
		}
	}
}
