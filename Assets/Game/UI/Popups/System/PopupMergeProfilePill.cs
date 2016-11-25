using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class PopupMergeProfilePill : MonoBehaviour 
{
	public TextMeshProUGUI m_name;
	public TextMeshProUGUI m_lastSave;
	public TextMeshProUGUI m_eggs;
	public TextMeshProUGUI m_timePlayed;
	public TextMeshProUGUI m_dragons;

	[Space]
	public Color m_normalTextColor = Color.white;
	public Color m_highlightTextColor = Color.green;

	UserProfile m_profile;

    public GameObject m_highlightGO;

	public void Setup(ProgressComparatorSystem _progress, ProgressComparatorSystem _progressToCompare, bool _highlight )
	{
		m_profile = _progress.UserProfile;    
        UserProfile _profileToCompare = _progressToCompare.UserProfile;

        m_name.text = _progress.lastDevice;
        m_lastSave.text = GetTimeString(_progress.lastModified);
        m_lastSave.color = (_progress.lastModified > _progressToCompare.lastModified) ? m_highlightTextColor : m_normalTextColor;

		m_eggs.text = StringUtils.FormatNumber(m_profile.eggsCollected);
        m_eggs.color = (m_profile.eggsCollected > _profileToCompare.eggsCollected ? m_highlightTextColor : m_normalTextColor);

		m_timePlayed.text = TimeUtils.FormatTime(m_profile.timePlayed, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 3, TimeUtils.EPrecision.HOURS);
        m_timePlayed.color = (m_profile.timePlayed > _profileToCompare.timePlayed ? m_highlightTextColor : m_normalTextColor);		

		int dragons1 = m_profile.GetNumOwnedDragons();
		int dragons2 = _profileToCompare.GetNumOwnedDragons();
		m_dragons.text = LocalizationManager.SharedInstance.Localize(
			"TID_FRACTION", 
			StringUtils.FormatNumber(dragons1), 
			StringUtils.FormatNumber(DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DRAGONS).Count)
		);
		m_dragons.color = (dragons1 > dragons2 ? m_highlightTextColor : m_normalTextColor);

        IsHighlightEnabled = _highlight;
	}

    private string GetTimeString(int unixTimeStamp)
    {
        //TODO Localize?
        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dt = dt.AddSeconds(unixTimeStamp).ToLocalTime();
        //return dt.ToString("F");
        return dt.ToString("G");
    }
    
    private bool IsHighlightEnabled
    {
        get
        {
            return m_highlightGO != null && m_highlightGO.activeSelf;
        }

        set
        {
            if (m_highlightGO != null)
            {
                m_highlightGO.SetActive(value);
            }
        }
    }
}
