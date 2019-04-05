// GDPRSettingsEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the GDPRSettings.CountrySetupDictionary class.
/// </summary>
[CustomPropertyDrawer(typeof(GDPRSettings.CountrySetupDictionary), true)]
public class CountrySetupDictionaryEditor : SerializableDictionaryEditor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Field ids
	public const string HAS_AGE_PROPERTY = "hasAgeRestriction";
	public const string AGE_PROPERTY = "ageRestriction";
	public const string CONSENT_PROPERTY = "requiresConsent";
	public const string GROUP_PROPERTY = "group";

	// Field labels
	public const string COUNTRY_LABEL = "Country";
	public const string HAS_AGE_LABEL = "Has\nAge";
	public const string AGE_LABEL = "Age";
	public const string CONSENT_LABEL = "Requires\nConsent";
	public const string GROUP_LABEL = "Group";

	// Field widths
	public static float SPACE_WIDTH = 10f;

	private static float s_countryCodeWidth = -1f;
	public static float COUNTRY_CODE_WIDTH {
		get { 
			if(s_countryCodeWidth < 0f) {
				float headerWidth = EditorStyles.label.CalcSize(new GUIContent(COUNTRY_LABEL)).x;
				float valueWidth = EditorStyles.label.CalcSize(new GUIContent("GB")).x;
				s_countryCodeWidth = Mathf.Max(headerWidth, valueWidth);
			}
			return s_countryCodeWidth;
		}
	}

	private static float s_hasAgeWidth = -1f;
	public static float HAS_AGE_WIDTH {
		get {
			if(s_hasAgeWidth < 0f) {
				float headerWidth = EditorStyles.label.CalcSize(new GUIContent(HAS_AGE_LABEL)).x;
				float valueWidth = 20f; // Approx the width of a checkbox
				s_hasAgeWidth = Mathf.Max(headerWidth, valueWidth);
			}
			return s_hasAgeWidth;
		}
	}

	private static float s_ageWidth = -1f;
	public static float AGE_WIDTH {
		get {
			if(s_ageWidth < 0f) {
				float headerWidth = EditorStyles.label.CalcSize(new GUIContent(AGE_LABEL)).x;
				float valueWidth = EditorStyles.label.CalcSize(new GUIContent("100")).x;	// Enough width for a 0-100 age value
				s_ageWidth = Mathf.Max(headerWidth, valueWidth);
			}
			return s_ageWidth;
		}
	}

	private static float s_consentWidth = -1f;
	public static float CONSENT_WIDTH {
		get {
			if(s_consentWidth < 0f) {
				float headerWidth = EditorStyles.label.CalcSize(new GUIContent(CONSENT_LABEL)).x;
				float valueWidth = 20f;	// Approx the width of a checkbox
				s_consentWidth = Mathf.Max(headerWidth, valueWidth);
			}
			return s_consentWidth;
		}
	}

	private static float s_countryGroupWidth = -1f;
	public static float COUNTRY_GROUP_WIDTH {
		get {
			if(s_countryGroupWidth < 0f) {
				float headerWidth = EditorStyles.label.CalcSize(new GUIContent(GROUP_LABEL)).x;
				float valueWidth = 100f; // Enough width for a enum popup
				s_countryGroupWidth = Mathf.Max(headerWidth, valueWidth);
			}
			return s_countryGroupWidth;
		}
	}

	// Haeader Label GUI style
	private static GUIStyle s_headerLabelStyle = null;
	public static GUIStyle headerLabelStyle {
		get {
			if(s_headerLabelStyle == null) {
				s_headerLabelStyle = new GUIStyle(GUI.skin.label);
				s_headerLabelStyle.alignment = TextAnchor.LowerLeft;
				s_headerLabelStyle.normal.textColor = Colors.gray;
				s_headerLabelStyle.fontStyle = FontStyle.Italic;
			}
			return s_headerLabelStyle;
		}
	}

	//------------------------------------------------------------------//
	// PARENT OVERRIDES													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Optionally draw a header line giving extra info on the dictionary's content.
	/// Will only be displayed when expanded.
	/// </summary>
	protected override void DrawHeader() {
		// Draw a table header showing what each column is
		Rect pos = new Rect(m_pos);
		pos.height = pos.height * 2f;

		// Backup some values
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Country Code
		pos.width = COUNTRY_CODE_WIDTH;
		EditorGUI.LabelField(pos, COUNTRY_LABEL, headerLabelStyle);
		pos.x += pos.width;

		// Space
		pos.x += SPACE_WIDTH;

		// Has Age Restriction?
		pos.width = HAS_AGE_WIDTH;
		EditorGUI.LabelField(pos, HAS_AGE_LABEL, headerLabelStyle);
		pos.x += pos.width;

		// Age Restriction
		pos.width = AGE_WIDTH;
		EditorGUI.LabelField(pos, AGE_LABEL, headerLabelStyle);
		pos.x += pos.width;

		// Space
		pos.x += SPACE_WIDTH;

		// Requires Consent
		pos.width = CONSENT_WIDTH;
		EditorGUI.LabelField(pos, CONSENT_LABEL, headerLabelStyle);
		pos.x += pos.width;

		// Space
		pos.x += SPACE_WIDTH;

		// Country Group
		float remainingWidth = m_pos.width - pos.x;
		pos.width = Mathf.Max(COUNTRY_GROUP_WIDTH, remainingWidth);
		EditorGUI.LabelField(pos, GROUP_LABEL, headerLabelStyle);
		pos.x += pos.width;

		// Restore some stuff
		EditorGUI.indentLevel = indentLevelBackup;

		// Next line
		AdvancePos(pos.height);
	}

	/// <summary>
	/// Get the width for the key field.
	/// </summary>
	/// <returns>The width for the key field.</returns>
	/// <param name="_availableWidth">Total available width for the property content.</param>
	protected override float GetKeyFieldWidth(float _availableWidth) {
		return COUNTRY_CODE_WIDTH;
	}

	/// <summary>
	/// Reset the values of a given dictionary element.
	/// Called when adding a new entry to the dictionary for example.
	/// </summary>
	/// <param name="_p">Property to be reset.</param>
	protected override void ResetElementValues(SerializedProperty _p) {
		// Reset to default values
		_p.FindPropertyRelative(HAS_AGE_PROPERTY).boolValue = false;
		_p.FindPropertyRelative(AGE_PROPERTY).intValue = -1;
		_p.FindPropertyRelative(CONSENT_PROPERTY).boolValue = false;
		_p.FindPropertyRelative(GROUP_PROPERTY).enumValueIndex = 0;
	}
}

