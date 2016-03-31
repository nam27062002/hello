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
	[Space]
	public DefinitionsCategory m_category = DefinitionsCategory.UNKNOWN;
	public string m_sku = "";
	public string m_propertyId = "sku";
	[Space]
	public string m_stringValue = "";
	public float m_floatValue = 0f;
	public int m_intValue = 0;
	public double m_doubleValue = 0;
	public long m_longValue = 0;

	// Tooltip test
	private int m_tooltipOpenCount = 0;

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
		if(m_category == DefinitionsCategory.UNKNOWN) {
			Debug.LogError("Please select a valid category!");
			return;
		}

		DefinitionNode def = DefinitionsManager.GetDefinition(m_category, m_sku);
		if(def == null) {
			string str = "Def not found, candidate skus are: ";
			List<string> skus = DefinitionsManager.GetSkuList(m_category);
			for(int i = 0; i < skus.Count; i++) str += "\n" + skus[i];
			Debug.LogError(str);
			return;
		}

		if(!def.Has(m_propertyId)) {
			string str = "Property not found, candidate are: ";
			List<string> properties = def.GetPropertyList();
			for(int i = 0; i < properties.Count; i++) str += "\n" + properties[i];
			Debug.LogError(str);
			return;
		}

		m_stringValue = def.Get<string>(m_propertyId);
		m_floatValue = def.Get<float>(m_propertyId);
		m_intValue = def.Get<int>(m_propertyId);
		m_doubleValue = def.Get<double>(m_propertyId);
		m_longValue = def.Get<long>(m_propertyId);
	}

	/// <summary>
	/// A tooltip is being opened
	/// </summary>
	/// <param name="_tooltip">The tooltip that is being opened.</param>
	public void OnTooltipOpen(UITooltip _tooltip) {
		m_tooltipOpenCount++;

		Text txt = _tooltip.GetComponentInChildren<Text>();
		if(txt != null) txt.text = "The tooltip has been opened " + m_tooltipOpenCount + " times!";
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}