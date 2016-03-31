// TransformExt.cs
// 
// Created by Alger Ortín Castellví on 30/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																  	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the Transform and RectTransform classes.
/// </summary>
public static class TransformExt {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// STATIC EXTENSION METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Transforms the given point from this transform space to the target one.
	/// </summary>
	/// <returns>The equivalent of <paramref name="_point"/> in the target space.</returns>
	/// <param name="_sourceSpace">Local space of <paramref name="_point"/>.</param>
	/// <param name="_point">The point in local coords to be transformed.</param>
	/// <param name="_targetSpace">The space to transform the point to. If <c>null</c>, world space will be used.</param>
	public static Vector3 TransformPoint(this Transform _sourceSpace, Vector3 _point, Transform _targetSpace) {
		// Transform to world coords
		_point = _sourceSpace.TransformPoint(_point);

		// If a target space is defined, convert to that space
		if(_targetSpace != null) {
			return _targetSpace.InverseTransformPoint(_point);
		}

		// Otherwise return world coords.
		return _point;
	}

	/// <summary>
	/// Transforms the given rect from this transform space to the target one.
	/// </summary>
	/// <returns>The equivalent of <paramref name="_rect"/> in the target space.</returns>
	/// <param name="_sourceSpace">Local space of <paramref name="_rect"/>.</param>
	/// <param name="_rect">The rectangle in local coords to be transformed.</param>
	/// <param name="_targetSpace">The space to transform the rect to. If <c>null</c>, world space will be used.</param>
	public static Rect TransformRect(this Transform _sourceSpace, Rect _rect, Transform _targetSpace) {
		// Create a new rect and use TransformPoint methods for min and max points.
		Rect newRect = new Rect();
		newRect.min = _sourceSpace.TransformPoint(new Vector3(_rect.xMin, _rect.yMin, 0f), _targetSpace);
		newRect.max = _sourceSpace.TransformPoint(new Vector3(_rect.xMax, _rect.yMax, 0f), _targetSpace);
		return newRect;
	}
}