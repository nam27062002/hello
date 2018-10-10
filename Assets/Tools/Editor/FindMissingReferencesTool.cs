// FindMissingReferencesTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple tool to help find all the missing references in open scenes.
/// From http://www.brunomikoski.com/playground/2015/3/31/convert-text-component-to-textmeshprougui-keeping-configurations
/// </summary>
public class FindMissingReferencesTool {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Find missing references among a given list of objects.
	/// </summary>
	/// <param name="_context">Context string.</param>
	/// <param name="_objs">The list of objects to be checked.</param>
	/// <param name="_includeNull">Optionally look for null values as well.</param>
	/// <param name="_typeFilter">Filter only some specific object types (the type of the missing ref object). <c>null</c> to include all types.</param>
	/// <param name="_componentFilter">Filter only some specific component types (the component containing the missing ref). <c>null</c> to include all types.</param>
	/// <param name="_reverseFilter">Use filters as a exclude list instead?</param>
	private static void FindMissingReferences(string _context, GameObject[] _objs, bool _includeNull, Type[] _typeFilter = null, Type[] _componentFilter = null, bool _reverseFilter = false) {
		// Looking for types by name is quite expensive, so better cache it :)
		Dictionary<string, Type> typesCache = new Dictionary<string, Type>();	// Dictionary of <SerializedProperty.type, Type>

		// Iterate all objects in the list
		int matches = 0;
		int processedObjects = 0;
		float progress = 0f;
		foreach(GameObject obj in _objs) {
			// Update progress and show bar
			processedObjects++;
			progress = (float)processedObjects/(float)_objs.Length;
			if(EditorUtility.DisplayCancelableProgressBar("Finding missing references...", "Processing " + processedObjects + "/" + _objs.Length + " objects in open scenes (" + obj.name + ")", progress)) {
				// Break loop!!!
				break;
			}

			// Iterate all components in each object
			Component[] components = obj.GetComponents<Component>();
			foreach(Component comp in components) {
				// Missing script! ^_^
				if(!comp) {
					Debug.LogError("Missing Component in GO: " + FullPath(obj), obj);
					continue;
				}

				// Check component type!
				// Ignore if filter list is null
				bool compFilterPassed = true;
				if(_componentFilter != null) {
					// Filter passed if the component's type is in it
					compFilterPassed = _componentFilter.Contains(comp.GetType());

					// Reverse filter usage?
					if(_reverseFilter) compFilterPassed = !compFilterPassed;
				}

				if(!compFilterPassed) {
					// Component type not in the filter list, skip to next component
					continue;
				}

				// Iterate all properties in the component, looking for object refs
				SerializedObject so = new SerializedObject(comp);
				SerializedProperty sp = so.GetIterator();
				while(sp.NextVisible(true)) {
					// Object ref type?
					if(sp.propertyType == SerializedPropertyType.ObjectReference) {
						// Null reference?
						if(sp.objectReferenceValue == null) {
							// Missing reference or _includeNull flag?
							if(_includeNull || sp.objectReferenceInstanceIDValue != 0) {
								// Skip if expected object type is not in the filter list
								// Last check since it's the most expensive one
								// First try to retrieve type from the cache
								Type targetType = null;
								if(!typesCache.TryGetValue(sp.type, out targetType)) {
									// Type not in the cache, get it via reflection and store it into the cache
									// Unfortunately, several types might match the same name :/
									// [AOC] HACK!! Since some very standard class names are repeated between several assemblies, let's just pick the most common ones
									Type[] matchingTypes = EditorUtils.GetPropertyObjectReferenceTypes(sp);
									foreach(Type t in matchingTypes) {
										if(t.Assembly.FullName.Contains("UnityEngine")
										|| t.Assembly.FullName.Contains("UnityEditor")
										|| t.Assembly.FullName.Contains("Assembly")
										|| t.Assembly.FullName.Contains("Calety")
										|| t.Assembly.FullName.Contains("DOTween")) {
											// Accepted assembly, pick this type as target
											targetType = t;
											break;
										}
									}

									// Add to cache
									typesCache[sp.type] = targetType;
									//Debug.Log(sp.type + " added to type cache (" + targetType + ")");
								}

								// No valid type was found
								if(targetType == null) continue;

								// Check property type!
								// Ignore if filter list is null
								bool typeFilterPassed = true;
								if(_typeFilter != null) {
									// Filter passed if the property's type is in it
									typeFilterPassed = _typeFilter.Contains(targetType);

									// Reverse filter usage?
									if(_reverseFilter) typeFilterPassed = !typeFilterPassed;
								}

								if(typeFilterPassed) {
									// [AOC] Missing ref! Print error
									matches++;
									string errorMessage = string.Format(
										"<color=#ff0000>MISSING REF!</color> {0}" +
										"\n{1}.{2} ({3})" +
										"\n[{4}]",
										FullPath(obj),
										comp,
										ObjectNames.NicifyVariableName(sp.name),
										targetType,
										_context
									);
									Debug.LogError(errorMessage, obj);
								}
							}
						}
					}
				}	// SerializedProperties loop
			}	// Components loop
		}	// GameObjects loop

		// Clear progress bar
		EditorUtility.ClearProgressBar();
		EditorUtility.DisplayDialog("Finding missing references DONE!", matches + " missing references found!\nCheck them in the console output :)", "Ok");
	}

	/// <summary>
	/// Finds the missing references in all open scenes.
	/// </summary>
	/// <param name="_includeNull">Optionally look for null values as well.</param>
	/// <param name="_typeFilter">Filter only some specific object types (the type of the missing ref object). <c>null</c> to include all types.</param>
	/// <param name="_componentFilter">Filter only some specific component types (the component containing the missing ref). <c>null</c> to include all types.</param>
	/// <param name="_reverseFilter">Use filters as a exclude list instead?</param>
	public static void FindMissingReferences(bool _includeNull, Type[] _typeFilter = null, Type[] _componentFilter = null, bool _reverseFilter = false) {
		// [AOC] Super-hardcore call that returns all loaded objects, no matter the state
		//		 Use HideFlags to filter Unity's system objects
		GameObject[] objs = Resources.FindObjectsOfTypeAll<GameObject>()
			.Where((GameObject _go) => {
				return string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_go)) 	// Ignore raw assets (png, fbx, etc.)
					&& _go.hideFlags == HideFlags.None;							// Ignore system objs
			}).ToArray();

		// Find missing refs!
		FindMissingReferences(EditorApplication.currentScene, objs, _includeNull, _typeFilter, _componentFilter, _reverseFilter);
	}

	/// <summary>
	/// Recursive method to get the full path of an object in the scene hierarchy.
	/// </summary>
	/// <returns>The path.</returns>
	/// <param name="_go">Target game object.</param>
	private static string FullPath(GameObject _go) {
		if(_go.transform.parent == null) {
			return _go.name;
		}
		return FullPath(_go.transform.parent.gameObject) + "/" + _go.name;
	}
}