// RulesReader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple editor window to check rule values.
/// </summary>
public class RulesReaderEditorWindow : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Windows instance
	private static RulesReaderEditorWindow m_instance = null;
	public static RulesReaderEditorWindow instance {
		get {
			if(m_instance == null) {
				m_instance = (RulesReaderEditorWindow)EditorWindow.GetWindow(typeof(RulesReaderEditorWindow));
			}
			return m_instance;
		}
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private DefinitionsCategory m_category = DefinitionsCategory.UNKNOWN;
	private string m_sku = "";
	private string m_propertyId = "sku";

	private string m_stringValue = "";
	private float m_floatValue = 0f;
	private int m_intValue = 0;
	private double m_doubleValue = 0;
	private long m_longValue = 0;
	private bool m_boolValue = false;

	private Vector2 m_scrollPos = Vector2.zero;
	private Vector2 m_defPreviewScrollPos = Vector2.zero;
	private bool m_defPreviewExpanded = true;
	private bool m_propertyPreviewExpanded = true;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent("Rules Reader");
		instance.minSize = new Vector2(300, 100);	// Arbitrary
		instance.maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);		// Fixed width, limitless

		// Show it
		instance.ShowTab();
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		EditorGUILayout.Space();
		m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); {
			// Category selector
			bool allValid = true;
			m_category = (DefinitionsCategory)EditorGUILayout.EnumPopup("category", m_category);

			// Category validation
			if(m_category == DefinitionsCategory.UNKNOWN) {
				EditorGUILayout.HelpBox("Please select a valid category!", MessageType.Error);
				allValid = false;
			}

			// Sku selector
			if(allValid) {
				List<string> skus = DefinitionsManager.GetSkuList(m_category);
				int skuIdx = Mathf.Max(skus.IndexOf(m_sku), 0);	// If not found, first option
				skuIdx = EditorGUILayout.Popup("sku", skuIdx, skus.ToArray());

				// Validation
				if(skuIdx < 0 || skuIdx >= skus.Count) {
					EditorGUILayout.HelpBox("Please select a valid sku!", MessageType.Error);
					allValid = false;
				} else {
					m_sku = skus[skuIdx];
				}
			}

			// Get target definition
			DefinitionNode def = DefinitionsManager.GetDefinition(m_category, m_sku);

			// Def preview
			if(allValid) {
				m_defPreviewExpanded = EditorGUILayout.Foldout(m_defPreviewExpanded, "Def Preview");
				if(m_defPreviewExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Draw definition as a string
					GUIContent defContent = new GUIContent(def.ToString());
					Vector2 defSize = EditorStyles.label.CalcSize(defContent);
					m_defPreviewScrollPos = EditorGUILayout.BeginScrollView(m_defPreviewScrollPos, EditorStyles.helpBox, GUILayout.Height(Mathf.Min(defSize.y + 20f, 150f)), GUILayout.ExpandWidth(true)); {
						GUILayout.Label(defContent);
					} EditorGUILayoutExt.EndScrollViewSafe();

					// Indent out
					EditorGUI.indentLevel--;
				}
			}

			// Property preview
			if(allValid) {
				m_propertyPreviewExpanded = EditorGUILayout.Foldout(m_propertyPreviewExpanded, "Property Preview");
				if(m_propertyPreviewExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Property selector
					List<string> properties = def.GetPropertyList();
					int propertyIdx = Mathf.Max(properties.IndexOf(m_propertyId), 0);	// If not found, first option
					propertyIdx = EditorGUILayout.Popup("property", propertyIdx, properties.ToArray());

					// Validation
					if(propertyIdx < 0 || propertyIdx >= properties.Count) {
						EditorGUILayout.HelpBox("Please select a valid property!", MessageType.Error);
						allValid = false;
					} else {
						m_propertyId = properties[propertyIdx];
					}

					// Draw the property in all its representations
					if(allValid) {
						EditorGUILayout.Space();
						GUI.enabled = false;
						EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
							m_stringValue = def.Get<string>(m_propertyId);
							EditorGUILayout.LabelField("as string", m_stringValue);

							m_floatValue = def.Get<float>(m_propertyId);
							EditorGUILayout.FloatField("as float", m_floatValue);

							m_intValue = def.Get<int>(m_propertyId);
							EditorGUILayout.IntField("as int", m_intValue);

							m_doubleValue = def.Get<double>(m_propertyId);
							EditorGUILayout.DoubleField("as double", m_doubleValue);

							m_longValue = def.Get<long>(m_propertyId);
							EditorGUILayout.LongField("as long", m_longValue);

							m_boolValue = def.Get<bool>(m_propertyId);
							EditorGUILayout.Toggle("as bool", m_boolValue);
						} EditorGUILayoutExt.EndVerticalSafe();
						GUI.enabled = true;
					}

					// Indent out
					EditorGUI.indentLevel--;
				}
			}
		} EditorGUILayoutExt.EndScrollViewSafe();
	}
}