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
using System.Collections;
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
	[SerializeField] private UIScene3DLoader m_nextDragonScene3DLoader = null;

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
		// Load and pose the dragon - will override any existing dragon
		MenuDragonLoader dragonLoader = m_nextDragonScene3DLoader.scene.FindComponentRecursive<MenuDragonLoader>();
		if(dragonLoader != null) {
			dragonLoader.LoadDragon("dragon_crocodile");
			dragonLoader.FindComponentRecursive<Animator>().SetTrigger("idle");
		}
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	IEnumerator DelayedDragonLoad() {
		yield return new WaitForSeconds(5);
		// Load and pose the dragon - will override any existing dragon
		MenuDragonLoader dragonLoader = m_nextDragonScene3DLoader.scene.FindComponentRecursive<MenuDragonLoader>();
		if(dragonLoader != null) {
			dragonLoader.LoadDragon("dragon_crocodile");
			dragonLoader.FindComponentRecursive<Animator>().SetTrigger("idle");
		}
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		// Load and pose the dragon - will override any existing dragon
		MenuDragonLoader dragonLoader = m_nextDragonScene3DLoader.scene.FindComponentRecursive<MenuDragonLoader>();
		if(dragonLoader != null) {
			dragonLoader.LoadDragon("dragon_crocodile");
			dragonLoader.FindComponentRecursive<Animator>().SetTrigger("idle");
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}