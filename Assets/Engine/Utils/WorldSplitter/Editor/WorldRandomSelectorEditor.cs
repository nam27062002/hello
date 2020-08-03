using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldRandomSelector))]
public class WorldRandomSelectorEditor : Editor {
    
	public override void OnInspectorGUI() {
		WorldRandomSelector selector = target as WorldRandomSelector;

		selector.RefreshElementCount();
        DrawDefaultInspector();	
    }
}
