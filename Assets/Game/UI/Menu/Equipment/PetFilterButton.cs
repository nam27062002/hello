// PetScreenFilterButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to manage pets screen's filters.
/// </summary>
[RequireComponent(typeof(Toggle))]
[ExecuteInEditMode]
public class PetFilterButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public class PetFilterButtonEvent : UnityEvent<PetFilterButton> { }
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UIColorFX m_iconColorFX = null;
	[SerializeField] private string m_filterName = "";
	public string filterName {
		get { return m_filterName; }
		set { m_filterName = value; }
	}

	[Space]
	[SerializeField] private UIColorFX.Setup m_enabledFX = new UIColorFX.Setup(Color.white, Colors.transparentBlack, 1f, 0f, 0f, 0f);
	[SerializeField] private UIColorFX.Setup m_disabledFX = new UIColorFX.Setup(Color.white, Colors.transparentBlack, 0.4f, -0.25f, -0.25f, -0.25f);

	// Events
	public PetFilterButtonEvent OnFilterToggled = new PetFilterButtonEvent();

	// Internal refs
	private Toggle m_toggle = null;
	public Toggle toggle {
		get {
			if(m_toggle == null) {
				m_toggle = GetComponent<Toggle>();
			}
			return m_toggle;
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Apply initial state
		RefreshVisuals();

		// Subscribe to change event
		toggle.onValueChanged.AddListener(OnToggleValueChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected void OnDisable() {
		// Unsubscribe from change event
		toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
	}

	/// <summary>
	/// Something has changed on the inspector.
	/// </summary>
	protected void OnValidate() {
		// Refresh!
		RefreshVisuals();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Update visuals based on current toggle state.
	/// </summary>
	private void RefreshVisuals() {
		// Is FX object valid?
		if(!m_iconColorFX) return;

		// Apply proper FX setup
		if(toggle.isOn) {
			m_iconColorFX.Apply(m_enabledFX);
		} else {
			m_iconColorFX.Apply(m_disabledFX);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The toggle's value has changed.
	/// </summary>
	/// <param name="_value">New value.</param>
	private void OnToggleValueChanged(bool _value) {
		// Update visuals
		RefreshVisuals();

		// Notify listeners
		OnFilterToggled.Invoke(this);
	}
}