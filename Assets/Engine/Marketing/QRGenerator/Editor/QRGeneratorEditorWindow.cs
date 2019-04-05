// QRGeneratorTestEditor.cs
// 
// Created by Alger Ortín Castellví on 03/04/2019.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Editor tool to generate a QR code.
/// </summary>
public class QRGeneratorEditorWindow : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string TITLE = "QR Generator";
	private const float BUTTON_SIZE = 30;
	private const float PREVIEW_SIZE = 300f;
	private const float EDITOR_MIN_SIZE = 350f;
	private const float MARGIN = 10f;

	private const int MIN_SIZE = 10;
	private const int MAX_SIZE = 1000;

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Menu entry
	/// </summary>
	[MenuItem("Tools/QR Code Generator", false)]
	public static void MenuEntry() {
		QRGeneratorEditorWindow.ShowWindow();
	}

	// Windows instance
	public static QRGeneratorEditorWindow instance {
		get {
			return (QRGeneratorEditorWindow)EditorWindow.GetWindow(typeof(QRGeneratorEditorWindow), false, TITLE, true);
		}
	}

	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent(TITLE);
		instance.minSize = new Vector2(MARGIN + EDITOR_MIN_SIZE + MARGIN + PREVIEW_SIZE + MARGIN, 500f);
		instance.maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		// Show it
		instance.Show();
	}

	// Styles
	private GUIStyle s_wrappedTextStyle = null;
	private GUIStyle WRAPPED_TEXT_STYLE {
		get {
			if(s_wrappedTextStyle == null) {
				s_wrappedTextStyle = new GUIStyle(EditorStyles.textField);
				s_wrappedTextStyle.wordWrap = true;
			}
			return s_wrappedTextStyle;
		}
	}

	private GUIStyle s_qrTexPreviewStyle = null;
	private GUIStyle QR_TEX_PREVIEW_STYLE {
		get {
			if(s_qrTexPreviewStyle == null) {
				s_qrTexPreviewStyle = new GUIStyle();
			}
			return s_qrTexPreviewStyle;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Setup
	private const string DEBUG_MODE_KEY = "QRGeneratorEditorWindow.DebugMode";
	private bool debugMode {
		get { return EditorPrefs.GetBool(DEBUG_MODE_KEY, m_debugMode); }
		set { EditorPrefs.SetBool(DEBUG_MODE_KEY, value); }
	}
	private bool m_debugMode = false;

	private const string TEXT_KEY = "QRGeneratorEditorWindow.Text";
	private string text {
		get { return EditorPrefs.GetString(TEXT_KEY, m_text); }
		set { EditorPrefs.SetString(TEXT_KEY, value); }
	}
	private string m_text = "http://www.ubisoft.com";

	private const string FOREGROUND_COLOR_KEY = "QRGeneratorEditorWindow.ForegroundColor";
	private Color foregroundColor {
		get { return Colors.ParseHexString(EditorPrefs.GetString(FOREGROUND_COLOR_KEY, m_foregroundColor.ToHexString())); }
		set { EditorPrefs.SetString(FOREGROUND_COLOR_KEY, value.ToHexString()); }
	}
	private Color m_foregroundColor = Color.black;

	private const string BACKGROUND_COLOR_KEY = "QRGeneratorEditorWindow.BackgroundColor";
	private Color backgroundColor {
		get { return Colors.ParseHexString(EditorPrefs.GetString(BACKGROUND_COLOR_KEY, m_backgroundColor.ToHexString())); }
		set { EditorPrefs.SetString(BACKGROUND_COLOR_KEY, value.ToHexString()); }
	}
	private Color m_backgroundColor = Color.white;

	private const string SIZE_KEY = "QRGeneratorEditorWindow.Size";
	private int size {
		get { return EditorPrefs.GetInt(SIZE_KEY, m_size); }
		set { EditorPrefs.SetInt(SIZE_KEY, value); }
	}
	private int m_size = 300;

	private const string LOGO_SIZE_KEY = "QRGeneratorEditorWindow.LogoSize";
	private float logoSize {
		get { return EditorPrefs.GetFloat(LOGO_SIZE_KEY, m_logoSize); }
		set { EditorPrefs.SetFloat(LOGO_SIZE_KEY, value); }
	}
	private float m_logoSize = 0.25f;

	private const string ERROR_TOLERANCE_KEY = "QRGeneratorEditorWindow.ErrorTolerance";
	private QRGenerator.ErrorTolerance errorTolerance {
		get { return (QRGenerator.ErrorTolerance)EditorPrefs.GetInt(ERROR_TOLERANCE_KEY, (int)m_errorTolerance); }
		set { EditorPrefs.SetInt(ERROR_TOLERANCE_KEY, (int)value); }
	}
	private QRGenerator.ErrorTolerance m_errorTolerance = QRGenerator.ErrorTolerance.PERCENT_25;

	private const string LOGO_FILTER_MODE_KEY = "QRGeneratorEditorWindow.LogoFilterMode";
	private QRGenerator.LogoFilterMode logoFilterMode {
		get { return (QRGenerator.LogoFilterMode)EditorPrefs.GetInt(LOGO_FILTER_MODE_KEY, (int)m_logoFilterMode); }
		set { EditorPrefs.SetInt(LOGO_FILTER_MODE_KEY, (int)value); }
	}
	private QRGenerator.LogoFilterMode m_logoFilterMode = QRGenerator.LogoFilterMode.BILINEAR;

	// Vars
	private Texture2D m_qrTex = null;
	private Texture2D m_logoTex = null;	// [AOC] TODO!! Persist

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Load persistence
		m_debugMode = debugMode;
		QRGenerator.DEBUG_ENABLED = m_debugMode;
		m_text = text;
		m_foregroundColor = foregroundColor;
		m_backgroundColor = backgroundColor;
		m_size = size;
		m_logoSize = logoSize;
		m_errorTolerance = errorTolerance;
		m_logoFilterMode = logoFilterMode;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Detect changes
		EditorGUI.BeginChangeCheck();

		// Top margin
		GUILayout.Space(MARGIN);

		// Horizontal Layout
		EditorGUILayout.BeginHorizontal();
		{
			// Left margin
			GUILayout.Space(MARGIN);

			// Editor Panel
			EditorGUILayout.BeginVertical();
			{
				// Text
				m_text = EditorGUILayout.TextField("Text", m_text, WRAPPED_TEXT_STYLE, GUILayout.Height(60f));

				// Size
				EditorGUILayout.Space();
				m_size = EditorGUILayout.IntSlider("Size", m_size, MIN_SIZE, MAX_SIZE);

				// Colors
				m_foregroundColor = EditorGUILayout.ColorField("Foreground Color", m_foregroundColor);
				m_backgroundColor = EditorGUILayout.ColorField("Background Color", m_backgroundColor);

				// Logo
				EditorGUILayout.Space();
				m_logoTex = (Texture2D)EditorGUILayout.ObjectField("Logo", m_logoTex, typeof(Texture2D), false);

				float maxLogoSize = 0.30f;
				switch(m_errorTolerance) {
					case QRGenerator.ErrorTolerance.PERCENT_7: maxLogoSize = 0.07f; break;
					case QRGenerator.ErrorTolerance.PERCENT_15: maxLogoSize = 0.15f; break;
					case QRGenerator.ErrorTolerance.PERCENT_25: maxLogoSize = 0.25f; break;
					case QRGenerator.ErrorTolerance.PERCENT_30: maxLogoSize = 0.30f; break;
				}
				m_logoSize = EditorGUILayout.Slider("Logo Size (%)", m_logoSize, 0f, maxLogoSize);

				// Error tolerance
				m_errorTolerance = (QRGenerator.ErrorTolerance)EditorGUILayout.EnumPopup("Error Tolerance", m_errorTolerance);

				// Logo filtering
				m_logoFilterMode = (QRGenerator.LogoFilterMode)EditorGUILayout.EnumPopup("Logo Filter Mode", m_logoFilterMode);

				// Debug mode
				GUILayout.FlexibleSpace();
				m_debugMode = EditorGUILayout.Toggle("DEBUG MODE", m_debugMode);
			}
			EditorGUILayout.EndVertical();

			// Separation margin ------------------------------------------------------------------------
			GUILayout.Space(MARGIN);

			// Preview Panel
			EditorGUILayout.BeginVertical(GUILayout.Width(PREVIEW_SIZE));
			{
				// Preview Box
				QR_TEX_PREVIEW_STYLE.normal.background = m_qrTex == null ? Texture2D.blackTexture : m_qrTex; // Use placeholder if QR not yet generated;
				GUILayout.Box(
					GUIContent.none,
					QR_TEX_PREVIEW_STYLE,
					GUILayout.Width(PREVIEW_SIZE),
					GUILayout.Height(PREVIEW_SIZE)
				);

				// Preview info
				if(m_qrTex != null) {
					EditorGUILayout.LabelField("Size", m_qrTex.width + "x" + m_qrTex.height);
				}

				// Space
				GUILayout.Space(10f);

				// Buttons
				EditorGUILayout.BeginHorizontal(GUILayout.Height(BUTTON_SIZE));
				{
					// Generate button
					if(GUILayout.Button("GENERATE QR CODE", GUILayout.ExpandHeight(true))) {
						if(m_debugMode) Debug.ClearDeveloperConsole();
						m_qrTex = QRGenerator.GenerateQR(
							text, 
							size, 
							foregroundColor,
							backgroundColor, 
							QRGenerator.ErrorTolerance.PERCENT_25, 
							m_logoTex, 
							m_logoSize, 
							m_logoFilterMode
						);
					}

					// Save button
					EditorGUI.BeginDisabledGroup(m_qrTex == null);
					{
						if(GUILayout.Button("SAVE PNG", GUILayout.ExpandHeight(true))) {
							// Choose path
							string path = EditorUtility.SaveFilePanel(
								"Save - QR Code Generator",
								Application.dataPath,
								"qr",
								"png"
							);

							// Encode and write file to disk
							byte[] pngData = m_qrTex.EncodeToPNG();
							File.WriteAllBytes(path, pngData);
						}
					} EditorGUI.EndDisabledGroup();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			// Right margin
			GUILayout.Space(MARGIN);
		}
		EditorGUILayout.EndHorizontal();

		// Bottom margin
		GUILayout.Space(MARGIN);

		// If something has changed, save persistence
		if(EditorGUI.EndChangeCheck()) {
			debugMode = m_debugMode;
			text = m_text;
			foregroundColor = m_foregroundColor;
			backgroundColor = m_backgroundColor;
			size = m_size;
			logoSize = m_logoSize;
			errorTolerance = m_errorTolerance;
			logoFilterMode = m_logoFilterMode;
			QRGenerator.DEBUG_ENABLED = m_debugMode;
		}
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {

	}

	/// <summary>
	/// Called multiple times per second on all visible windows.
	/// </summary>
	public void Update() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}