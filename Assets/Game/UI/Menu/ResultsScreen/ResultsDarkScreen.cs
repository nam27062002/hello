// ResultsDarkScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class ResultsDarkScreen : UbiBCN.SingletonMonoBehaviour<ResultsDarkScreen> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string PREFAB_PATH = "UI/Common/PF_CameraDarkScreen";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Color m_color = Colors.WithAlpha(Color.black, 0.8f);
	[SerializeField] private float m_distance = 50f;
	[SerializeField] private float m_fadeDuration = 0.25f;

	// Internal
	// A single dark screen shared among all cameras
	private static MeshRenderer m_screen = null;
	private static MeshRenderer screen {
		get {
			// If instance is not created, do it now!
			if(m_screen == null) {
				// No! Do it now
				GameObject screenPrefab = Resources.Load<GameObject>(PREFAB_PATH);
				GameObject screenInstance = GameObject.Instantiate<GameObject>(screenPrefab);
				screenInstance.hideFlags = HideFlags.DontSave;
				m_screen = screenInstance.GetComponent<MeshRenderer>();

				// Move to target camera's hierarchy
				instance.LinkToCamera();
			}
			return m_screen;
		}
	}

	private static Camera m_targetCamera = null;
	public static Camera targetCamera {
		get {
			return m_targetCamera == null ? Camera.main : m_targetCamera;
		}

		set { 
			m_targetCamera = value;
			instance.LinkToCamera();
		}
	}

	//------------------------------------------------------------------------//
	// SINGLETON METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Toggle the dark screen on.
	/// </summary>
	/// <param name="_animate">Fade animation?</param>
	public static void Show(bool _animate = true) {
		Show(instance.m_color, instance.m_distance, _animate ? instance.m_fadeDuration : -1f);
	}

	/// <summary>
	/// Toggle the dark screen on.
	/// </summary>
	/// <param name="_animate">Fade animation?</param>
	public static void Show(Color _color, float _distance, float _duration) {
		// Make sure screen is active
		screen.gameObject.SetActive(true);

		// If duration is negative, instantly apply
		if(_duration <= 0f) {
			instance.Apply(_color, _distance);
		} else {
			// Kill existing tween
			screen.DOKill();

			// Tween color
			DOTween.To(
				() => { return screen.material.GetColor("_TintColor"); },
				(Color _newValue) => { instance.ApplyColor(_newValue); },
				_color,_duration
			).SetTarget(screen);

			// Tween distance
			screen.transform.DOLocalMoveZ(_distance, _duration).SetTarget(screen);
		}
	}

	/// <summary>
	/// Toggle the dark screen off.
	/// </summary>
	/// <param name="_animate">Fade animation?.</param>
	public static void Hide(bool _animate = true) {
		Hide(_animate ? instance.m_fadeDuration : -1f);
	}

	/// <summary>
	/// Toggle the dark screen off.
	/// </summary>
	/// <param name="_animate">Fade animation?</param>
	public static void Hide(float _duration) {
		// Figure out target color
		Color targetColor = Colors.WithAlpha(screen.material.GetColor("_TintColor"), 0f);

		// If duration is negative, instantly apply
		if(_duration <= 0f) {
			instance.Apply(targetColor, instance.m_distance);
			screen.gameObject.SetActive(false);
		} else {
			// Kill existing tween
			screen.DOKill();

			// Tween color
			DOTween.To(
				() => { return screen.material.GetColor("_TintColor"); },
				(Color _newValue) => { instance.ApplyColor(_newValue); },
				targetColor,_duration
			)
			.SetTarget(screen)
		 	.OnComplete(
				() => { screen.gameObject.SetActive(false); }
			);
		}
	}

	/// <summary>
	/// Set the dark screen visibility.
	/// </summary>
	/// <param name="_show">Whether to show or hide the dark screen.</param>
	/// <param name="_animate">Fade animation?</param>
	public static void Set(bool _show, bool _animate = true) {
		if(_show) {
			Show(_animate);
		} else {
			Hide(_animate);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Attach screen instance to the target camera hierarchy.
	/// </summary>
	private void LinkToCamera() {
		// If screen is not instantiated, do nothing
		if(m_screen == null) return;

		// Move to target camera's hierarchy
		m_screen.transform.SetParent(targetCamera.transform, false);

		// Set target distance
		m_screen.transform.SetLocalPosZ(m_distance);

		// Link screen to the camera
		m_screen.GetComponent<AdjustDarkScreenToViewport>().targetCamera = targetCamera;
	}

	/// <summary>
	/// Apply the given setup to the screen.
	/// </summary>
	/// <param name="_color">Target color.</param>
	/// <param name="_distance">Target distance.</param>
	private void Apply(Color _color, float _distance) {
		// If screen instance is not valid, do nothing
		if(m_screen == null) return;

		// Use internal apply methods
		ApplyColor(_color);
		ApplyDistance(_distance);
	}

	/// <summary>
	/// Apply the given color to the screen.
	/// </summary>
	/// <param name="_color">Target color.</param>
	private void ApplyColor(Color _color) {
		// Because of the algorithm used by the shader, alpha needs to be corrected
		m_screen.material.SetColor("_TintColor", Colors.WithAlpha(_color, _color.a * 0.5f));
	}

	/// <summary>
	/// Apply the given distance to the screen.
	/// </summary>
	/// <param name="_distance">Target distance.</param>
	private void ApplyDistance(float _distance) {
		m_screen.transform.SetLocalPosZ(_distance);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}