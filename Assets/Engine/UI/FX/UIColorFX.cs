// UIColorFX.cs
// 
// Created by Alger Ortín Castellví on 12/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Apply several color effects to all UI 2D graphic or text within this object's hierarchy.
/// Will replace their material by a new one created in runtime.
/// Strongly based on http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
/// </summary>
[ExecuteInEditMode]	// This way we can preview the changes while editing
public class UIColorFX : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public Color colorMultiply = new Color(1, 1, 1, 1);
	public Color colorAdd = new Color(0, 0, 0, 0);

	[Space]
	[Range(0f, 1f)] public float alpha = 1f;	// Will be multiplied to the source and tint alpha components

	[Space]
	[Range(-1f, 1f)] public float brightness = 0f;
	[Range(-1, 1f)] public float saturation = 0f;
	[Range(-1, 1)] public float contrast = 0f;

	// Custom materials for images and fonts
	// Unfortunately they can't share the same material since rendering techniques are a bit different
	private Material m_imageMaterial = null;
	private Material m_fontMaterial = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	void OnEnable() {
		// Initialize materials
		ApplyMaterials();
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update() {
		// Detect hierarchy changes
        // We assume that hierarchy is not going to change when the application is running in order to prevent memory from being allocated potencially every tick,
        // however we want to apply the materials in edit time (hierarchy in edit time might change in order to check how a new widget would look like in the hierarchy)
		if(!Application.isPlaying && transform.hasChanged) {
			ApplyMaterials();
		}

		// Keep shaders updated
		UpdateValues();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	void OnDisable() {
		// On editor mode, destroy materials every time we unselect the object.
		if(!Application.isPlaying) {
			DestroyMaterials();
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {
		// Destroy created materials
		DestroyMaterials();
	}

	/// <summary>
	/// A change has been made on the inspector.
	/// http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnValidate.html
	/// </summary>
	protected void OnValidate() {
		// Make sure all children have the proper material (for children added after the component)
		ApplyMaterials();
		UpdateValues();
	}

	/// <summary>
	/// Reset to default values!
	/// </summary>
	public void Reset() {
		brightness = 0f;
		saturation = 0f;
		contrast = 0f;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the values on the shader.
	/// </summary>
	private void UpdateValues() {
		// Image
		if(m_imageMaterial != null) {
			m_imageMaterial.SetColor("_ColorMultiply", colorMultiply);
			m_imageMaterial.SetColor("_ColorAdd", colorAdd);
			m_imageMaterial.SetFloat("_Alpha", alpha);
			m_imageMaterial.SetFloat("_BrightnessAmount", brightness);
			m_imageMaterial.SetFloat("_SaturationAmount", saturation);
			m_imageMaterial.SetFloat("_ContrastAmount", contrast);
		}

		// Fonts
		if(m_fontMaterial != null) {
			m_fontMaterial.SetColor("_ColorMultiply", colorMultiply);
			m_fontMaterial.SetColor("_ColorAdd", colorAdd);
			m_fontMaterial.SetFloat("_Alpha", alpha);
			m_fontMaterial.SetFloat("_BrightnessAmount", brightness);
			m_fontMaterial.SetFloat("_SaturationAmount", saturation);
			m_fontMaterial.SetFloat("_ContrastAmount", contrast);
		}
	}

	/// <summary>
	/// Make sure materials are created and apply them to all subchildren.
	/// </summary>
	private void ApplyMaterials() {
        // Create all materials if not already done
        Material mBase = Resources.Load<Material>("UI/UIImageHolder");
		if(m_imageMaterial == null) {
			m_imageMaterial = new Material(mBase);
			m_imageMaterial.hideFlags = HideFlags.HideAndDontSave;
			m_imageMaterial.name = "MT_UIColorFX";
		}

        mBase = Resources.Load<Material>("UI/UIFontHolder");
        if (m_fontMaterial == null) {
			m_fontMaterial = new Material(mBase);
			m_fontMaterial.hideFlags = HideFlags.HideAndDontSave;
			m_fontMaterial.name = "MT_UIColorFX";
		}

		// Get all image components and replace their material
		if(m_imageMaterial != null) {
			Image[] images = GetComponentsInChildren<Image>();
			foreach(Image img in images) {
				img.material = m_imageMaterial;
			}
		}

		// Do the same with textfields
		if(m_fontMaterial != null) {
			Text[] texts = GetComponentsInChildren<Text>();
			foreach(Text txt in texts) {
				txt.material = m_fontMaterial;
			}
		}
	}

	/// <summary>
	/// Destroy the custom materials.
	/// </summary>
	private void DestroyMaterials() {
		if(m_imageMaterial != null) {
			DestroyImmediate(m_imageMaterial);
			m_imageMaterial = null;
		}

		if(m_fontMaterial != null) {
			DestroyImmediate(m_fontMaterial);
			m_fontMaterial = null;
		}
	}
}
