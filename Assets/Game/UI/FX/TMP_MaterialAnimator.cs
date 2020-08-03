// TMProMaterialAnimator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Proxy component to be able to animate TextMeshPro materials through an Animator
/// </summary>
public class TMP_MaterialAnimator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Generic class + specializations so they can be serialized
	/// <summary>
	/// Top-most interface, defininig some methods to be used externally.
	/// </summary>
	[System.Serializable]
	private abstract class IMaterialProperty {
		[HideInInspector] public string propertyID = "";
		public bool wasToggled = false;
		public delegate bool GetToggleDelegate();
		public GetToggleDelegate IsToggled = null;

		[System.NonSerialized] public Material targetMaterial = null;

		public abstract void Init(Material _m);
		public abstract void Apply();
		public abstract void Restore();
	}

	/// <summary>
	/// Generic class, adding value treatment.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[System.Serializable]
	private abstract class IMaterialProperty<T> : IMaterialProperty {
		//public T value = default(T);
		public delegate T GetValueDelegate();
		public GetValueDelegate GetValue = null;

		[System.NonSerialized] public T originalValue = default(T);

		public abstract void SetValueToMaterial(T _value);
		public abstract T GetValueFromMaterial();

		public override void Init(Material _m) {
			targetMaterial = _m;
			originalValue = GetValueFromMaterial();
		}

		public override void Apply() {
			SetValueToMaterial(GetValue());
		}

		public override void Restore() {
			SetValueToMaterial(originalValue);
		}
	}

	/// <summary>
	/// Color
	/// </summary>
	[System.Serializable] private class ColorProperty : IMaterialProperty<Color> {
		public override Color GetValueFromMaterial() {
			return targetMaterial.GetColor(propertyID);
		}

		public override void SetValueToMaterial(Color _value) {
			targetMaterial.SetColor(propertyID, _value);
		}
	};

	/// <summary>
	/// Float
	/// </summary>
	[System.Serializable] private class FloatProperty : IMaterialProperty<float> {
		public override float GetValueFromMaterial() {
			return targetMaterial.GetFloat(propertyID);
		}

		public override void SetValueToMaterial(float _value) {
			targetMaterial.SetFloat(propertyID, _value);
		}
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Header("Face")]
	[SerializeField] private bool m_faceColorToggle = false;
	[SerializeField] private Color m_faceColor = Color.white;

	[SerializeField] private bool m_faceSoftnessToggle = false;
	[SerializeField] [Range(0f, 1f)] private float m_faceSoftness = 0f;

	[SerializeField] private bool m_faceDilateToggle = false;
	[SerializeField] [Range(-1f, 1f)] private float m_faceDilate = 0f;

	[Header("Outline")]
	[SerializeField] private bool m_outlineColorToggle = false;
	[SerializeField] private Color m_outlineColor = Color.black;

	[SerializeField] private bool m_outlineThicknessToggle = false;
	[SerializeField] [Range(0f, 1f)] private float m_outlineThickness = 0f;

	[Header("Underlay")]
	[SerializeField] private bool m_underlayColorToggle = false;
	[SerializeField] private Color m_underlayColor = Colors.WithAlpha(Color.black, 0.5f);

	[SerializeField] private bool m_underlayOffsetXToggle = false;
	[SerializeField] [Range(-1f, 1f)] private float m_underlayOffsetX = 0f;

	[SerializeField] private bool m_underlayOffsetYToggle = false;
	[SerializeField] [Range(-1f, 1f)] private float m_underlayOffsetY = 0f;

	[SerializeField] private bool m_underlayDilateToggle = false;
	[SerializeField] [Range(-1f, 1f)] private float m_underlayDilate = 0f;

	[SerializeField] private bool m_underlaySoftnessToggle = false;
	[SerializeField] [Range(0f, 1f)] private float m_underlaySoftness = 0f;

	// Internal
	private IMaterialProperty[] m_properties = null;
	private bool m_dirty = true;
	private TextMeshProUGUI m_targetText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Store reference to target text
		m_targetText = GetComponent<TextMeshProUGUI>();

		// Create a MaterialProperty data object for each property and put them all into a List for convenience
		m_properties = new IMaterialProperty[] {
			// Face
			new ColorProperty {
				propertyID = "_FaceColor",
				IsToggled = () => m_faceColorToggle,
				GetValue = () => m_faceColor
			},

			new FloatProperty {
				propertyID = "_OutlineSoftness",
				IsToggled = () => m_faceSoftnessToggle,
				GetValue = () => m_faceSoftness
			},

			new FloatProperty {
				propertyID = "_FaceDilate",
				IsToggled = () => m_faceDilateToggle,
				GetValue = () => m_faceDilate
			},

			// Outline
			new ColorProperty {
				propertyID = "_OutlineColor",
				IsToggled = () => m_outlineColorToggle,
				GetValue = () => m_outlineColor
			},

			new FloatProperty {
				propertyID = "_OutlineWidth",
				IsToggled = () => m_outlineThicknessToggle,
				GetValue = () => m_outlineThickness
			},

			// Underlay
			new ColorProperty {
				propertyID = "_UnderlayColor",
				IsToggled = () => m_underlayColorToggle,
				GetValue = () => m_underlayColor
			},

			new FloatProperty {
				propertyID = "_UnderlayOffsetX",
				IsToggled = () => m_underlayOffsetXToggle,
				GetValue = () => m_underlayOffsetX
			},

			new FloatProperty {
				propertyID = "_UnderlayOffsetY",
				IsToggled = () => m_underlayOffsetYToggle,
				GetValue = () => m_underlayOffsetY
			},

			new FloatProperty {
				propertyID = "_UnderlayDilate",
				IsToggled = () => m_underlayDilateToggle,
				GetValue = () => m_underlayDilate
			},

			new FloatProperty {
				propertyID = "_UnderlaySoftness",
				IsToggled = () => m_underlaySoftnessToggle,
				GetValue = () => m_underlaySoftness
			},
		};

		// Initialize properties
		for(int i = 0; i < m_properties.Length; ++i) {
			m_properties[i].Init(m_targetText.fontMaterial);
		}
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Keep material updated
		UpdateValues();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update material properties.
	/// </summary>
	private void UpdateValues() {
		// Apply all toggled properties
		for(int i = 0; i < m_properties.Length; ++i) {
			if(m_properties[i].IsToggled()) {
				// Store toggle status
				m_properties[i].wasToggled = true;
				m_properties[i].Apply();
			} else if(m_properties[i].wasToggled) {
				// Restore original values if no longer toggled
				m_properties[i].wasToggled = false;
				m_properties[i].Restore();
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}