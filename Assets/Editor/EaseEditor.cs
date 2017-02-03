// EaseEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Attempt at doing a custom inspector for the Ease enum.
/// </summary>
[CustomPropertyDrawer(typeof(Ease))]
public class EaseEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const int CURVE_SAMPLES = 10;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Cache preview curves
	protected Dictionary<Ease, AnimationCurve> m_curves = new Dictionary<Ease, AnimationCurve>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public EaseEditor() {
		// Nothing to do
	}

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Draw property with the default setting (enum foldable dropdown)
		m_pos.height = EditorGUI.GetPropertyHeight(_property);
		EditorGUI.PropertyField(m_pos, _property);
		AdvancePos();

		// Find out property's current value
		Ease targetEase = (Ease)_property.enumValueIndex;

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
					float deltaInc = 1f/CURVE_SAMPLES;
					for(int i = 0; i < CURVE_SAMPLES + 1; i++) {	// One extra sample for the last point
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
			m_pos.height = 50f;
			EditorGUI.CurveField(m_pos, "·", curve);
			AdvancePos();
		}
	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}
}