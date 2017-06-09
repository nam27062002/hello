// EasePreviewTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/02/2017.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Standalone tool to preview all ease types
/// </summary>
public class EasePreviewTool : EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// STATICS																  //
	//------------------------------------------------------------------------//
	// Windows instance
	public static EasePreviewTool instance {
		get {
			return (EasePreviewTool)EditorWindow.GetWindow(typeof(EasePreviewTool), false, "Ease Preview Tool", true);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Cache preview curves
	protected Dictionary<Ease, AnimationCurve> m_curves = new Dictionary<Ease, AnimationCurve>();

	// Internal
	private Vector2 m_listScrollPos = Vector2.zero;
	private Vector2 m_tweensScrollPos = Vector2.zero;
	private float m_lastUpdateTime = 0f;

	// Setup
	private int CURVE_SAMPLES = 10;
	private float PREVIEW_HEIGHT = 100f;

	// Styles
	private GUIStyle m_boxStyle = null;
	private Texture m_previewTexture = null;

	// Live preview
	private LivePreviewTween m_moveTween = new LivePreviewTween("MOVE");
	private LivePreviewTween m_scaleTween = new LivePreviewTween("SCALE");

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the window.
	/// </summary>
	public static void ShowWindow() {
		// Setup window
		instance.titleContent = new GUIContent("Ease Preview Tool");
		instance.minSize = new Vector2(630f, 100f);
		instance.maxSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		// Reset timer
		instance.m_lastUpdateTime = Time.realtimeSinceStartup;

		// Show it
		instance.Show();
	}

	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Load stored data
		m_moveTween.Load();
		m_scaleTween.Load();

		// Restart tweens so they are synced
		m_moveTween.Restart();
		m_scaleTween.Restart();

		// We want to know if a tween is changed
		m_moveTween.OnChange.AddListener(OnTweenChanged);
		m_scaleTween.OnChange.AddListener(OnTweenChanged);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	/// <summary>
	/// Creates custom GUI styles if not already done.
	/// Must only be called from the OnGUI() method.
	/// </summary>
	private void InitStyles() {
		// Box style
		if(m_boxStyle == null) {
			m_boxStyle = new GUIStyle(GUI.skin.box);
			m_boxStyle.normal.background = EditorUtils.CreateTexture(Colors.pink);
		}

		// Preview texture
		if(m_previewTexture == null) {
			m_previewTexture = EditorUtils.CreateTexture(Colors.orange);
		}
	}

	/// <summary>
	/// Update the inspector window.
	/// </summary>
	public void OnGUI() {
		// Make sure styles are initialized - must be done in the OnGUI call
		InitStyles();

		// 2 Columns layout: left for setup and eases list, right for live preview
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal(); {
			// Column 1
			EditorGUILayout.BeginVertical(EditorStyles.helpBox); {
				DoSetup();
				EditorGUILayout.Space();
				DoList();
			}; EditorGUILayout.EndVertical();

			// Column 2
			EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f)); {
				DoLivePreview();
			}; EditorGUILayout.EndVertical();
		} EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		// Force repainting to update with current selection
		//Repaint();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	public void Update() {
		// Update time
		float deltaTime = Time.realtimeSinceStartup - m_lastUpdateTime;
		m_lastUpdateTime = Time.realtimeSinceStartup;

		// Update tweens
		m_moveTween.Update(deltaTime);
		m_scaleTween.Update(deltaTime);

		// Force a repaint
		Repaint();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do the setup layout. To be called from the OnGUI() call.
	/// </summary>
	private void DoSetup() {
		// Curve samples
		GUI.changed = false;
		CURVE_SAMPLES = EditorPrefs.GetInt("EasePreviewTool.CURVE_SAMPLES", 10);
		int newCurveSamples = EditorGUILayout.IntField("Curve Samples", CURVE_SAMPLES);
		if(GUI.changed) {
			CURVE_SAMPLES = Mathf.Max(2, newCurveSamples);	// At least 2!
			EditorPrefs.SetInt("EasePreviewTool.CURVE_SAMPLES", CURVE_SAMPLES);
		}

		// Preview height
		GUI.changed = false;
		PREVIEW_HEIGHT = EditorPrefs.GetFloat("EasePreviewTool.PREVIEW_HEIGHT", 50f);
		float newPreviewHeight = EditorGUILayout.FloatField("Preview Height", PREVIEW_HEIGHT);
		if(GUI.changed) {
			PREVIEW_HEIGHT = Mathf.Max(10f, newPreviewHeight);	// At least 10!
			EditorPrefs.SetFloat("EasePreviewTool.PREVIEW_HEIGHT", PREVIEW_HEIGHT);
		}
	}

	/// <summary>
	/// Do the ease preview list. To be called from the OnGUI() call.
	/// </summary>
	private void DoList() {
		// Group in a scroll panel
		m_listScrollPos = EditorGUILayout.BeginScrollView(m_listScrollPos); {
			// Better prefix label width
			float labelWidthBackup = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80f;

			// One curve per type
			Ease[] eases = (Ease[])Enum.GetValues(typeof(Ease));
			foreach(Ease targetEase in eases) {
				// If the preview curve for the current value is not yet created, do it now
				AnimationCurve curve = null;
				if(!m_curves.TryGetValue(targetEase, out curve)) {
					// Create the curve
					switch(targetEase) {
						// Special cases:
						case Ease.Unset:
						case Ease.INTERNAL_Zero:
						case Ease.INTERNAL_Custom: {
							// Don't show any curve
							curve = null;
						} break;

						// Standard case:
						default: {
							// Create new curve
							curve = new AnimationCurve();

							// Initialize!
							float delta = 0f;
							float deltaInc = 1f/(CURVE_SAMPLES - 1);	// Last sample should be at 1f. We know that CURVE_SAMPLES is at least 2.
							for(int i = 0; i < CURVE_SAMPLES; i++) {	
								// Add new key
								curve.AddKey(
									new Keyframe(
										delta,
										DOVirtual.EasedValue(0f, 1f, delta, targetEase)
									)
								);

								// Increase delta
								delta += deltaInc;
							}

							// Make all tangents "Auto"
							// From http://answers.unity3d.com/questions/47968/how-can-i-make-an-animation-curve-keyframe-auto-in.html
							for(int i = 0; i < curve.keys.Length; i++) {
								curve.SmoothTangents(i, 0f); // Zero weight means average
							}
						} break;
					}

					// Add it to the dictionary
					m_curves[targetEase] = curve;
				}

				// Draw the curve field showing a preview of the selected Ease function
				if(curve != null) {
					EditorGUILayout.CurveField(targetEase.ToString(), curve, GUILayout.Height(PREVIEW_HEIGHT));
				}
			}

			// Restore prefix label width
			EditorGUIUtility.labelWidth = labelWidthBackup;
		} EditorGUILayout.EndScrollView();
	}

	/// <summary>
	/// Do the live preview layouting. To be called from the OnGUI() call.
	/// </summary>
	private void DoLivePreview() {
		// The preview
		Rect areaPos = GUILayoutUtility.GetAspectRect(1f);	// This reserves an area in the layout ^^
		GUI.Box(areaPos, "LIVE PREVIEW", EditorStyles.helpBox);

		// Apply transformations
		Vector2 offset = m_moveTween.toggled ? m_moveTween.value : Vector2.zero;
		Vector2 scale = m_scaleTween.toggled ? m_scaleTween.value : Vector2.one;
		Vector3 boxSize = new Vector2(30f * scale.x, 30f * scale.y);

		// Add some margins so the box doesn't go outside the area!
		float margin = 10f;
		Rect canvasPos = new Rect(
			areaPos.x + boxSize.x/2f + margin, 
			areaPos.y + boxSize.y/2f + margin, 
			areaPos.width - boxSize.x - margin * 2f, 
			areaPos.height - boxSize.y - margin * 2f
		);

		// Compute final box position and draw!
		Rect miniBoxPos = new Rect(
			canvasPos.center.x + canvasPos.width/2f * offset.x - boxSize.x/2f, 
			canvasPos.center.y - canvasPos.height/2f * offset.y - boxSize.y/2f, 	// Y axis inverted
			boxSize.x, 
			boxSize.y
		);
		GUI.Box(miniBoxPos, GUIContent.none, m_boxStyle);

		// Show Tweens setup
		EditorGUILayout.Space();
		m_tweensScrollPos = EditorGUILayout.BeginScrollView(m_tweensScrollPos); {
			m_moveTween.OnGUI();
			EditorGUILayout.Space();
			m_scaleTween.OnGUI();
		} EditorGUILayout.EndScrollView();
	}

	/// <summary>
	/// A tween has been toggled.
	/// </summary>
	/// <param name="_tween">Target tween.</param>
	private void OnTweenChanged(LivePreviewTween _tween) {
		// Restart both tweens so they are synced
		m_moveTween.Restart();
		m_scaleTween.Restart();
	}
}

