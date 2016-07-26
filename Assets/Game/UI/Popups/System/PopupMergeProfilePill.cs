using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupMergeProfilePill : MonoBehaviour 
{
	public Text m_name;
	public Text m_softCurrency;
	public Text m_hardCurrency;
	public Text m_maxScore;
	public Text m_dragons;

	[Space]
	public Color m_normalTextColor = Color.white;
	public Color m_highlightTextColor = Color.green;

	UserProfile m_profile;

	public void Setup( UserProfile _profile, UserProfile _profileToCompare )
	{
		m_profile = _profile;

		m_name.text = _profile.saveTimestamp.ToString("G", LocalizationManager.SharedInstance.Culture);
		m_name.color = (_profile.saveTimestamp.CompareTo(_profileToCompare.saveTimestamp) > 0 ? m_highlightTextColor : m_normalTextColor);

		m_softCurrency.text = StringUtils.FormatNumber(m_profile.coins);
		m_softCurrency.color = (m_profile.coins > _profileToCompare.coins ? m_highlightTextColor : m_normalTextColor);

		m_hardCurrency.text =  StringUtils.FormatNumber(m_profile.pc);
		m_hardCurrency.color = (m_profile.pc > _profileToCompare.pc ? m_highlightTextColor : m_normalTextColor);

		m_maxScore.text = StringUtils.FormatNumber(m_profile.highScore);
		m_maxScore.color = (m_profile.highScore > _profileToCompare.highScore ? m_highlightTextColor : m_normalTextColor);

		int dragons1 = m_profile.GetNumOwnedDragons();
		int dragons2 = _profileToCompare.GetNumOwnedDragons();
		m_dragons.text = LocalizationManager.SharedInstance.Localize(
			"TID_FRACTION", 
			StringUtils.FormatNumber(dragons1), 
			StringUtils.FormatNumber(DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DRAGONS).Count)
		);
		m_dragons.color = (dragons1 > dragons2 ? m_highlightTextColor : m_normalTextColor);
	}
}
