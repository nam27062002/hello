// UIColorFX.cs
// 
// Created by Alger Ortín Castellví on 12/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//
//#define ALLOW_RAMP_INTENSITY   // [AOC] Disable to make it more optimal by just getting the full value from the gradient

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Apply several color effects to all UI 2D graphic or text within this object's hierarchy.
/// Will replace their material by a new one created in runtime.
/// Strongly based on http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
/// </summary>
[ExecuteInEditMode]	// This way we can preview the changes while editing
public class UIColorFX : UIBehaviour {	// Inherit from UIBehaviour to have some extra useful callbacks
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string IMAGE_REFERENCE_MATERIAL_PATH = "UI/UIImageHolder";
	public const string TEXT_REFERENCE_MATERIAL_PATH = "UI/UIFontHolder";
	public const string COLOR_RAMP_MATERIAL_SUFFIX = "_Ramp";

	// Auxiliar class
	[System.Serializable]
	public class Setup {
		// Members
		public Color colorMultiply = Color.white;
		public Color colorAdd = new Color(0f, 0f, 0f, 0f);	// Alpha 0!

		[Space]
		[Range(0f, 1f)] public float alpha = 1f;	// Will be multiplied to the source and tint alpha components

		[Space]
		[Range(-1f, 1f)] public float brightness = 0f;
		[Range(-1, 1f)] public float saturation = 0f;
		[Range(-1, 1)] public float contrast = 0f;

		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		public Setup(Color _multiply, Color _add, float _alpha, float _brightness, float _saturation, float _contrast) {
			colorMultiply = _multiply;
			colorAdd = _add;
			alpha = _alpha;
			brightness = _brightness;
			saturation = _saturation;
			contrast = _contrast;
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Comment("For memory optimization, don't toggle if not needed!")]
	[SerializeField] private bool m_applyToFonts = false;
	[SerializeField] private bool m_applyToImages = true;

	[Space]
	[FormerlySerializedAs("colorMultiply")]
	[SerializeField] private Color m_colorMultiply = new Color(1, 1, 1, 1);
	public Color colorMultiply {
		get { return m_colorMultiply; }
		set { m_colorMultiply = value; SetDirty(); }
	}

	[FormerlySerializedAs("colorAdd")]
	[SerializeField] private Color m_colorAdd = new Color(0, 0, 0, 0);
	public Color colorAdd {
		get { return m_colorAdd; }
		set { m_colorAdd = value; SetDirty(); }
	}

	[Space]
	[SerializeField] private bool m_colorRampEnabled = false;
	public bool colorRampEnabled {
		get { return m_colorRampEnabled; }
		set {
			// If value changes, force a reload of the materials
			if(m_colorRampEnabled != value) DestroyMaterials();
			m_colorRampEnabled = value;
			SetDirty(); 
		}
	}

	[SerializeField] private Texture2D m_colorRamp = null;
	public Texture2D colorRamp {
		get { return m_colorRamp; }
		set { m_colorRamp = value;  SetDirty(); }
	}

#if ALLOW_RAMP_INTENSITY
	[SerializeField] [Range(0f, 1f)] private float m_colorRampIntensity = 0f;
	public float colorRampIntensity {
		get { return m_colorRampIntensity; }
		set { m_colorRampIntensity = value; SetDirty(); }
	}
#endif

	[Space]
	[FormerlySerializedAs("alpha")]
	[SerializeField] [Range(0f, 1f)] private float m_alpha = 1f;	// Will be multiplied to the source and tint alpha components
	public float alpha {
		get { return m_alpha; }
		set { m_alpha = value; SetDirty(); }
	}

	[Space]
	[FormerlySerializedAs("brightness")]
	[SerializeField] [Range(-1f, 1f)] private float m_brightness = 0f;
	public float brightness {
		get { return m_brightness; }
		set { m_brightness = value; SetDirty(); }
	}

	[FormerlySerializedAs("saturation")]
	[SerializeField] [Range(-1, 1f)] private float m_saturation = 0f;
	public float saturation {
		get { return m_saturation; }
		set { m_saturation = value; SetDirty(); }
	}

	[FormerlySerializedAs("contrast")]
	[SerializeField] [Range(-1, 1)] private float m_contrast = 0f;
	public float contrast {
		get { return m_contrast; }
		set { m_contrast = value; SetDirty(); }
	}

	[Space]
	[Tooltip("If toggled, multiply will be applied after Brightness, Saturation and Contrast (but always before additive)")]
	[SerializeField] private bool m_lateMultiply = false;
	public bool lateMultiply {
		get { return m_lateMultiply; }
		set { m_lateMultiply = value; SetDirty(); }
	}

