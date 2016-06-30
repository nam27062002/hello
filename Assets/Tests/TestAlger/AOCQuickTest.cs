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
using TMPro;

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
	public GameObject m_currentObj = null;
	public int m_currentFont = 0;

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
		// Select a new font
		string[] fonts = {
			"FNT_Default",
			"FNT_JA",
			"FNT_RU",
			"FNT_KO",
			"FNT_ZH",
			"FNT_Bold"
		};
		m_currentFont = (m_currentFont + 1)%fonts.Length;
		string newFontName = fonts[m_currentFont];
		string newFontPath = "UI/Fonts/" + newFontName + "/" + newFontName;	// i.e. "UI/Fonts/FNT_Default/FNT_Default"

		// Load the new font asset
		TMP_FontAsset newFont = Resources.Load<TMP_FontAsset>(newFontPath);
		if(newFont == null) return;

		// For each text, set the new font
		TextMeshProUGUI[] texts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
		for(int i = 0; i < texts.Length; i++) {
			// Try to match the equivalent material for the new font
			string matName = texts[i].fontSharedMaterial.name.Replace(texts[i].font.name, "");	// i.e. "_BlackStroke"
			Material newMat = Resources.Load<Material>(newFontPath + matName);	// i.e. "UI/Fonts/FNT_Default/FNT_Default_BlackStroke"

			// Replace the new font
			texts[i].font = newFont;

			// If found, apply the new material preset
			if(newMat != null) {
				texts[i].fontSharedMaterial = newMat;
			}
		}

		Resources.UnloadUnusedAssets();
	}

	public void LoadPrefab(string _path) {
		string current = "";
		if(m_currentObj != null) {
			current = m_currentObj.name;
			GameObject.Destroy(m_currentObj);
			m_currentObj = null;
		}

		if(current == _path) return;

		GameObject prefab = Resources.Load<GameObject>(_path);
		if(prefab == null) return;

		m_currentObj = GameObject.Instantiate<GameObject>(prefab);
		m_currentObj.name = _path;
		m_currentObj.transform.SetParent(this.transform.parent, false);
		m_currentObj.transform.SetSiblingIndex(0);
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}