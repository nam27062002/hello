using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragonCorpse))]
public class DragonCorpseEditor : Editor {

	DragonCorpse m_target = null;
	string[] m_disguiseOptions;
	string[] m_disguiseDragons;
	int m_selectedDisguise = -1;

	public void Awake() {
		m_target = (DragonCorpse)target;
		m_target.GetReferences();
		// If definitions are not loaded, do it now
		if(!ContentManager.ready){
			ContentManager.InitContent(true);
		}

		// Cache some important data
		List<string> skus = new List<string>();
		List<string> dragons = new List<string>();
		skus.Add("clean");
		dragons.Add("clean");
		Dictionary<string, DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DISGUISES);
		foreach( KeyValuePair<string, DefinitionNode> pair in defs)
		{
			skus.Add( pair.Key );
			dragons.Add( pair.Value.Get("dragonSku") );
		}
		m_disguiseOptions = skus.ToArray();
		m_disguiseDragons = dragons.ToArray();
	}

	
	public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		int oldIdx = m_selectedDisguise;

		// Display list and store new index
		m_selectedDisguise = EditorGUILayout.Popup("Disguise", oldIdx, m_disguiseOptions);

		// Special case: if nothing is selected, select first option
		if(m_selectedDisguise < 0 && m_disguiseOptions.Length > 0) m_selectedDisguise = 0;

		// If dragon has changed, store new value and update disguises list
		if(oldIdx != m_selectedDisguise && m_disguiseOptions.Length > 0) {
			string disguiseStr = m_disguiseOptions[m_selectedDisguise];
			if ( disguiseStr.CompareTo("clean") == 0 )
			{
				// Remove all accessories and set the skin to empty materials!
				m_target.RemoveAccessories();
				m_target.CleanSkin();
			}
			else
			{
				m_target.EquipDisguise(m_disguiseDragons[ m_selectedDisguise ], disguiseStr);
			}
		}
    }
}
