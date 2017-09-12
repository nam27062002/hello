using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsTracker : MonoBehaviour {

	AchievementObjective[] m_objectives = null;
	
	// Use this for initialization
	void Start () 
	{
		// Check achievement is not already unlocked!
		Dictionary<string, DefinitionNode> kAchievementSKUs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.ACHIEVEMENTS);
		if (kAchievementSKUs.Count > 0)
		{
			m_objectives = new AchievementObjective[ kAchievementSKUs.Count ];
			int iSKUIdx = 0;
			foreach(KeyValuePair<string, DefinitionNode> kEntry in kAchievementSKUs)
			{
				m_objectives[iSKUIdx] = new AchievementObjective( kEntry.Value );
				m_objectives[iSKUIdx].OnObjectiveComplete.AddListener (CleanReportedAchievements );
				iSKUIdx++;
			}
		}
	}

	void CleanReportedAchievements()
	{
		for( int i = 0;i<m_objectives.Length; i++ )
		{
			if ( m_objectives[i] != null && m_objectives[i].reported  )
			{
				// Clean object
				m_objectives[i] = null;
			}
		}
	}

}
