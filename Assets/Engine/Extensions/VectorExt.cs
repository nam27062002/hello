// VectorExt.cs
// 
// Created by Alger Ortín Castellví on 09/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the Vector2, Vector3 and Vector4 classes.
/// </summary>
public static class VectorExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Find out the angle between two 3d vectors defining a custom shared plane.
	/// </summary>
	/// <returns>The angle [-179, 180] between two 3d vectors.</returns>
	/// <param name="_v1">The first vector of the angle.</param>
	/// <param name="_v2">The second vector of the angle.</param>
	/// <param name="_n">The normal of the plane used to determine what you would call "clockwise/counterclockwise".</param>
	public static float Angle360(this Vector3 _v1, Vector3 _v2, Vector3 _n) {
		// From http://stackoverflow.com/questions/19675676/calculating-actual-angle-between-two-vectors-in-unity3d
		float angle = Vector3.Angle(_v1, _v2);	// angle in [0,180]
		float sign = Mathf.Sign(Vector3.Dot(_n, Vector3.Cross(_v1, _v2)));
		float signedAngle = angle * sign;	// angle in [-179,180]
		//float angle360 =  (signedAngle + 180) % 360;	// angle in [0,360] (not used but included here for completeness)
		return signedAngle;
	}
}
