// CPNumericValueEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// UI controller for a numeric value in the Control Panel.
/// </summary>
public class CPNumericValueEditor : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public class NumericValueEvent : UnityEvent<float> { }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private TMP_InputField m_valueInput = null;
	public TMP_InputField valueInput {
		get { return m_valueInput; }
	}

	[SerializeField] private Button m_addButton = null;
	public Button addButton {
		get { return m_addButton; }
	}

	[SerializeField] private Button m_removeButton = null;
	public Button removeButton {
		get { return m_removeButton; }
	}

	[SerializeField] private Button m_setButton = null;
	public Button setButton {
		get { return m_setButton; }
	}

	// Setup
	[SerializeField] private float m_step = 10f;
	public float step {
		get { return m_step; }
	}

	[SerializeField] private Range m_range = new Range(0f, 100000f);
	public Range range {
		get { return m_range; }
	}

	// Events
	[SerializeField] public NumericValueEvent OnValueChanged = new NumericValueEvent();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void Awake() {
		// Change button's labels
		if(m_addButton != null) {
			TextMeshProUGUI addText = m_addButton.GetComponentInChildren<TextMeshProUGUI>();
			if(addText != null) addText.text = "+" + m_step;
		}

		if(m_removeButton != null) {
			TextMeshProUGUI removeText = m_removeButton.GetComponentInChildren<TextMeshProUGUI>();
			if(removeText != null) removeText.text = "-" + m_step;
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to button events
		if(m_addButton != null) m_addButton.onClick.AddListener(OnAddButton);
		if(m_removeButton != null) m_removeButton.onClick.AddListener(OnRemoveButton);
		if(m_setButton != null) m_setButton.onClick.AddListener(OnSetButton);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from button events
		if(m_addButton != null) m_addButton.onClick.RemoveListener(OnAddButton);
		if(m_removeButton != null) m_removeButton.onClick.RemoveListener(OnRemoveButton);
		if(m_setButton != null) m_setButton.onClick.RemoveListener(OnSetButton);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the current value.
	/// </summary>
	/// <returns></returns>
	public float GetValue() {
		// Get amount from linked input field
		float amount = 0f;
		if(!float.TryParse(m_valueInput.text, NumberStyles.Any, CultureInfo.InvariantCulture, out amount)) {
			ControlPanel.LaunchTextFeedback("Invalid number format!!", Color.red);
			return float.MinValue;
		}

		// Clamp value and return
		return m_range.Clamp(amount);
	}

	/// <summary>
	/// Set value. Will be clamped to the range.
	/// </summary>
	/// <param name="_newValue">The value to be set.</param>
	/// <param name="_triggerEvents">Whether to trigger the OnValueChanged event or not.</param>
	public void SetValue(float _newValue, bool _triggerEvents = false) {
		// Clamp value
		float clampedValue = m_range.Clamp(_newValue);

		// Update textfield
		m_valueInput.text = clampedValue.ToString();

		// Toggle buttons interactability
		if(m_addButton != null) {
			m_addButton.interactable = _newValue < m_range.max;
		}

		if(m_removeButton != null) {
			m_removeButton.interactable = _newValue > m_range.min;
		}

		// Notify listeners
		if(_triggerEvents) {
			OnValueChanged.Invoke(clampedValue);
		}
	}

	/// <summary>
	/// Read value from input field and performs the required operation.
	/// </summary>
	/// <param name="_operation">Target operation.</param>
	private void SetValue(CPOperation _operation) {
		// Get amount from linked input field
		float amount = 0f;
		if(!float.TryParse(m_valueInput.text, NumberStyles.Any, CultureInfo.InvariantCulture, out amount)) {
			ControlPanel.LaunchTextFeedback("Invalid number format!!", Color.red);
			return;
		}

		// Compute amount to add
		switch(_operation) {
			case CPOperation.ADD: {
				amount += m_step;
			} break;

			case CPOperation.REMOVE: {
				amount -= m_step;
			} break;

			case CPOperation.SET: {
				amount = m_range.Clamp(amount);
			} break;
		}

		// Update textfield
		SetValue(amount, true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Button callbacks.
	/// </summary>
	private void OnAddButton() { SetValue(CPOperation.ADD); }
	private void OnRemoveButton() { SetValue(CPOperation.REMOVE); }
	private void OnSetButton() { SetValue(CPOperation.SET); }
}