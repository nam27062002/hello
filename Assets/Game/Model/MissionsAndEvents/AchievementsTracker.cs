using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsTracker {

	Dictionary<string, AchievementObjective> m_objectives = new Dictionary<string, AchievementObjective>();
	private bool m_initialized = false;
	// Use this for initialization
	public void Initialize() 
	{
		// Check achievement is not already unlocked!
		m_objectives.Clear();
		Dictionary<string, DefinitionNode> kAchievementSKUs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.ACHIEVEMENTS);
		if (kAchievementSKUs.Count > 0)
		{
			foreach(KeyValuePair<string, DefinitionNode> kEntry in kAchievementSKUs)
			{
				AchievementObjective newObjective = new AchievementObjective( kEntry.Value );
				m_objectives.Add( kEntry.Key, newObjective);
			}
		}
		Messenger.AddListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, OnGooglePlayEvent);
		m_initialized = true;
	}

	public void Dispose()  // destructor
    {
        if (m_initialized)
        {
			Messenger.RemoveListener(MessengerEvents.GOOGLE_PLAY_STATE_UPDATE, OnGooglePlayEvent);
			m_objectives.Clear();
			m_initialized = false;
        }
    }

	void OnGooglePlayEvent()
	{
		if ( ApplicationManager.instance.GameCenter_IsAuthenticated() )
		{
			UpdateAchievementsProgress();	
		}
	}

	public void UpdateAchievementsProgress()
	{	
		foreach(KeyValuePair<string, AchievementObjective> kEntry in m_objectives)
		{
			if ( !kEntry.Value.reported || kEntry.Value.reportProgress )
			{
                kEntry.Value.RefreshCurrentValue();
				kEntry.Value.OnValueChanged();
			}
		}
	}


	public void Load(SimpleJSON.JSONNode _data) {
		// Load current achievement progress
		SimpleJSON.JSONArray achievements = _data.AsArray;
		for( int i = 0; i<achievements.Count; ++i )
		{
			SimpleJSON.JSONNode node = achievements[i];
			if ( node.ContainsKey("sku") && node.ContainsKey("currentValue") )
			{
				string sku = node["sku"];
				if (m_objectives.ContainsKey(sku))
				{
					if (node["currentValue"].AsInt > m_objectives[sku].currentValue )
						m_objectives[sku].currentValue = node["currentValue"].AsInt;
				}
				else
				{
					// ???? no achievement anymore?
				}
			}
		}

		// If logged report finished, just in case
		if ( ApplicationManager.instance.GameCenter_IsAuthenticated() )
		{
			UpdateAchievementsProgress();
		}

	}

	public SimpleJSON.JSONNode Save() {
		// Save current achievement progress
		// Create new object, initialize and return it
		SimpleJSON.JSONArray achievements = new SimpleJSON.JSONArray();
		foreach(KeyValuePair<string, AchievementObjective> kEntry in m_objectives){
			SimpleJSON.JSONClass newNode = new SimpleJSON.JSONClass();
			newNode.Add("sku", kEntry.Value.achievementSku);
			newNode.Add("currentValue", kEntry.Value.currentValue);
			achievements.Add( newNode );
		}
		return achievements;
	}

}
