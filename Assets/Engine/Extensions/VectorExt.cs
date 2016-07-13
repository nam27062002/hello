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

	/// <summary>
	/// Compute the distanece between this vector and another.
	/// </summary>
	/// <returns>The distance between this vector and <paramref name="_v2"/></returns>
	/// <param name="_v1">This vector.</param>
	/// <param name="_v2">The other vector.</param>
	public static float Distance(this Vector3 _v1, Vector3 _v2) {
		return Vector3.Distance(_v1, _v2);
	}

	/// <summary>
	/// Initializes the vector fields with the given string in the format "(x.x, y.y, z.z)" 
	/// (same as ToString() output with invariant culture).
	/// From http://answers.unity3d.com/questions/1134997/string-to-vector3.html
	/// </summary>
	/// <returns>A new vector with the values from the parsed string.</returns>
	/// <param name="_str">The string to be parsed.</param>
	public static Vector3 ParseVector3(string _str) {
		// Remove the parentheses
		if(_str.StartsWith("(") && _str.EndsWith(")")) {
			_str = _str.Substring(1, _str.Length - 2);
		}

		// Remove all spaces
		_str.Replace(" ", "");

		// Split individual components
		string[] componentsStr = _str.Split(',');

		// Store them into the new vector
		Vector3 v = new Vector3();
		if(componentsStr.Length > 0) v.x = float.Parse(componentsStr[0], System.Globalization.CultureInfo.InvariantCulture);
		if(componentsStr.Length > 1) v.y = float.Parse(componentsStr[1], System.Globalization.CultureInfo.InvariantCulture);
		if(componentsStr.Length > 2) v.z = float.Parse(componentsStr[2], System.Globalization.CultureInfo.InvariantCulture);
		return v;
	}
}
