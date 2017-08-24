// CategorizedEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extended Editor to easily categorize properties.
/// </summary>
//[CustomEditor(typeof(MonoBehaviourTemplate), true)]	// True to be used by heir classes as well
//[CanEditMultipleObjects]
public abstract class CategorizedEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar class to better organize properties.
	/// </summary>
	protected class Category {
		public string name = "";
		public List<SerializedProperty> properties = new List<SerializedProperty>();
		public Dictionary<string, Category> subCategories = new Dictionary<string, Category>();

		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		/// <param name="_name">Category name.</param>
		public Category(string _name) {
			name = _name;
		}

		/// <summary>
		/// Assign a property to this category.
		/// </summary>
		/// <param name="_p">Property to be assigned.</param>
		public void Add(SerializedProperty _p) {
			properties.Add(_p.Copy());	// [AOC] Copy, since _p may come from an iterator and we want the reference to the property, not the iterator!
		}

		/// <summary>
		/// Assign a property to a subcategory of this category.
		/// </summary>
		/// <param name="_p">Property to be assigned.</param>
		/// <param name="_subCategory">Sub category.</param>
		public void Add(SerializedProperty _p, string _subCategory) {
			// Does sub-category exist?
			if(!subCategories.ContainsKey(_subCategory)) {
				// No, do it!
				subCategories[_subCategory] = new Category(_subCategory);
			}

			subCategories[_subCategory].Add(_p);
		}

		/// <summary>
		/// Does the category actually have any property assigned?
		/// </summary>
		/// <returns>Whether the category is empty (doesn't have any property in any of its subcategories).</returns>
		public bool IsEmpty() {
			// If we have at least one property, we're not empty!
			if(properties.Count > 0) return false;

			// Check subcategories
			foreach(KeyValuePair<string, Category> kvp in subCategories) {
				// Category not empty, return
				if(!kvp.Value.IsEmpty()) return false;
			}

			// All empty!
			return true;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Properties by category
	protected Dictionary<string, Category> m_categories = new Dictionary<string, Category>();

	// Styles
	private static GUIStyle m_foldoutStyle = null;
	private static GUIStyle foldoutStyle {
		get {
			if(m_foldoutStyle == null) {
				// [AOC] We will be drawing a box actually, so copy some values from the box style
				m_foldoutStyle = new GUIStyle(EditorStyles.foldout);
				m_foldoutStyle.fontStyle = FontStyle.Italic;
				m_foldoutStyle.normal.textColor = Colors.gray;
				m_foldoutStyle.onNormal.textColor = m_foldoutStyle.normal.textColor;
			}
			return m_foldoutStyle;
		}
	}

	private static GUIStyle m_lineStyle = null;
	private static GUIStyle lineStyle {
		get {
			if(m_lineStyle == null) {
				// [AOC] We will be drawing a box actually, so copy some values from the box style
				m_lineStyle = new GUIStyle();
				m_lineStyle.normal.background = Texture2DExt.Create(foldoutStyle.normal.textColor);
				m_lineStyle.margin = EditorStyles.helpBox.margin;
				m_lineStyle.padding = EditorStyles.helpBox.padding;
			}
			return m_lineStyle;
		}
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	protected virtual void OnEnable() {
		// Clear and intialize categories dictionary
		m_categories.Clear();
		InitCategories();
	}

	/// <summary>
	/// Do the OnGUI() call for the target category.
	/// </summary>
	/// <param name="_category">Category to be displayed.</param>
	protected virtual void DoCategory(string _category) {
		// Make sure category is valid and not empty
		Category cat = null;
		if(!m_categories.TryGetValue(_category, out cat)) return;
		if(cat.IsEmpty()) return;

		// Do it!
		DoCategory(cat, true);
	}

	/// <summary>
	/// Do the OnGUI() call for the target category.
	/// </summary>
	/// <param name="_cat">Category to be displayed.</param>
	/// <param name="_isSubcategory">Whether this is a subcategory or a main category. Affects the visual style.</param>
	protected virtual void DoCategory(Category _cat, bool _isMainCategory) {
		// Add spacing before (only for main categories)
		if(_isMainCategory) {
			GUILayout.Space(10f);
		}

		// Figure out style to be used for the foldout
		GUIStyle style = _isMainCategory ? foldoutStyle : EditorStyles.foldout;

		// Compute some aux vars for the foldout design
		GUIContent textContent = new GUIContent(_cat.name);
		Vector2 textSize = style.CalcSize(textContent);
		float height = Mathf.Max(textSize.y, 20f);	// Minimum size 20f (should be enough)

		// Load foldout state from prefs
		bool expanded = EditorPrefs.GetBool(this.GetType().Name + "." + _cat.name, false);		// Collapsed by default

		// Do category foldout
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(height)); {
			// Draw foldout
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.Width(textSize.x)); {
				float spaceSize = (height - textSize.y)/2f;	// Vertically centered
				GUILayout.Space(spaceSize);
				expanded = EditorGUILayout.Foldout(expanded, textContent, true, style);
				GUILayout.Space(spaceSize);
			} EditorGUILayout.EndVertical();

			// Draw line after the title (only for main categories)
			if(_isMainCategory) {
				// We need to reset indentation so we get the line right next to the foldout title
				int indentBackup = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				// Do it!
				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true)); {
					float lineThickness = 1f;
					float spaceSize = (height - lineThickness)/2f;	// Vertically centered
					GUILayout.Space(spaceSize);
					GUILayout.Box(GUIContent.none, lineStyle, GUILayout.Height(lineThickness));
					GUILayout.Space(spaceSize);
				} EditorGUILayout.EndVertical();

				// Restore indentation
				EditorGUI.indentLevel = indentBackup;
			}
		} EditorGUILayout.EndHorizontal();

		// Store foldout state to prefs
		EditorPrefs.SetBool(this.GetType().Name + "." + _cat.name, expanded);

		// Do properties and subcategories
		if(expanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Do properties!
			for(int i = 0; i < _cat.properties.Count; i++) {
				DoProperty(_cat.properties[i]);
			}

			// Do subcategories
			foreach(KeyValuePair<string, Category> kvp in _cat.subCategories) {
				// Skip if empty!
				if(kvp.Value.IsEmpty()) continue;
				DoCategory(kvp.Value, false);
			}

			// Indent out
			EditorGUI.indentLevel--;
		}
	}

	/// <summary>
	/// Do the OnGUI() call for the target property.
	/// Override to treat some properties differently.
	/// </summary>
	/// <param name="_p">Property to be displayed.</param>
	protected virtual void DoProperty(SerializedProperty _p) {
		// Default property display
		EditorGUILayout.PropertyField(_p, true);
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the categories by adding them to the categories dictionary.
	/// </summary>
	protected abstract void InitCategories();
}