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
//[ExecuteInEditMode]
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
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.Append("Daily Chests: ");
		sb.Append(ChestManager.collectedChests.ToString());
		sb.Append("/");
		sb.Append(ChestManager.NUM_DAILY_CHESTS.ToString());
		sb.AppendLine();
		sb.Append("Time Remaining: ");
		sb.Append(TimeUtils.FormatTime(ChestManager.timeToReset.TotalSeconds, TimeUtils.EFormat.DIGITS, 3));
		GetComponent<Text>().text = sb.ToString();
	}

	public void OnRestart() {
		Debug.Log("RESTART");
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		TabSystem ts = GetComponent<TabSystem>();
		if(ts != null) {
			ts.SetTabEnabled(2, !ts.GetTab(2).tabEnabled);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}