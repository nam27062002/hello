// UIColorModifier.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Apply several color effects to a target UI object and all of its children.
/// Strongly based on http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
/// LIMITATION: Doesn't apply to objects added to the hierarchy after the Start() call.
/// </summary>
[ExecuteInEditMode]	// This way we can preview the changes while editing
public class UIColorModifier : MonoBehaviour {
	#region EXPOSED PROPERTIES -----------------------------------------------------------------------------------------
	public Color colorMultiply = new Color(1, 1, 1, 1);
	public Color colorAdd = new Color(0, 0, 0, 0);
	[Range(0f, 1f)] public float alpha = 1f;
	[Range(0f, 10f)] public float brightness = 1f;	// [AOC] Arbitrary limits
	[Range(0f, 2f)] public float saturation = 1f;	// [AOC] Arbitrary limits
	[Range(0f, 5f)] public float contrast = 1f;	// [AOC] Arbitrary limits
	#endregion

	#region INTERNAL PROPERTIES ----------------------------------------------------------------------------------------
	// Custom materials for images and fonts
	// Unfortunately they can't share the same material since rendering techniques are a bit different
	private Material mImageMaterial;
	private Material mFontMaterial;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start () {
		// Create all materials
		mImageMaterial = new Material(Shader.Find("Custom/UI/UIImage"));
		mImageMaterial.hideFlags = HideFlags.HideAndDontSave;

		mFontMaterial = new Material(Shader.Find("Custom/UI/UIFont"));
		mFontMaterial.hideFlags = HideFlags.HideAndDontSave;


		// Get all image components and replace their material
		if(mImageMaterial != null) {
			Image[] images = GetComponentsInChildren<Image>();
			foreach(Image img in images) {
				img.material = mImageMaterial;
			}
		}

		// Do the same with textfields
		if(mFontMaterial != null) {
			Text[] texts = GetComponentsInChildren<Text>();
			foreach(Text txt in texts) {
				txt.material = mFontMaterial;
			}
		}
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () {
		// Keep shaders updated
		if(mImageMaterial != null) {
			mImageMaterial.SetColor("_ColorMultiply", colorMultiply);
			mImageMaterial.SetColor("_ColorAdd", colorAdd);
			mImageMaterial.SetFloat("_Alpha", alpha);
			mImageMaterial.SetFloat("_BrightnessAmount", brightness);
			mImageMaterial.SetFloat("_SaturationAmount", saturation);
			mImageMaterial.SetFloat("_ContrastAmount", contrast);
		}

		// Keep shaders updated
		if(mFontMaterial != null) {
			mFontMaterial.SetColor("_ColorMultiply", colorMultiply);
			mFontMaterial.SetColor("_ColorAdd", colorAdd);
			mFontMaterial.SetFloat("_Alpha", alpha);
			mFontMaterial.SetFloat("_BrightnessAmount", brightness);
			mFontMaterial.SetFloat("_SaturationAmount", saturation);
			mFontMaterial.SetFloat("_ContrastAmount", contrast);
		}
	}

	/// <summary>
	/// Destroy created material.
	/// </summary>
	void OnDisable() {
		if(mImageMaterial) {
			DestroyImmediate(mImageMaterial);
		}

		if(mFontMaterial) {
			DestroyImmediate(mFontMaterial);
		}
	}
	#endregion
}
#endregion
