// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		// Delete current dragons
		MenuDragonPreview[] toDelete = GetComponentsInChildren<MenuDragonPreview>();
		for(int i = 0; i < toDelete.Length; i++) {
			GameObject.DestroyImmediate(toDelete[i].gameObject);
		}

		// Load dragon definitions
		List<DefinitionNode> defs = DefinitionsManager.GetDefinitions(DefinitionsCategory.DRAGONS);
		DefinitionsManager.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);

		// Aux vars
		BezierCurve lookAtPath = this.transform.parent.FindComponentRecursive<BezierCurve>("LookAtPath");

		// Instantiate dragon prefabs
		for(int i = 0; i < defs.Count; i++) {
			// Get slot
			Transform slotTransform = transform.FindChild("DragonSlot" + i);
			if(slotTransform == null) continue;

			// Instantiate the prefab and add it as child of this object
			GameObject dragonPrefab = Resources.Load<GameObject>(defs[i].GetAsString("menuPrefab"));
			GameObject dragonObj = PrefabUtility.InstantiatePrefab(dragonPrefab) as GameObject;
			dragonObj.transform.SetParent(slotTransform, false);
			dragonObj.name = dragonPrefab.name;	// Remove the "(Clone)" text
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}