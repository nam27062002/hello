// GameObjectExt.cs
// 
// Created by Alger Ortín Castellví on 15/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the GameObject class.
/// </summary>
public static class GameObjectExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Generate and apply a unique name for this object in its current parent (no siblings with the same name).
	/// Costly method, avoid intensive usage (i.e. in the Update call).
	/// </summary>
	/// <param name="_obj">The object we're changing.</param>
	/// <param name="_nameBase">The prefix of the new name. The name will be composed by "prefix_#" where # is a unique numeric identifier.</param>
	public static void SetUniqueName(this GameObject _obj, string _nameBase) {
		// Get parent transform
		Transform parentTr = _obj.transform.parent;

		// We don't want double separators, so clean name base for trailing "_"
		// [AOC] Gotta love c#
		_nameBase = _nameBase.TrimEnd('_');

		// Try different names until one is not found in the parent
		int i = 0;
		string targetName = "";
		Transform childTr = null;
		do {
			// An object exists with this name?
			targetName = _nameBase + "_" + i.ToString();
			childTr = parentTr.Find(targetName);
			i++;
		} while(childTr != null);

		// We found our index, set new name to the target object
		_obj.name = targetName;
	}

	/// <summary>
	/// Set the layer to an object and all its children.
	/// </summary>
	/// <param name="_obj">The object we're changing.</param>
	/// <param name="_layerName">The name of the layer to be applied.</param>
	public static void SetLayerRecursively(this GameObject _obj, string _layerName) {
		// Get layer mask
		int layerMask = LayerMask.NameToLayer(_layerName);

		// Apply the layer to the object itself
		_obj.layer = layerMask;

		// Apply the layer to all children as well
		Transform[] children = _obj.GetComponentsInChildren<Transform>(true);
		for(int i = 0; i < children.Length; i++) {
			children[i].gameObject.layer = layerMask;
		}
	}
}
