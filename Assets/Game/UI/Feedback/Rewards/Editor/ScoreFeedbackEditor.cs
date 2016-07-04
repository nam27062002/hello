// ScoreFeedbackEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Custom editor for the ScoreFeedback class.
/// </summary>
[CustomEditor(typeof(ScoreFeedback), true)]	// True to be used by heir classes as well
public class ScoreFeedbackEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static GUIStyle s_colorPreviewStyle = null;
	private static GUIStyle colorPreviewStyle {
		get {
			if(s_colorPreviewStyle == null) {
				s_colorPreviewStyle = new GUIStyle();
				s_colorPreviewStyle.imagePosition = ImagePosition.ImageOnly;
				s_colorPreviewStyle.stretchHeight = true;
				s_colorPreviewStyle.stretchWidth = true;
			}
			return s_colorPreviewStyle;
		}
	}

	private static GUIStyle s_colorPreviewLinearStyle = null;
	private static GUIStyle colorPreviewLinearStyle {
		get {
			if(s_colorPreviewLinearStyle == null) {
				s_colorPreviewLinearStyle = new GUIStyle();
				s_colorPreviewLinearStyle.imagePosition = ImagePosition.ImageOnly;
				s_colorPreviewLinearStyle.stretchHeight = true;
				s_colorPreviewLinearStyle.stretchWidth = true;
			}
			return s_colorPreviewLinearStyle;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private ScoreFeedback m_targetScoreFeedback = null;

	// Preview gradient object
	private AnimationCurve m_scoreCurve = null;
	private Texture2D m_colorTex = null;
	private Texture2D m_colorTexLinear = null;
	private AnimationCurve m_fontScaleCurve = null;
	private AnimationCurve m_fontScaleCurveLinear = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetScoreFeedback = target as ScoreFeedback;

		// Initialize preview properties
		if(m_scoreCurve == null) {
			m_scoreCurve = new AnimationCurve();
		}
		if(m_colorTex == null) {
			// [AOC] Extracted from https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/GradientEditor.cs
			// Color gradient will be renderd in a 256x2 texture
			m_colorTex = new Texture2D(256, 2, TextureFormat.ARGB32, false);
			m_colorTex.wrapMode = TextureWrapMode.Clamp;
			m_colorTex.hideFlags = HideFlags.HideAndDontSave;

			colorPreviewStyle.normal.background = m_colorTex;
			colorPreviewStyle.active.background = m_colorTex;
			colorPreviewStyle.hover.background = m_colorTex;
			colorPreviewStyle.focused.background = m_colorTex;
			colorPreviewStyle.onNormal.background = m_colorTex;
			colorPreviewStyle.onActive.background = m_colorTex;
			colorPreviewStyle.onHover.background = m_colorTex;
			colorPreviewStyle.onFocused.background = m_colorTex;
		}
		if(m_colorTexLinear == null) {
			// [AOC] Extracted from https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/GradientEditor.cs
			// Color gradient will be renderd in a 256x2 texture
			m_colorTexLinear = new Texture2D(256, 2, TextureFormat.ARGB32, false);
			m_colorTexLinear.wrapMode = TextureWrapMode.Clamp;
			m_colorTexLinear.hideFlags = HideFlags.HideAndDontSave;

			colorPreviewLinearStyle.normal.background = m_colorTexLinear;
			colorPreviewLinearStyle.active.background = m_colorTexLinear;
			colorPreviewLinearStyle.hover.background = m_colorTexLinear;
			colorPreviewLinearStyle.focused.background = m_colorTexLinear;
			colorPreviewLinearStyle.onNormal.background = m_colorTexLinear;
			colorPreviewLinearStyle.onActive.background = m_colorTexLinear;
			colorPreviewLinearStyle.onHover.background = m_colorTexLinear;
			colorPreviewLinearStyle.onFocused.background = m_colorTexLinear;
		}
		if(m_fontScaleCurve == null) {
			m_fontScaleCurve = new AnimationCurve();
		}
		if(m_fontScaleCurveLinear == null) {
			m_fontScaleCurveLinear = new AnimationCurve();
		}

		// Perform a first refresh of the preview properties
		RefreshPreviewVars();
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object and properties
		m_targetScoreFeedback = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Draw default inspector and detect changes to refresh the preview vars
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
		if(EditorGUI.EndChangeCheck()) {
			RefreshPreviewVars();
		}

		// Show some extra controls to help visualize the data, but make them non-editable and customized
		EditorGUILayoutExt.Separator(new SeparatorAttribute("", 20f));
		bool unfolded = EditorPrefs.GetBool("ScoreFeedbackEditorDataPreviewUnfolded", true);
		unfolded = EditorGUILayout.Foldout(unfolded, "Data Preview");
		EditorPrefs.SetBool("ScoreFeedbackEditorDataPreviewUnfolded", unfolded);
		if(unfolded) {
			// Show score preview
			GUILayout.Label("Score Curve", CustomEditorStyles.commentLabelLeft);
			EditorGUILayout.CurveField(m_scoreCurve, GUILayout.Height(100f));
			EditorGUILayout.Space();

			// Show color preview
			GUILayout.Label("Color Gradient", CustomEditorStyles.commentLabelLeft);
			GUILayout.Box("", colorPreviewStyle, GUILayout.Height(100f));
			EditorGUILayout.Space();

			GUILayout.Label("Color Gradient (Linear)", CustomEditorStyles.commentLabelLeft);
			GUILayout.Box("", colorPreviewLinearStyle, GUILayout.Height(100f));
			EditorGUILayout.Space();
			
			// Show font scale preview
			GUILayout.Label("Font Scale Curve", CustomEditorStyles.commentLabelLeft);
			EditorGUILayout.CurveField(m_fontScaleCurve, GUILayout.Height(100f));
			EditorGUILayout.Space();

			GUILayout.Label("Font Scale Curve (Linear)", CustomEditorStyles.commentLabelLeft);
			EditorGUILayout.CurveField(m_fontScaleCurveLinear, GUILayout.Height(100f));

			// Apply any changed performed in the default inspector - changes performed from this point on will be ignored
			serializedObject.ApplyModifiedProperties();
		}
	}

	/// <summary>
	/// Make sure preview vars are updated.
	/// Should be called every time a value changes or upon enabling the inspector.
	/// </summary>
	private void RefreshPreviewVars() {
		// Update preview vars
		List<ScoreFeedback.ScoreThreshold> thresholds = m_targetScoreFeedback.thresholds;

		// Score curve
		// Use the same loop to store min and max score values (used to initialize the rest of the preview objects)
		List<Keyframe> newScoreKeys = new List<Keyframe>();
		Range scoreRange = new Range(0f, float.MinValue);
		Keyframe k;
		for(int i = 0; i < thresholds.Count; i++) {
			k = new Keyframe(i, thresholds[i].maxScore);
			newScoreKeys.Add(k);
			scoreRange.max = Mathf.Max(thresholds[i].maxScore, scoreRange.max);
		}
		//m_scorePreview.keys = newScoreKeys.ToArray();
		m_scoreCurve = new AnimationCurve(newScoreKeys.ToArray());	// [AOC] Unity bug where curve preview is not refreshed when changing the keys http://answers.unity3d.com/questions/385889/refresh-curve-field-previews.html

		// Make it linear
		m_scoreCurve.MakeLinear();

		// Color preview
		if(m_colorTex != null) {
			// [AOC] The initial idea was to use Unity's Gradient class, but it's limited to 8 color keys so it's not enough for us
			//       As an alternative, manually create a texture with the gradient (as Unity's GradientEditor does)
			//       Use however the GradientColorKey class for convenience :)
			//       See https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/GradientEditor.cs
			List<GradientColorKey> newColorKeys = new List<GradientColorKey>();
			for(int i = 0; i < thresholds.Count; i++) {
				// Check score range
				int maxScore = thresholds[i].maxScore - 1;
				int minScore = 0;
				if(i > 0) {
					minScore = thresholds[i - 1].maxScore;
				}

				// Create a new global gradient key for each color key in the threshold's gradient
				for(int j = 0; j < thresholds[i].colorRange.colorKeys.Length; j++) {
					float score = Mathf.Lerp(minScore, maxScore, thresholds[i].colorRange.colorKeys[j].time);
					GradientColorKey newKey = new GradientColorKey(
						thresholds[i].colorRange.colorKeys[j].color,
						scoreRange.InverseLerp(score)
					);
					newColorKeys.Add(newKey);
				}
			}

			// Make sure keys are sorted by time
			newColorKeys.Sort((x, y) => {
				return x.time.CompareTo(y.time);
			});

			// If there is no key at time 0 and time 1, add them
			if(newColorKeys.Count == 0) {
				newColorKeys.Add(new GradientColorKey(Color.gray, 0f));
				newColorKeys.Add(new GradientColorKey(Color.gray, 1f));
			} else {
				if(newColorKeys[0].time > Mathf.Epsilon) {
					newColorKeys.Insert(0, new GradientColorKey(newColorKeys[0].color, 0f));
				}
				if(newColorKeys[newColorKeys.Count - 1].time < (1f - Mathf.Epsilon)) {
					newColorKeys.Add(new GradientColorKey(newColorKeys[newColorKeys.Count - 1].color, 1f));
				}
			}

			// Generate the texture by manually interpolating between keys
			// Preview is a texture of 256x2
			// Assume keys 
			Color[] pixels = new Color[512];
			Color[] pixelsLinear = new Color[512];
			for(int i = 0; i < newColorKeys.Count - 1; i++) {
				GradientColorKey k0 = newColorKeys[i];
				GradientColorKey k1 = newColorKeys[i+1];

				// Real version
				int col0 = (int)(k0.time * 255f);
				int col1 = (int)(k1.time * 255f);
				for(int j = col0; j <= col1; j++) {
					// Fill both rows at once with this double assignation
					pixels[j] = pixels[j + 256] = Color.Lerp(k0.color, k1.color, Mathf.InverseLerp(col0, col1, j));
				}

				// Linear version
				col0 = (int)(Mathf.InverseLerp(0, newColorKeys.Count - 1, i) * 255f);
				col1 = (int)(Mathf.InverseLerp(0, newColorKeys.Count - 1, i+1) * 255f);
				for(int j = col0; j <= col1; j++) {
					// Fill both rows at once with this double assignation
					pixelsLinear[j] = pixelsLinear[j + 256] = Color.Lerp(k0.color, k1.color, Mathf.InverseLerp(col0, col1, j));
				}
			}

			m_colorTex.SetPixels(pixels);
			m_colorTex.Apply();

			m_colorTexLinear.SetPixels(pixelsLinear);
			m_colorTexLinear.Apply();
		}

		// Font scale curves
		List<Keyframe> newFontScaleKeys = new List<Keyframe>();
		List<Keyframe> newFontScaleKeysLinear = new List<Keyframe>();
		for(int i = 0; i < thresholds.Count; i++) {
			// Check score range
			int maxScore = thresholds[i].maxScore - 1;
			int minScore = 0;
			if(i > 0) {
				minScore = thresholds[i - 1].maxScore;
			}

			// Create a new key for each font scale factor in the threshold
			// Min
			k = new Keyframe(minScore, thresholds[i].fontScaleRange.min);
			newFontScaleKeys.Add(k);

			// Max
			k = new Keyframe(maxScore, thresholds[i].fontScaleRange.max);
			newFontScaleKeys.Add(k);

			// Linear min
			k = new Keyframe(i, thresholds[i].fontScaleRange.min);
			newFontScaleKeysLinear.Add(k);

			// Linear max
			k = new Keyframe(i + 0.9f, thresholds[i].fontScaleRange.max);
			newFontScaleKeysLinear.Add(k);
		}
		//m_fontScalePreview.keys = newFontScaleKeys.ToArray();
		m_fontScaleCurve = new AnimationCurve(newFontScaleKeys.ToArray());	// [AOC] Unity bug where curve preview is not refreshed when changing the keys http://answers.unity3d.com/questions/385889/refresh-curve-field-previews.html
		m_fontScaleCurveLinear = new AnimationCurve(newFontScaleKeysLinear.ToArray());

		// Make it linear
		m_fontScaleCurve.MakeLinear();
		m_fontScaleCurveLinear.MakeLinear();
	}
}