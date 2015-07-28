// CompensateCameraMovement.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Simple behaviour to keep a UI element at a given world position.
/// </summary>
public class WorldSpacePositioner : MonoBehaviour {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	public Vector3 targetWorldPosition = Vector3.zero;	// The world position to stick to
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Update is called once per frame.
	/// Use late update since we want the animation to change the relative position 
	/// during the Update() call and then apply the offset.
	/// </summary>
	void LateUpdate() {
		// Animation also modifies object's position around [0,0], so we just have to apply the offset afterwards (that's why we use LateUpdate())
		Vector2 offset = ScreenUtils.WorldToCanvas(targetWorldPosition);	// The 2D offset to be applied to the object to position it at the 2D projection in the canvas of the target world position
		RectTransform tr = transform as RectTransform;
		tr.anchoredPosition += offset;
	}
	#endregion
}
#endregion
