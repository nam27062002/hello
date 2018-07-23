﻿// DragonControlPlayer.cs
// Hungry Dragon
// 
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class DragonControlPlayer : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[HideInInspector] public bool moving = false;
	[HideInInspector] public bool action = false;

	TouchControlsDPad	touchControls = null;

	JoystickControls joystickControls = null;
	bool joystickMoving = false;

	TiltControls tiltControls = null;
	bool tiltMoving = false;

	bool m_useTiltControl = false;

	private Assets.Code.Game.Spline.BezierSpline m_followingSpline;
	private int m_testDirecton = 1;

	public static float BOOST_WITH_HARD_PUSH_DEFAULT_THRESHOLD = 0.85f;
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		GameObject gameInputObj = GameObject.Find("PF_GameInput");
		if(gameInputObj != null) {
			touchControls = gameInputObj.GetComponent<TouchControlsDPad>();
			if (touchControls != null){
				touchControls.Set3DTouch( Prefs.GetBoolPlayer(GameSettings.TOUCH_3D_ENABLED) , BOOST_WITH_HARD_PUSH_DEFAULT_THRESHOLD);
			}
			tiltControls = gameInputObj.GetComponent<TiltControls>();
			if(tiltControls != null) {
				tiltControls.Calibrate();
				tiltControls.SetSensitivity(GameSettings.tiltControlSensitivity);
			}
			joystickControls = gameInputObj.GetComponent<JoystickControls>();
		}

		if(ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST) {           
            m_followingSpline = GameObjectExt.FindComponent<Assets.Code.Game.Spline.BezierSpline>(true, "TestPath");            	
		}
		m_useTiltControl = GameSettings.Get(GameSettings.TILT_CONTROL_ENABLED);
		SetupInputs();

		// Subscribe to external events
		Messenger.AddListener<string, bool>(MessengerEvents.GAME_SETTING_TOGGLED, OnGameSettingsToggled);
		Messenger.AddListener(MessengerEvents.TILT_CONTROL_CALIBRATE, OnTiltCalibrate);
		Messenger.AddListener<float>(MessengerEvents.TILT_CONTROL_SENSITIVITY_CHANGED, OnTiltSensitivityChanged);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {
		Messenger.RemoveListener<string, bool>(MessengerEvents.GAME_SETTING_TOGGLED, OnGameSettingsToggled);
		Messenger.RemoveListener(MessengerEvents.TILT_CONTROL_CALIBRATE, OnTiltCalibrate);
		Messenger.RemoveListener<float>(MessengerEvents.TILT_CONTROL_SENSITIVITY_CHANGED, OnTiltSensitivityChanged);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	void OnEnable() {
		SetupInputs();
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		if(ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST && m_followingSpline != null) {
			moving = true;
			action = true;
			return;
		}

		moving = false;
		action = false;

		// [AOC] Nothing to do if paused
		// if(InstanceManager.gameSceneControllerBase.paused) return;

		if(!m_useTiltControl) {
			if(touchControls != null) {
				touchControls.UpdateTouchControls();

				moving = touchControls.CurrentTouchState != TouchState.none;
				action = touchControls.touchAction;
			}	
		}
		else if(tiltControls != null) {
			tiltControls.UpdateTiltControls();
			tiltMoving = tiltControls.IsMoving();
			moving = moving || tiltMoving;
			action = action || tiltControls.getAction();
		}

		#if UNITY_EDITOR
		if(joystickControls != null) {
			joystickControls.UpdateJoystickControls();
			joystickMoving = joystickControls.isMoving();
			moving = moving || joystickMoving;
			action = action || joystickControls.getAction();
		}
		#endif
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="desiredVelocity"></param>
	/// <param name="impulse"></param>
	public void GetImpulse(float desiredVelocity, ref Vector3 impulse) {

		impulse = Vector3.zero;

		if (!enabled) return;

		// if app mode is test -> input something else?
		if(ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST && m_followingSpline != null) {
			float m_followingClosestT;
			int m_followingClosestStep;
			m_followingSpline.GetClosestPointToPoint(transform.position, 100, out m_followingClosestT, out m_followingClosestStep);
			if((m_followingClosestT >= 1.0f && m_testDirecton > 0) || (m_followingClosestT <= 0f && m_testDirecton < 0)) {
				// What to do?
				m_testDirecton *= -1;
			}

			m_followingClosestT += 0.01f * m_testDirecton;

			Vector3 target = m_followingSpline.GetPoint(m_followingClosestT);
			target.z = 0;
			Vector3 move = target - transform.position;
			impulse = move.normalized * desiredVelocity;
			return;
		}	

		#if UNITY_EDITOR
		if(joystickControls != null && joystickMoving) {
			joystickControls.CalcSharkDesiredVelocity(desiredVelocity);
			impulse.x = joystickControls.SharkDesiredVel.x;
			impulse.y = joystickControls.SharkDesiredVel.y;
			impulse.z = 0;
			return;
		}
		#endif
		if(!m_useTiltControl) {
			if(touchControls != null && moving) {
				touchControls.CalcSharkDesiredVelocity(desiredVelocity);
				impulse.x = touchControls.SharkDesiredVel.x;
				impulse.y = touchControls.SharkDesiredVel.y;
				impulse.z = 0;
			}
		}
		else if(tiltControls != null && tiltMoving) {
			tiltControls.CalcSharkDesiredVelocity(desiredVelocity);
			impulse.x = tiltControls.SharkDesiredVel.x;
			impulse.y = tiltControls.SharkDesiredVel.y;
			impulse.z = 0;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	private void SetupInputs() {
		if(touchControls != null) {
			touchControls.enabled = !m_useTiltControl;
			touchControls.SetTouchObjRendering(!m_useTiltControl);
		}

		if(tiltControls != null)
			tiltControls.enabled = m_useTiltControl;

#if UNITY_EDITOR
		if(joystickControls != null)
			joystickControls.enabled = true;
#endif
	}

	void OnDisable() {
		if(touchControls != null) {
			touchControls.SetTouchObjRendering(false);
			touchControls.SetTouch2ObjRendering(false, false);
			touchControls.enabled = false;
		}

		if(tiltControls != null)
			tiltControls.enabled = false;

#if UNITY_EDITOR
		if(joystickControls != null)
			joystickControls.enabled = false;
#endif

		moving = false;
		action = false;

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The game settings has been toggled.
	/// </summary>
	/// <param name="">Toggle on or off?</param>
	private void OnGameSettingsToggled(string _settingId, bool _settingsValue) {
		if(_settingId == GameSettings.TILT_CONTROL_ENABLED) {
			m_useTiltControl = _settingsValue;
			if(tiltControls && _settingsValue) {
				tiltControls.Calibrate();
				tiltControls.SetSensitivity(GameSettings.tiltControlSensitivity);
			}
		}
		else if( _settingId == GameSettings.TOUCH_3D_ENABLED ) {
			if ( touchControls != null ) {
				touchControls.Set3DTouch( Prefs.GetBoolPlayer(GameSettings.TOUCH_3D_ENABLED) , BOOST_WITH_HARD_PUSH_DEFAULT_THRESHOLD);
			}
		}
	}

	/// <summary>
	/// A tilt calibration has been requested.
	/// </summary>
	private void OnTiltCalibrate() {
		// Just propagate to tilt control
		if(tiltControls != null) {
			tiltControls.Calibrate();
		}
	}

	/// <summary>
	/// The tilt sensitivity has changed.
	/// </summary>
	/// <param name="_sensitivity">New tilt sensitivity value [0..1].</param>
	private void OnTiltSensitivityChanged(float _sensitivity) {
		// Recalibrate tilt control
		if(tiltControls != null) {
			tiltControls.SetSensitivity(_sensitivity);
		}
	}
}
