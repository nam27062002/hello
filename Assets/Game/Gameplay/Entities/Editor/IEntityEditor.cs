using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof( IEntity), true)]
public class IEntityEditor : Editor {

	private void OnEnable() {
		if ( !Application.isPlaying )
		{
			IEntity ientity  = target as IEntity;
			if ( ientity != null )
				ientity.GetReferences();
		}
	}
	private void OnDisable() {
		if ( !Application.isPlaying )
		{
			IEntity ientity  = target as IEntity;
            if (ientity != null) {
                ientity.GetReferences();
                ientity.gameObject.SetActive(false);
            }            
		}
	}

	
}
