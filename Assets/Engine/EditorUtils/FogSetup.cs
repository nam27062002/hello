//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to easily configure Unity's default fog from a game object.
/// </summary>
[ExecuteInEditMode]  
public class FogSetup : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public bool fogEnabled = true;
	public Color fogColor = Color.white;
	public Range fogDistance = new Range(500f, 2000f);
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update () {
		RenderSettings.fog = fogEnabled;
		if(fogEnabled) {
			// [AOC] TODO!! Support other modes
			RenderSettings.fogMode = FogMode.Linear;
			RenderSettings.fogColor = fogColor;
			RenderSettings.fogStartDistance = fogDistance.min;
			RenderSettings.fogEndDistance = fogDistance.max;
		}
	}
}