/// <summary>
/// Auxiliar class to easily store, display and setup the live preview.
/// </summary>
public class LivePreviewTween {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Generic
	public string id = "";
	private string prefsId {
		get { return "LivePreviewTween." + id; }
	}

	// Serializable
	public bool toggled = false;
	public Ease ease = Ease.Linear;
	public float duration = 1f;
	public Vector2 fromValue = new Vector2(-1f, -1f);
	public Vector2 toValue = Vector2.one;
	public bool yoyoRepeat = true;

	// Logic
	public float delta = 0f;
	public Vector2 value = Vector2.zero;
	private bool backwards = false;

	// Events
	public class LivePreviewTweenEvent : UnityEvent<LivePreviewTween> { }
	public LivePreviewTweenEvent OnChange = new LivePreviewTweenEvent();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_id">Identifier to save the setup into settings.</param>
	public LivePreviewTween(string _id) {
		// Store ID
		id = _id;
	}

	/// <summary>
	/// Save data to Editor prefs.
	/// </summary>
	public void Save() {
		// Validate some values
		duration = Mathf.Max(0f, duration);
		fromValue.x = Mathf.Clamp(fromValue.x, -1f, 1f);
		fromValue.y = Mathf.Clamp(fromValue.y, -1f, 1f);
		toValue.x = Mathf.Clamp(toValue.x, -1f, 1f);
		toValue.y = Mathf.Clamp(toValue.y, -1f, 1f);

		// Save everything in a single string
		// Use Unity's JSON serialization tools!
		string saveStr = EditorJsonUtility.ToJson(this);

		// Save it!
		EditorPrefs.SetString(prefsId, saveStr);
	}

