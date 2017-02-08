using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HungryLettersManager))]
public class HungryLettersManagerEditor : Editor 
{

	public override void OnInspectorGUI()
    { 
        DrawDefaultInspector();
        

        if(GUILayout.Button("Retrieve Spawn Points"))
        {
            HungryLettersManager manager = (HungryLettersManager) target;
            manager.ClearSpawnPoints();
			HungryLettersPlaceholderEditor[] placeholderEditors = GameObject.FindObjectsOfType<HungryLettersPlaceholderEditor>();
			for( int i = 0; i<placeholderEditors.Length; i++ )
			{
				HungryLettersPlaceholder placeHolder = placeholderEditors[i].GetComponent<HungryLettersPlaceholder>();
				if ( placeHolder != null )
				{
					manager.AddSpawnerPoint( placeHolder, placeholderEditors[i].difficulty);
				}
			}
			EditorUtility.SetDirty( target );
        }
    }
}
