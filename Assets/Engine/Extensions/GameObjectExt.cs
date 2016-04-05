﻿// GameObjectExt.cs
// 
// Created by Alger Ortín Castellví on 15/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
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
	/// Toggles the local active state of this object.
	/// </summary>
	/// <param name="_obj">The object to be toggled.</param>
	public static void ToggleActive(this GameObject _obj) {
		_obj.SetActive(!_obj.activeSelf);
	}

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

	//------------------------------------------------------------------//
	// HIERARCHY NAVIGATION HELPERS										//
	// Imported from Hungry Dragon										//
	//------------------------------------------------------------------//
	/// <summary>
	/// Find a child transform by name, returning the first one encountered 
	/// (searches depth first, does not necessarily find closest one to the 
	/// root, which maybe we want?)
	/// </summary>
	/// <returns></returns>
	/// <param name="_trans"></param>
	/// <param name="_name"></param>
	public static Transform FindTransformRecursive(this Transform _trans, string _name) {
		// Found!
		if(_trans.name == _name)  return _trans;

		// Not found, iterate children transforms
		foreach(Transform t in _trans) {
			Transform found  = t.FindTransformRecursive(_name);
			if(found != null) return found;
		}
		return null;
	}

	/// <summary>
	/// Recursively find child Transform on Component.
	/// </summary>
	/// <returns></returns>
	/// <param name="_comp"></param>
	/// <param name="_name"></param>
	public static Transform FindTransformRecursive(this Component _comp, string _name) {
		return _comp.transform.FindTransformRecursive(_name);
	}

	/// <summary>
	/// Recursively find child Transform on GameObject.
	/// </summary>
	/// <returns></returns>
	/// <param name="_obj"></param>
	/// <param name="_name"></param>
	public static Transform FindTransformRecursive(this GameObject _obj, string _name) {
		return _obj.transform.FindTransformRecursive(_name);
	}

	/// <summary>
	/// Recursively find child GameObject on Component.
	/// </summary>
	/// <returns></returns>
	/// <param name="_comp"></param>
	/// <param name="_name"></param>
	public static GameObject FindObjectRecursive(this Component _comp, string _name) {
		Transform t = _comp.transform.FindTransformRecursive(_name);
		return (t == null) ? null : t.gameObject;
	}

	/// <summary>
	/// Recursively find child GameObject on GameObject.
	/// </summary>
	/// <returns></returns>
	/// <param name="_obj"></param>
	/// <param name="_name"></param>
	public static GameObject FindObjectRecursive(this GameObject _obj, string _name) {
		Transform t = _obj.transform.FindTransformRecursive(_name);
		return (t == null) ? null : t.gameObject;
	}

	/// <summary>
	/// Recursively find child Transform on Transform with part of the name.
	/// </summary>
	/// <returns></returns>
	/// <param name="_trans"></param>
	/// <param name="_name"></param>
	public static Transform FindTransformRecursivePartialName(this Transform _trans, string _namePart) {
		if(_trans.name.Contains(_namePart)) return _trans;
		
		foreach(Transform t in _trans) {
			Transform found  = t.FindTransformRecursivePartialName(_namePart);
			if(found != null) return found;
		}
		return null;
	}

	/// <summary>
	/// Recursively find child GameObject on GameObject with part of the name.
	/// </summary>
	/// <returns></returns>
	/// <param name="_obj"></param>
	/// <param name="_name"></param>
	public static GameObject FindObjectRecursivePartialName(this GameObject _obj, string _name) {
		Transform t = _obj.transform.FindTransformRecursivePartialName(_name);
		return (t == null) ? null : t.gameObject;
	}

	/// <summary>
	/// Returns the first component of type T found in any of the child objects named <paramref name="_objName"/>.
	/// </summary>
	/// <returns>The first component of type T found in any of the child objects named <paramref name="_objName"/>.</returns>
	/// <param name="_t">The root transform to look wihtin.</param>
	/// <param name="_objName">The name to look for</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	public static T FindComponentRecursive<T>(this Transform _t, string _objName) where T : Component {
		// Found!
		T c = _t.GetComponent<T>();
		if(_t.name == _objName && c != null) return c;

		// Not found, iterate children transforms
		foreach(Transform t in _t) {
			c = t.FindComponentRecursive<T>(_objName);
			if(c != null) return c;
		}
		return null;
	}
	/// <summary>
	/// Returns the first component of type T found in any of the child objects named <paramref name="_objName"/>.
	/// </summary>
	/// <returns>The first component of type T found in any of the child objects named <paramref name="_objName"/>.</returns>
	/// <param name="_obj">The root object to look wihtin.</param>
	/// <param name="_objName">The name to look for</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	public static T FindComponentRecursive<T>(this GameObject _obj, string _objName) where T : Component {
		return _obj.transform.FindComponentRecursive<T>(_objName);
	}

	/// <summary>
	/// Returns the first component of type T found in any of the child objects named <paramref name="_objName"/>.
	/// </summary>
	/// <returns>The first component of type T found in any of the child objects named <paramref name="_objName"/>.</returns>
	/// <param name="_comp">The root component to look wihtin.</param>
	/// <param name="_objName">The name to look for</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	public static T FindComponentRecursive<T>(this Component _comp, string _objName) where T : Component {
		return _comp.transform.FindComponentRecursive<T>(_objName);
	}

	/// <summary>
	/// Returns the first component of type T found in any of the child objects.
	/// </summary>
	/// <returns>The first component of type T found in any of the children objects.</returns>
	/// <param name="_t">The root transform to look wihtin.</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	public static T FindComponentRecursive<T>(this Transform _t) where T : Component {
		// Found!
		T c = _t.GetComponent<T>();
		if(c != null) return c;

		// Not found, iterate children transforms
		foreach(Transform t in _t) {
			c = t.FindComponentRecursive<T>();
			if(c != null) return c;
		}
		return null;
	}

	/// <summary>
	/// Returns the first component of type T found in any of the child objects.
	/// </summary>
	/// <returns>The first component of type T found in any of the children objects.</returns>
	/// <param name="_obj">The root object to look wihtin.</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	public static T FindComponentRecursive<T>(this GameObject _obj) where T : Component {
		return _obj.transform.FindComponentRecursive<T>();
	}

	/// <summary>
	/// Returns the first component of type T found in any of the child objects.
	/// </summary>
	/// <returns>The first component of type T found in any of the children objects.</returns>
	/// <param name="_comp">The root component to look wihtin.</param>
	/// <typeparam name="T">The type to look for.</typeparam>
	public static T FindComponentRecursive<T>(this Component _comp) where T : Component {
		return _comp.transform.FindComponentRecursive<T>();
	}

	/// <summary>
	/// Find first component of a given type in game object's parent hirearchy.
	/// </summary>
	/// <returns></returns>
	/// <param name="_obj"></param>
	/// <typeparam name="T"></typeparam>
	public static T FindComponentInParents<T>(this GameObject _obj) where T : Component {
		T comp = _obj.GetComponent<T>();
		if(comp == null)  {
			// recurse
			Transform parent = _obj.transform.parent;
			if(parent != null) {
				return parent.gameObject.FindComponentInParents<T>();
			}
		}
		return comp;
	}

	/// <summary>
	/// Find a child object and attach it to a different child object.
	/// Often need this for things like attach points and child collision etc. on prefabs.  e.g. to have a collider on a character's
	/// head, it's safer to stick it on an object parented to the root, and then attach it to the head at runtime (the prefab is
	/// less likely to randomly screw up if the model is updated).
	/// </summary>
	/// <param name="_t"></param>
	/// <param name="_childName"></param>
	/// <param name="_newParentName"></param>
	public static void ReparentChild(this Transform _t, string _childName, string _newParentName) {
		Transform child = _t.FindTransformRecursive(_childName);
		Transform newParent = _t.FindTransformRecursive(_newParentName);
		Debug.Assert(child != null);
		Debug.Assert(newParent != null);
		child.parent = newParent;
	}

	/// <summary>
	/// Version where we already have transform ref for the child.
	/// </summary>
	/// <param name="_t"></param>
	/// <param name="_child"></param>
	/// <param name="_newParentName"></param>
	public static void ReparentChild(this Transform _t, Transform _child, string _newParentName) {
		Transform newParent = _t.FindTransformRecursive(_newParentName);
		Debug.Assert(newParent != null);
		_child.parent = newParent;
	}

	/// <summary>
	/// Version where we already have transform ref for the child.
	/// </summary>
	/// <param name="_t"></param>
	/// <param name="_child"></param>
	/// <param name="_newParentName"></param>
	public static void ReparentChild(this Transform _t, string _childName, Transform _newParent) {
		Transform child = _t.FindTransformRecursive(_childName);
		Debug.Assert(child != null);
		child.parent = _newParent;
	}

	/// <summary>
	/// Get component if it exists, otherwise add it and return the newly added one.
	/// </summary>
	/// <returns></returns>
	/// <param name="_obj"></param>
	/// <typeparam name="T"></typeparam>
	public static T ForceGetComponent<T>(this GameObject _obj) where T : Component {
		return _obj.GetComponent<T>() ?? _obj.AddComponent<T>();
	}

	/// <summary>
	/// Get component if it exists, otherwise add it and return the newly added one.
	/// </summary>
	/// <returns></returns>
	/// <param name="_comp"></param>
	/// <typeparam name="T"></typeparam>
	public static T ForceGetComponent<T>(this Component _comp) where T: Component {
		return _comp.GetComponent<T>() ?? _comp.gameObject.AddComponent<T>();
	}

	/// <summary>
	/// Disable a component of type T if it exists (technically Behaviour rather 
	/// than component, it has to have the 'enabled' property).
	/// </summary>
	/// <param name="_obj"></param>
	/// <typeparam name="T"></typeparam>
	public static void DisableComponent<T>(this GameObject _obj) where T : Behaviour {
		T comp = _obj.GetComponent<T>();
		if(comp != null) comp.enabled = false;
	}

	/// <summary>
	/// Version of GetComponentInChildren that finds the one closest to the root of the hierarchy.
	/// </summary>
	/// <returns></returns>
	/// <param name="_comp"></param>
	/// <typeparam name="T"></typeparam>
	public static T GetFirstComponentInChildren<T>(this Component _comp) where T : Component {
		Transform trans = _comp.transform;

		// first see if it's on the root component
		T t = _comp.GetComponent<T>();
		if(t != null) return t;

		// next see if it can be found at the first child level
		foreach(Transform tr in trans) {
			t = tr.GetComponent<T>();
			if(t != null) return t;
		}

		// haven't found it, now we'll search the entire hierarchy
		// and if we find any, find the least deep one.
		//
		// The first 2 checks are not necessary, just an optimization
		// to stop us trawling through the entire hierarchy every time
		// because this is a bit crap.
		T[] ts = _comp.GetComponentsInChildren<T>();
		if((ts == null) || (ts.Length==0)) return null;

		// found at least one, now search for the one nearest the root
		T bestOne = null;
		int bestDepth = 9999;
		foreach(T test in ts) {
			Transform tr = test.transform;
			int depth = 0;
			while(tr != trans) {				// count how many parents to step through before we get back to the root
				depth++;
				tr = tr.parent;
			}

			if(depth < bestDepth) {
				bestDepth = depth;
				bestOne = test;
			}
		}	
		return bestOne;
	}

	/// <summary>
	/// Version of GetComponentInChildren that finds the one closest to the root of the hierarchy.
	/// </summary>
	/// <returns></returns>
	/// <param name="_obj"></param>
	/// <typeparam name="T"></typeparam>
	public static T GetFirstComponentInChildren<T>(this GameObject _obj) where T : Component {
		return _obj.transform.GetFirstComponentInChildren<T>();
	}

	//------------------------------------------------------------------//
	// TRUE STATIC METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Find the first component of the given type within the whole hierarchy of
	/// the current scene.
	/// </summary>
	/// <returns>The first object of type T found in the scene.</returns>
	/// <param name="_includeInactive">Whether to include inactive objects in the search.</param>
	/// <typeparam name="T">Type to search.</typeparam>
	public static T FindComponent<T>(bool _includeInactive = true, string _name = "") where T : Component {
		// Use Unity methods if inactives not required
		if(_includeInactive) {
			// Traverse whole hierarchy
			GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
			for(int i = 0; i < rootObjs.Length; i++) {
				// Check name?
				T searchResult = null;
				if(string.IsNullOrEmpty(_name)) {
					searchResult = rootObjs[i].FindComponentRecursive<T>();
				} else {
					searchResult = rootObjs[i].FindComponentRecursive<T>(_name);
				}

				// If found, return! Otherwise check the next root object
				if(searchResult != null) return searchResult;
			}
		} else {
			// Filter by name?
			if(string.IsNullOrEmpty(_name)) {
				// Just get the first component of type T
				return GameObject.FindObjectOfType<T>();
			} else {
				// Get all components of type T and find the first one matching the requested name
				T[] matches = GameObject.FindObjectsOfType<T>();
				for(int i = 0; i < matches.Length; i++) {
					if(matches[i].name == _name) {
						return matches[i];
					}
				}
			}
		}

		// Object matching the search criteria not found in the scene
		return null;
	}
}
