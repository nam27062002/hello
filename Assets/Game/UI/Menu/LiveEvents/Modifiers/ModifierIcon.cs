using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModifierIcon : MonoBehaviour {

	[SerializeField] private Image m_icon;
	[SerializeField] private TextMeshProUGUI m_text;


	public void InitFromDefinition(IModifierDefinition _def) {
		// Load from resources
		m_icon.sprite = Resources.Load<Sprite>(UIConstants.MODIFIER_ICONS_PATH + _def.GetIconRelativePath());
		m_text.text = _def.GetDescription();
	}
}