/// <summary>
/// Custom editor for the GDPRSettings.CountrySetup class.
/// </summary>
[CustomPropertyDrawer(typeof(GDPRSettings.CountrySetup), true)]
public class CountrySetupEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	protected override void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Aux vars
		SerializedProperty p = null;
		m_pos.height = EditorStyles.numberField.lineHeight + EditorStyles.numberField.margin.vertical;
		Rect pos = new Rect(m_pos);

		// Backup some values
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Space
		pos.x += CountrySetupDictionaryEditor.SPACE_WIDTH;

		// Has Age Restriction?
		pos.width = CountrySetupDictionaryEditor.HAS_AGE_WIDTH;
		p = _property.FindPropertyRelative(CountrySetupDictionaryEditor.HAS_AGE_PROPERTY);
		EditorGUI.PropertyField(pos, p, GUIContent.none);
		pos.x += pos.width;

		// Age Restriction
		// Disable and set default value if age restriction not enabled
		bool hasAgeRequirement = p.boolValue;
		EditorGUI.BeginDisabledGroup(!hasAgeRequirement); {
			pos.width = CountrySetupDictionaryEditor.AGE_WIDTH;
			p = _property.FindPropertyRelative(CountrySetupDictionaryEditor.AGE_PROPERTY);
			if(!hasAgeRequirement && p.intValue != -1) {
				p.intValue = -1;
			}
			EditorGUI.PropertyField(pos, p, GUIContent.none);
			pos.x += pos.width;
		} EditorGUI.EndDisabledGroup();

		// Space
		pos.x += CountrySetupDictionaryEditor.SPACE_WIDTH;

		// Requires Consent
		pos.width = CountrySetupDictionaryEditor.CONSENT_WIDTH;
		p = _property.FindPropertyRelative(CountrySetupDictionaryEditor.CONSENT_PROPERTY);
		EditorGUI.PropertyField(pos, p, GUIContent.none);
		pos.x += pos.width;

		// Space
		pos.x += CountrySetupDictionaryEditor.SPACE_WIDTH;

		// Country Group
		// [AOC] Idiot Unity doesn't offer a editor tool to display flag enums from a SerializedProperty
		//		 We must un-serialize it first in order to use the EditorGUI.EnumFlagsField() method -_-
		float remainingWidth = m_pos.xMax - pos.x;	// Fill remaining space :)
		pos.width = Mathf.Max(CountrySetupDictionaryEditor.COUNTRY_GROUP_WIDTH, remainingWidth);
		p = _property.FindPropertyRelative(CountrySetupDictionaryEditor.GROUP_PROPERTY);
		GDPRSettings.CountryGroup oldGroup = (GDPRSettings.CountryGroup)p.intValue;
		GDPRSettings.CountryGroup newGroup = (GDPRSettings.CountryGroup)EditorGUI.EnumFlagsField(pos, GUIContent.none, oldGroup);
		p.intValue = (int)newGroup;
		pos.x += pos.width;

		// Restore some stuff
		EditorGUI.indentLevel = indentLevelBackup;

		// Done!
		AdvancePos();
	}
}