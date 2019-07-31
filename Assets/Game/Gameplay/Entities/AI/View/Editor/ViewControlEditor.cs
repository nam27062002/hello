using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof( ViewControl), true)]
public class ViewControlEditor : Editor {

	private void OnEnable() {
		if ( !Application.isPlaying )
		{
			ViewControl vc  = target as ViewControl;
			vc.GetReferences();
		}
	}
	private void OnDisable() {
		if ( !Application.isPlaying )
		{
			ViewControl vc  = target as ViewControl;
			vc.GetReferences();
		}
	}
	
}
