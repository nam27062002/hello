// GenerateMaterialEditorWindow.cs
// 
// Created by Alger Ortín Castellví on 30/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Custom editor windows to simplify Hungry Dragon's level design.
	/// </summary>
	public class GenerateMaterialEditorWindow : EditorWindow {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private List<Color> m_colors = new List<Color>();
		private List<Texture2D> m_textures = new List<Texture2D>();

		private float m_whiteTintFactor = 0.7f;
		private float m_blackTintFactor = 0.4f;

		//------------------------------------------------------------------//
		// STATIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Add menu item to be able to open the editor.
		/// </summary>
		//[MenuItem("Hungry Dragon/Color Materials Generator")]
		public static void ShowWindow() {
			// Show existing window instance. If one doesn't exist, make one.
			GenerateMaterialEditorWindow window = (GenerateMaterialEditorWindow)EditorWindow.GetWindow(typeof(GenerateMaterialEditorWindow));
			
			// Setup window
			window.titleContent = new GUIContent("Color Materials Generator");
			window.minSize = new Vector2(330f, 350f);	// Min required width to properly fit all the content

			// Make sure everything is initialized properly
			window.GenerateColors();
			
			// Show it
			window.Show();
		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		private void GenerateColors() {
			Color[] baseColors = new Color[] {
				Colors.red,
				Colors.orange,
				Colors.yellow,
				Colors.lime,
				Colors.cyan,
				Colors.skyBlue,
				Colors.blue,
				Colors.magenta,
				Colors.pink
			};
			
			float d1 = 0.5f;
			float d2 = 0.66f;
			List<Color> colorIt1 = new List<Color>();
			for(int i = 0; i < baseColors.Length; i++) {
				int j = (i+1)%baseColors.Length;	// 0 for last color
				
				colorIt1.Add(baseColors[i]);
				//colorIt1.Add(Color.Lerp(baseColors[i], baseColors[j], d1));
				//colorIt1.Add(Color.Lerp(baseColors[i], baseColors[j], d2));
			}
			
			float f1 = m_whiteTintFactor;
			float f2 = m_blackTintFactor;
			m_colors = new List<Color>();
			m_colors.Add(Colors.white);
			m_colors.Add(Colors.gray);
			m_colors.Add(Colors.black);
			for(int i = 0; i < colorIt1.Count; i++) {
				m_colors.Add(Color.Lerp(colorIt1[i], Colors.white, f1));
				m_colors.Add(colorIt1[i]);
				m_colors.Add(Color.Lerp(colorIt1[i], Colors.black, f2));
			}

			// Generate textures for preview
			m_textures = new List<Texture2D>();
			for(int i = 0; i < m_colors.Count; i++) {
				m_textures.Add(Texture2DExt.Create(2, 2, m_colors[i]));
			}
		}

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// The window has been enabled - similar to the constructor.
		/// </summary>
		public void OnEnable() {

		}

		/// <summary>
		/// The window has been disabled - similar to the destructor.
		/// </summary>
		public void OnDisable() {

		}

		/// <summary>
		/// Called 100 times per second on all visible windows.
		/// </summary>
		public void Update() {

		}

		/// <summary>
		/// OnDestroy is called when the EditorWindow is closed.
		/// </summary>
		public void OnDestroy() {

		}

		//------------------------------------------------------------------//
		// WINDOW METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Update the inspector window.
		/// </summary>
		public void OnGUI() {
			GUILayout.Space(10);

			float newWhiteFactor = EditorGUILayout.Slider("White Tint Factor", m_whiteTintFactor, 0f, 1f);
			float newBlackFactor = EditorGUILayout.Slider("Black Tint Factor", m_blackTintFactor, 0f, 1f);
			newWhiteFactor = MathUtils.Snap(newWhiteFactor, 0.1f);
			newBlackFactor = MathUtils.Snap(newBlackFactor, 0.1f);
			if(newWhiteFactor != m_whiteTintFactor || newBlackFactor != m_blackTintFactor) {
				m_whiteTintFactor = newWhiteFactor;
				m_blackTintFactor = newBlackFactor;
				GenerateColors();
			}

			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(); {
				GUILayout.FlexibleSpace();
				GUIStyle style = new GUIStyle();
				int numRows = 3;
				for(int i = 0; i < m_colors.Count; i++) {
					if(i%numRows == 0) {
						EditorGUILayout.BeginVertical();
					}
					
					style.normal.background = m_textures[i];
					GUILayout.Box("", style, GUILayout.Width(50), GUILayout.Height(50));
					
					if(i%numRows == numRows-1) {
						EditorUtils.EndVerticalSafe();
						if(i != m_colors.Count - 1) GUILayout.Space(5);
					}
				}
				GUILayout.FlexibleSpace();
			} EditorUtils.EndHorizontalSafe();

			GUILayout.Space(10);
			
			if(GUILayout.Button("GENERATE MATERIALS")) {
				//ShowNotification(new GUIContent("TODO!!"));
				string path = "Assets/Tools/LevelEditor/Materials/";
				for(int i = 0; i < m_colors.Count; i++) {
					Material mat = new Material(Shader.Find("Standard"));
					mat.color = m_colors[i];
					AssetDatabase.CreateAsset(mat, path + "MT_LevelEditor_" + i + ".mat");
				}
			}
		}
	}
}