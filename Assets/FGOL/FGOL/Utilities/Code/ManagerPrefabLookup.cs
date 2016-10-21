using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class LookupEntry
{
	public string type;
	public GameObject prefab;

	[NonSerialized]
	public GameObject instance;

	public bool CheckValid()
	{
		return (!string.IsNullOrEmpty(type) && (prefab != null));
	}
}

public class ManagerPrefabLookup : MonoBehaviour
{
	#region Inspector variables
	[SerializeField]
	private List<LookupEntry> m_lookupEntries;

	[SerializeField]
	private bool m_controlledInstantiate;
	#endregion

	#region Singleton
	private static ManagerPrefabLookup m_instance = null;
	public static ManagerPrefabLookup Instance
	{
		get 
		{
			return m_instance;
		}
	}
	#endregion

	#region Actual API
	public GameObject GetPrefabFromType(string typeName)
	{
		for(int i=0; i < m_lookupEntries.Count; i++)
		{
			if(m_lookupEntries[i].type.Equals(typeName))
			{
				return m_lookupEntries[i].prefab;
			}
		}
		return null;
	}
	public void SetInstanceForType(string typeName, GameObject instance)
	{
		for(int i=0; i < m_lookupEntries.Count; i++)
		{
			if(m_lookupEntries[i].type.Equals(typeName))
			{
				m_lookupEntries[i].instance = instance;
			}
		}
	}
	#endregion

	#region Monobehaviour
	void Awake()
	{
		Debug.Log("ManagerPrefabLookup.Awake");

		if(m_instance != null)
		{
			Destroy(this.gameObject);
			return;
		}

		m_instance = this;
		DontDestroyOnLoad(this.gameObject);

		if(m_controlledInstantiate)
		{
			for(int i=0; i < m_lookupEntries.Count; i++)
			{
				if(m_lookupEntries[i].CheckValid())
				{
					if(m_lookupEntries[i].instance != null)
					{
						// an instance has already been created before this, so just roll with it
						continue;
					}
					GameObject tObj = GameObject.Instantiate(m_lookupEntries[i].prefab) as GameObject;
					GameObject.DontDestroyOnLoad(tObj);
				}
			}
		}
	}
	#endregion
}
