// NumberTextAnimator.cs
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Animate a textfield by interpolating between two numbers.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class NumberTextAnimator : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] protected float m_duration = 0.5f;
	public float duration {
		get { return m_duration; }
		set { m_duration = value; }
	}

	// Custom actions and events
	[SerializeField] protected Action<NumberTextAnimator> m_customTextSetter = null;
	public Action<NumberTextAnimator> CustomTextSetter {
		get { return m_customTextSetter; }
		set { m_customTextSetter = value; }
	}

	[SerializeField] protected UnityEvent m_onFinished = new UnityEvent();
	public UnityEvent OnFinished {
		get { return m_onFinished; }
	}

	// References
	protected TextMeshProUGUI m_targetTxt;
	public TextMeshProUGUI text {
		get { return m_targetTxt; }
	}

	// Value tracking
	protected long m_initialValue = 0;
	public long initialValue {
		get { return m_initialValue; }
	}

	protected long m_finalValue = 0;
	public long finalValue {
		get { return m_finalValue; }
	}

	protected long m_currentValue = 0;
	public long currentValue {
		get { return m_currentValue; }
	}

	// Internal logic
	protected float m_startTime = 0;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Get target text component if not yet initialized
		if(m_targetTxt == null) {
			m_targetTxt = gameObject.GetComponent<TextMeshProUGUI>();	// Should have one, since we're using the [RequireComponent] tag
		}

		// Set initial value
		SetValue(m_currentValue, false);
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);

		// Make sure text is properly set (in case language changed while disabled)
		ApplyValue(m_currentValue);
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}
	
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.LANGUAGE_CHANGED:
            {
                OnLanguageChanged();
            }break;
        }
    }
    
	/// <summary>
	/// Called every frame
	/// </summary>
	void Update() {
		// Skip if we've reached the final value
		if(m_currentValue != m_finalValue) {
			// Update current value
			long newValue = (long)Mathf.SmoothStep(m_initialValue, m_finalValue, (Time.unscaledTime - m_startTime)/m_duration);

			// If value has changed, update textfield
			if(newValue != m_currentValue) {
				m_currentValue = newValue;
				ApplyValue(newValue);
			}
		}
	}

	/// <summary>
	/// Animate the textfield to interpolate using these parameters.
	/// </summary>
	/// <param name="_initialValue">Initial value of the textfield.</param>
	/// <param name="_finalValue">Final value of the textfield.</param>
	public void SetValue(long _initialValue, long _finalValue) {
		// Store parameters
		m_initialValue = _initialValue;
		m_finalValue = _finalValue;

		// Reset timestamp and current value
		m_currentValue = m_initialValue;
		m_startTime = Time.unscaledTime;

		// Initialize textfield
		ApplyValue(m_initialValue);
	}

	/// <summary>
	/// Alternative version, current value will be used as starting value.
	/// </summary>
	/// <param name="_finalValue">Final value of the textfield.</param>
	/// <param name="_animate">Whether to animate or not.</param>
	public void SetValue(long _finalValue, bool _animate) {
		// Call the other version
		// Animate? Start at current value, otherwise force same values as initial and final
		if(_animate) {
			SetValue(m_currentValue, _finalValue);
		} else {
			SetValue(_finalValue, _finalValue);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the given value to the textfield. That way we make sure formatting is always respected.
	/// </summary>
	/// <param name="_value">The value to be applied.</param>
	protected void ApplyValue(long _value) {
		// Get target text component if not yet initialized
		if(m_targetTxt == null) {
			m_targetTxt = gameObject.GetComponent<TextMeshProUGUI>();	// Should have one, since we're using the [RequireComponent] tag
		}

		// If a custom function is defined, invoke it
		// Otherwise just use default formatting
		if(m_customTextSetter != null) {
			m_customTextSetter(this);
		} else {
			m_targetTxt.text = FormatValue(_value);
		}

		// If it's the target value, notify listeners
		if(_value == m_finalValue) {
			OnFinished.Invoke();
		}
	}

	/// <summary>
	/// Format a given value into the desired format.
	/// </summary>
	/// <returns>The formatted string.</returns>
	/// <param name="_value">The value to be formatted.</param>
	protected virtual string FormatValue(long _value) {
		return StringUtils.FormatNumber(_value);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Localization language has changed, update textfield.
	/// </summary>
	protected void OnLanguageChanged() {
		// Update with current value
		ApplyValue(m_currentValue);
	}
}
