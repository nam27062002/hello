// PopupTiltControl.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tilt-control calibration popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTiltControl : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupTiltControl";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private Slider m_sensitivitySlider = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Initialize slider to current sensitivity value
		m_sensitivitySlider.value = GameSettings.tiltControlSensitivity;
	}

	/// <summary>
	/// Sensitivity slider has changed.
	/// </summary>
	/// <param name="_value">New value.</param>
	public void OnSensitivityValueChanged(float _value) {
		// Just set new value
		GameSettings.tiltControlSensitivity = _value;
	}

	/// <summary>
	/// Calibrate button has been pressed.
	/// </summary>
	public void OnCalibrateButton() {
		// Open calibration popup
		PopupManager.OpenPopupInstant(PopupTiltCalibrationAnim.PATH);

		// Popup is actually fake, do the calibration now!
		Messenger.Broadcast(GameEvents.TILT_CONTROL_CALIBRATE);
	}
}