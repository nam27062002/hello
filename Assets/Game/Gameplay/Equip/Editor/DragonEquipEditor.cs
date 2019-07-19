using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DragonEquip))]
public class DragonEquipEditor : Editor {

	DragonEquip m_target = null;
	string[] m_disguiseOptions;
	string[] m_disguiseDragons;
	int m_selectedDisguise = -1;

	public void Awake() {
		m_target = (DragonEquip)target;
		m_target.CacheRenderesAndMaterials();
		m_target.CacheAttachPoints();
		// If definitions are not loaded, do it now
		if(!ContentManager.ready){
			ContentManager.InitContent(true, false);
		}

		// Cache some important data
		List<string> skus = new List<string>();
		List<string> dragons = new List<string>();
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

		if(GUILayout.Button("Clean Disguise"))  {
			// Remove all accessories and set the skin to empty materials!
			m_target.RemoveAccessories();
			m_target.CleanSkin();
		}

		// Display list and store new index
		m_selectedDisguise = EditorGUILayout.Popup("Disguise", m_selectedDisguise, m_disguiseOptions);
		if ( GUILayout.Button("Equip Disguise" ))
		{
			HDAddressablesManager.Instance.Initialize();
			m_target.dragonSku = m_disguiseDragons[ m_selectedDisguise ];
			m_target.EquipDisguise( m_disguiseOptions[ m_selectedDisguise ] );
		}
    }
}
