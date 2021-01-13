// DragonXPBarSeparator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Just a small separator to split a XP bar into levels.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DragonXPBarSeparator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] protected GameObject m_activeObj = null;
	[SerializeField] protected GameObject m_inactiveObj = null;

	// Public properties
	public RectTransform rectTransform {
		get { return (RectTransform)this.transform; }
	}

	protected float m_delta = 0f;
	public float delta {
		get { return m_delta; }
		set { SetDelta(value); }
	}

	protected Slider m_slider = null;
	public Slider slider {
		get { return m_slider; }
		set { AttachToSlider(value, m_delta); }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure we're properly set up before showing up!
		Refresh();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// De-attach from slider, if any
		if(m_slider != null) {
			m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Attach this separator to a slider.
	/// Subscribes to its OnValueChanged event and places the slider at the target position.
	/// </summary>
	/// <param name="_slider">Slider.</param>
	/// <param name="_delta">Normalized position where to place this separator [0..1].</param>
	public void AttachToSlider(Slider _slider, float _delta) {
		// Update slider ref
		// Skip if already attached to this slider
		if(_slider != m_slider) {
			// De-attach from previous slider, if any
			if(m_slider != null) {
				m_slider.onValueChanged.RemoveListener(OnSliderValueChanged);
			}

			// Attach to new slider, if any
			if(_slider != null) {
				_slider.onValueChanged.AddListener(OnSliderValueChanged);
			}

			// Store new slider
			m_slider = _slider;
		}

		// Update delta
		SetDelta(_delta);
	}

	/// <summary>
	/// Define where this separator is placed.
	/// </summary>
	/// <param name="_delta">Normalized position where to place this separator [0..1].</param>
	public void SetDelta(float _delta) {
		// Store new delta
		m_delta = _delta;

		// Refresh visuals
		Refresh();
	}

	/// <summary>
	/// Show the proper clip based on slider's value and current delta.
	/// Put the separator in position.
	/// </summary>
	public virtual void Refresh() {
		// Disable both clips if slider is not defined
		if(m_slider == null) {
			m_activeObj.SetActive(false);
			m_inactiveObj.SetActive(false);
			return;
		}

		// Put in position
		rectTransform.anchorMin = new Vector2(m_delta, 0f);
		rectTransform.anchorMax = new Vector2(m_delta, 1f);

		// Activate the right clip based on current delta and slider's value
		bool active = m_slider.normalizedValue >= m_delta;
		m_activeObj.SetActive(active);
		m_inactiveObj.SetActive(!active);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The value on the attached slider has changed.
	/// </summary>
	/// <param name="_newValue">The new value of the slider.</param>
	private void OnSliderValueChanged(float _newValue) {
		// Only if active
		if(!isActiveAndEnabled) return;

		// Just refresh visuals
		Refresh();
	}
}
