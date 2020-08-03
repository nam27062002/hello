// ShinyGodraysFX.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to control the God Rays FX in the open-egg screen.
/// Better performance version than the original GodRaysFX.
/// </summary>
public class ShinyGodraysFX : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Auxiliar class to hold all colors defining the FX
	private class ColorData {
		public Color raysBasicColor = Color.black;
		public Color raysSaturatedColor = Color.black;
		public Color raysStartColorMax = Color.black;

		public Color glowStartColor = Color.black;

		public Color sparksStartColorMin = Color.black;
		public Color sparksStartColorMax = Color.black;
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ParticleSystem m_raysPS = null;
	[SerializeField] private ParticleSystem m_glowPS = null;
	[SerializeField] private ParticleSystem m_sparksPS = null;

	// Internal
	private ColorData m_originalColors = null;
	private ColorData originalColors {
		get { 
			if(m_originalColors == null) {
				AcquireOriginalColors();
			}
			return m_originalColors;
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start (or restart) the FX with a given rarity.
	/// </summary>
	/// <param name="_rarity">The rarity to be used to initialize the FX.</param>
	public void Tint(Color _color) {
		// Create a new color setup mixing input color with original alphas
		ColorData newColorData = new ColorData();

		// Rays
		newColorData.raysBasicColor = Colors.WithAlpha(_color * 0.5f, originalColors.raysBasicColor.a);	// Same color, but darker
		newColorData.raysSaturatedColor = Colors.WithAlpha(_color, originalColors.raysSaturatedColor.a);
		newColorData.raysStartColorMax = Colors.WithAlpha(_color, originalColors.raysStartColorMax.a);

		// Glow
		newColorData.glowStartColor = Colors.WithAlpha(_color, originalColors.glowStartColor.a);

		// Sparks: Adding some variation
		newColorData.sparksStartColorMin = Colors.WithAlpha(_color * 0.8f, originalColors.sparksStartColorMin.a);
		newColorData.sparksStartColorMax = Colors.WithAlpha(_color * 1.2f, originalColors.sparksStartColorMax.a);

		// Apply!
		Apply(newColorData);
	}

	/// <summary>
	/// Reset back to the original colors.
	/// </summary>
	public void ResetColor() {
		Apply(originalColors);
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the given color setup.
	/// <param name="_colorData">Color setup to be applied.</param>
	/// </summary>
	private void Apply(ColorData _colorData) {
		// Aux vars
		ParticleSystemRenderer psRenderer = null;
		ParticleSystem.MainModule mainModule;
		ParticleSystem.MinMaxGradient colorGradient;

		// Rays: Material-based color
		psRenderer = m_raysPS.GetComponent<ParticleSystemRenderer>();
		if(psRenderer != null) {
			psRenderer.material.SetColor("_BasicColor", _colorData.raysBasicColor);
			psRenderer.material.SetColor("_SaturatedColor", _colorData.raysSaturatedColor);
		}

		// Rays: Start color
		mainModule = m_raysPS.main;
		colorGradient = mainModule.startColor;
		colorGradient.colorMax = _colorData.raysStartColorMax;
		mainModule.startColor = colorGradient;

		// Glow: Start color
		mainModule = m_glowPS.main;
		colorGradient = mainModule.startColor;
		colorGradient.color = _colorData.glowStartColor;
		mainModule.startColor = colorGradient;

		// Sparks: Start color
		mainModule = m_sparksPS.main;
		colorGradient = mainModule.startColor;
		colorGradient.colorMin = _colorData.sparksStartColorMin;
		colorGradient.colorMax = _colorData.sparksStartColorMax;
		mainModule.startColor = colorGradient;
	}

	/// <summary>
	/// Backup the original colors of the effect.
	/// </summary>
	private void AcquireOriginalColors() {
		m_originalColors = new ColorData();

		// Rays: Material-based color
		ParticleSystemRenderer raysRenderer = m_raysPS.GetComponent<ParticleSystemRenderer>();
		if(raysRenderer != null) {
			m_originalColors.raysBasicColor = raysRenderer.material.GetColor("_BasicColor");
			m_originalColors.raysSaturatedColor = raysRenderer.material.GetColor("_SaturatedColor");
		}

		// Rays: Start color
		m_originalColors.raysStartColorMax = m_raysPS.main.startColor.colorMax;

		// Glow: Start color
		m_originalColors.glowStartColor = m_glowPS.main.startColor.color;

		// Sparks: Start color
		m_originalColors.sparksStartColorMin = m_sparksPS.main.startColor.colorMin;
		m_originalColors.sparksStartColorMax = m_sparksPS.main.startColor.colorMax;
	}
}