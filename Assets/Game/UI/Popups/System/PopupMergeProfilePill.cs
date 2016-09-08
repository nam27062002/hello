using UnityEngine;
using UnityEngine.UI;
using System;
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

	public void Setup(ProgressComparatorSystem _progress, ProgressComparatorSystem _progressToCompare )
	{
		m_profile = _progress.UserProfile;    
        UserProfile _profileToCompare = _progressToCompare.UserProfile;

        //m_name.text = _profile.timePlayed.ToString("G", LocalizationManager.SharedInstance.Culture);
        m_name.text = GetTimeString(_progress.lastModified);
		m_name.color = (_progress.lastModified > _progressToCompare.lastModified) ? m_highlightTextColor : m_normalTextColor;

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

    private string GetTimeString(int unixTimeStamp)
    {
        //TODO Localize?
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dt = dt.AddSeconds(unixTimeStamp).ToLocalTime();
        //return dt.ToString("F");
        return dt.ToString("G");
    }
}