	// Custom materials for images and fonts
	// Unfortunately they can't share the same material since rendering techniques are a bit different
	private Material m_imageMaterial = null;
	public Material imageMaterial {
		get { return m_imageMaterial; }
	}

	private Material m_imageMaterialWithRamp = null;
	public Material imageMaterialWithRamp {
		get { return m_imageMaterialWithRamp; }
	}

	private Material m_fontMaterial = null;
	public Material fontMaterial {
		get { return m_fontMaterial; }
	}

	// Material replacement support
	// Adding this to support IMaterialModifiers changing the actual material being used. This has to be properly set by the IMaterialModifier.
	private Material m_imageMaterialReplacement = null;
	public Material imageMaterialReplacement {
		get { return m_imageMaterialReplacement; }
		set {
			m_imageMaterialReplacement = value;
			SetDirty();
		}
	}

	private Material m_fontMaterialReplacement = null;
	public Material fontMaterialReplacement {
		get { return m_fontMaterialReplacement; }
		set {
			m_fontMaterialReplacement = value;
			SetDirty();
		}
	}

	// Internal
	private bool m_dirty = true;
	private bool m_materialDirty = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize materials
		ApplyMaterials();
		SetDirty();
		SetMaterialDirty();
	}
	
	/// <summary>
	/// Update is called once per frame
	/// </summary>
	private void Update() {
		// Detect hierarchy changes
        // We assume that hierarchy is not going to change when the application is running in order to prevent memory from being allocated potentially every tick,
        // however we want to apply the materials in edit time (hierarchy in edit time might change in order to check how a new widget would look like in the hierarchy)
#if UNITY_EDITOR
		if(!Application.isPlaying && transform.hasChanged) {
			// Make sure materials are valid
			m_materialDirty = true;
			m_dirty = true;
			transform.hasChanged = false;
		}
#endif

		// Keep materials updated
		if(m_materialDirty) {
			ApplyMaterials();
			m_materialDirty = false;
		}

		// Keep shaders updated
		if(m_dirty) {
			UpdateValues();
			m_dirty = false;
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// On editor mode, destroy materials every time we unselect the object.
		if(!Application.isPlaying) {
			DestroyMaterials();
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
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

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply a specific setup to this color FX.
	/// </summary>
	/// <param name="_setup">The setup to be applied.</param>
	public void Apply(Setup _setup) {
		// Check params
		if(_setup == null) return;

		// Copy values
		this.colorMultiply = _setup.colorMultiply;
		this.colorAdd = _setup.colorAdd;

		this.alpha = _setup.alpha;

		this.brightness = _setup.brightness;
		this.saturation = _setup.saturation;
		this.contrast = _setup.contrast;

		// If in edit mode, force an update
#if UNITY_EDITOR
		if(!Application.isPlaying) {
			SetDirty();
			Update();
		}
#endif
	}

	/// <summary>
	/// Reset to default values!
	/// </summary>
	public void Reset() {
		brightness = 0f;
		saturation = 0f;
		contrast = 0f;
		SetDirty();
	}

	/// <summary>
	/// Force an refresh of the materials on the next update call.
	/// </summary>
	public void SetDirty() {
		m_dirty = true;

		// If in edit mode, force an update so new values are applied
#if UNITY_EDITOR
		if(!Application.isPlaying) {
			Update();
		}
#endif
	}

	/// <summary>
	/// Force a refresh of the materials in the whole nested hierarchy on the next
	/// update call.
	/// Performance-heavy, use with caution.
	/// </summary>
	public void SetMaterialDirty() {
		m_materialDirty = true;

		// If in edit mode, force an update so new values are applied
#if UNITY_EDITOR
		if(!Application.isPlaying) {
			Update();
		}
#endif
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update the values on the shader.
	/// </summary>
	private void UpdateValues() {
		// Images
		if(m_applyToImages) {
			UpdateMaterial(m_imageMaterial);
			if(m_colorRampEnabled) {
				UpdateMaterial(m_imageMaterialWithRamp);
			}
			UpdateMaterial(m_imageMaterialReplacement);
		}

		// Fonts
		if(m_applyToFonts) {
			UpdateMaterial(m_fontMaterial);
			UpdateMaterial(m_fontMaterialReplacement);
		}
	}

	/// <summary>
	/// Update the values on the shader of the given material.
	/// </summary>
	/// <param name="_mat">target material.</param>
	private void UpdateMaterial(Material _mat) {
		if(_mat != null) {
			_mat.SetColor("_ColorMultiply", colorMultiply);
			_mat.SetColor("_ColorAdd", colorAdd);

			// Color ramp: only if enabled and if material is the ramp material
			if(_mat == m_imageMaterialWithRamp) {
				_mat.SetFloat("_ColorRampEnabled", m_colorRampEnabled ? 1f : 0f);
				if(m_colorRampEnabled) {
					_mat.SetTexture("_ColorRampTex", colorRamp);
#if ALLOW_RAMP_INTENSITY
					_mat.SetFloat("_ColorRampIntensity", colorRampIntensity);
#endif
				}
			}

			_mat.SetFloat("_Alpha", alpha);

			_mat.SetFloat("_BrightnessAmount", brightness);
			_mat.SetFloat("_SaturationAmount", saturation);
			_mat.SetFloat("_ContrastAmount", contrast);

			_mat.SetFloat("_LateMultiply", lateMultiply ? 1f : 0f);
		}
	}

	/// <summary>
	/// Make sure materials are created and apply them to all subchildren.
	/// </summary>
	private void ApplyMaterials() {
		// Create all materials if not already done
		if(m_applyToImages) {
			// Without ramp
			if(m_imageMaterial == null) {
				Material matBase = Resources.Load<Material>(IMAGE_REFERENCE_MATERIAL_PATH);
				m_imageMaterial = new Material(matBase);
				m_imageMaterial.hideFlags = HideFlags.HideAndDontSave;
				m_imageMaterial.name = "MT_UIColorFX_" + this.name;
			}

			// With ramp (only if needed)
			if(m_colorRampEnabled && m_imageMaterialWithRamp == null) {
				Material matBase = Resources.Load<Material>(IMAGE_REFERENCE_MATERIAL_PATH + COLOR_RAMP_MATERIAL_SUFFIX);
				m_imageMaterialWithRamp = new Material(matBase);
				m_imageMaterialWithRamp.hideFlags = HideFlags.HideAndDontSave;
				m_imageMaterialWithRamp.name = "MT_UIColorFX_" + this.name + COLOR_RAMP_MATERIAL_SUFFIX;
			}
		}

		if(m_applyToFonts) {
			if(m_fontMaterial == null) {
				Material matBase = Resources.Load<Material>(TEXT_REFERENCE_MATERIAL_PATH);
				m_fontMaterial = new Material(matBase);
				m_fontMaterial.hideFlags = HideFlags.HideAndDontSave;
				m_fontMaterial.name = "MT_UIColorFX_" + this.name;
			}
		}

		// Get all image components and replace their material
		if(m_applyToImages) {
			if(m_imageMaterial != null) {
				Material targetMaterial = null;
				Image[] images = GetComponentsInChildren<Image>();
				foreach(Image img in images) {
					// If using color ramp, use the color ramp material ONLY for this object
					targetMaterial = m_imageMaterial;
					if(img.gameObject == this.gameObject) {
						if(m_colorRampEnabled && m_imageMaterialWithRamp != null) {
							targetMaterial = m_imageMaterialWithRamp;
						}
					}

					// Apply selected material
					img.material = targetMaterial;
				}
			}
		}

		// Do the same with textfields
		if(m_applyToFonts) {
			if(m_fontMaterial != null) {
				Text[] texts = GetComponentsInChildren<Text>();
				foreach(Text txt in texts) {
					txt.material = m_fontMaterial;
				}
			}
		}
	}

	/// <summary>
	/// Destroy the custom materials.
	/// </summary>
	public void DestroyMaterials() {
		if(m_imageMaterial != null) {
			DestroyImmediate(m_imageMaterial);
			m_imageMaterial = null;
		}

		if(m_imageMaterialWithRamp != null) {
			DestroyImmediate(m_imageMaterialWithRamp);
			m_imageMaterialWithRamp = null;
		}

		if(m_imageMaterialReplacement != null) {
			DestroyImmediate(m_imageMaterialReplacement);
			m_imageMaterialReplacement = null;
		}

		if(m_fontMaterial != null) {
			DestroyImmediate(m_fontMaterial);
			m_fontMaterial = null;
		}

		if(m_fontMaterialReplacement != null) {
			DestroyImmediate(m_fontMaterialReplacement);
			m_fontMaterialReplacement = null;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Callback for when properties have been changed by animation.
	/// </summary>
	override protected void OnDidApplyAnimationProperties() {
		// Call parent
		base.OnDidApplyAnimationProperties();

		// Mark as dirty!
		SetDirty();
	}

	/// <summary>
	/// This function is called when the list of children of the transform of the GameObject has changed.
	/// </summary>
	private void OnTransformChildrenChanged() {
		// Material must be applied to the new children!
		SetMaterialDirty();
	}
}
