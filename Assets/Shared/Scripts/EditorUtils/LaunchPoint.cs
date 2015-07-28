using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Auxiliar class to easily define a lookAt point for a game object (e.g. a camera).
/// </summary>
[ExecuteInEditMode]  
public class LaunchPoint : MonoBehaviour {
	#region MEMBERS ----------------------------------------------------------------------------------------------------
	public Vector3 launchPoint = Vector3.zero;	
	#endregion
	
	#region METHODS ----------------------------------------------------------------------------------------------------
	/// <summary>
	/// Logic update call.
	/// Don't do it when the game is running, it's only for editing purposes!!
	/// @see http://blog.brendanvance.com/2014/04/08/elegant-editor-only-script-execution-in-unity3d/comment-page-1/
	/// </summary>
	#if UNITY_EDITOR
	void Update () {
		if(EditorApplication.isPlayingOrWillChangePlaymode) {
			this.enabled = false;	// This way the Update() method won't be called again during this execution
		}
	}
	#endif
	#endregion
}
#endregion