	/// <summary>
	/// Load data from Editor prefs.
	/// </summary>
	public void Load() {
		// Load from a single string in editor prefs. If it doesn't exist, leave default values
		string loadStr = EditorPrefs.GetString(prefsId, string.Empty);
		if(string.IsNullOrEmpty(loadStr)) return;

		// Use Unity's JSON serialization tools!
		EditorJsonUtility.FromJsonOverwrite(loadStr, this);
	}

	/// <summary>
	/// Do the inspector gui!
	/// </summary>
	public void OnGUI() {
		// Better prefix label width
		float labelWidthBackup = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 100f;

		// Group it all in a toggle group
		EditorGUI.BeginChangeCheck();
		SeparatorAttributeEditor.DrawSeparator(new SeparatorAttribute(5));
		bool wasToggled = toggled;
		toggled = EditorGUILayout.BeginToggleGroup(id, toggled); {
			EditorGUI.indentLevel++;
			ease = (Ease)EditorGUILayout.EnumPopup("Ease", ease);
			duration = EditorGUILayout.FloatField("Duration", duration);
			fromValue = EditorGUILayout.Vector2Field("From", fromValue);
			toValue = EditorGUILayout.Vector2Field("To", toValue);
			yoyoRepeat = EditorGUILayout.Toggle("YoYo Repeat?", yoyoRepeat);
			EditorGUI.indentLevel--;
		} EditorGUILayout.EndToggleGroup();

		// If changes were made, save them!
		if(EditorGUI.EndChangeCheck()) {
			// Save!
			Save();

			// Notify external listeners
			OnChange.Invoke(this);
		}

		// Restore label width
		EditorGUIUtility.labelWidth = labelWidthBackup;
	}

	/// <summary>
	/// Update value
	/// </summary>
	/// <param name="_deltaTime">Delta time.</param>
	public void Update(float _deltaTime) {
		// Update delta
		delta += _deltaTime/duration;

		// Detect loop ending
		if(delta >= 1f) {
			// Reset delta
			delta = 0f;

			// If yoyo-ing, reverse direction
			if(yoyoRepeat) {
				backwards = !backwards;
			}
		}

		// Compute new value!
		// Going forward or backwards?
		float easedDelta = delta;
		if(backwards) {
			easedDelta = DOVirtual.EasedValue(0f, 1f, 1f - delta, ease);
		} else {
			easedDelta = DOVirtual.EasedValue(0f, 1f, delta, ease);
		}
		value = Vector2.LerpUnclamped(fromValue, toValue, easedDelta);
	}

	/// <summary>
	/// Restart the tween from the start.
	/// </summary>
	public void Restart() {
		// Reset delta and backwards properties and force an update
		delta = 0f;
		backwards = false;
		Update(0);
	}
}