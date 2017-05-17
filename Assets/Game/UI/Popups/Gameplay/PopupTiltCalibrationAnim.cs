// PopupTiltCalibrationAnim.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple popup to show some feedback while the tilt control is being calibrated.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTiltCalibrationAnim : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupTiltCalibrationAnim";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private GameObject m_targetObj = null;
	[SerializeField] private GameObject m_animatedObj = null;
	[SerializeField] private Localizer m_calibratingInfoText = null;

	// Setup
	[Space]
	[SerializeField] private float m_initialDelay = 1f;
	[SerializeField] private float m_totalDuration = 3f;
	[SerializeField] private Ease m_ease = Ease.Linear;

	[Space]
	[SerializeField] private Sprite m_targetSprite = null;
	[SerializeField] private Sprite m_animatedSprite = null;

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

	/// <summary>
	/// A change has occurred on the inspector.
	/// </summary>
	private void OnValidate() {
		m_totalDuration = Mathf.Max(1f, m_totalDuration);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Aux vars
		AnimationCurve flashEaseCurve = new AnimationCurve();
		flashEaseCurve.AddKey(0f, 0f);
		flashEaseCurve.AddKey(0.25f, 1f);
		flashEaseCurve.AddKey(1f, 0f);

		RectTransform animatedRt = (RectTransform)m_animatedObj.transform;
		RectTransform targetRt = (RectTransform)m_targetObj.transform;

		// Setup animation
		if(m_animatedSprite != null) m_animatedObj.GetComponent<Image>().sprite = m_animatedSprite;
		if(m_targetSprite != null) m_targetObj.GetComponent<Image>().sprite = m_targetSprite;

		// Create sequence
		Sequence sq = DOTween.Sequence()
			.SetUpdate(UpdateType.Normal, true);

		// Initial delay
		sq.AppendInterval(m_initialDelay);

		// Slowly close animated circle
		sq.Append(
			animatedRt.DOSizeDelta(Vector2.one * 1100f, m_totalDuration)
			.From()
			.SetEase(m_ease)
		);

		sq.Join(
			m_animatedObj.transform.DOLocalRotate(new Vector3(0f, 0f, 180f), m_totalDuration)
			.From()
			.SetEase(m_ease)
		);

		// Slowly saturate target circle
		sq.Join(
			m_targetObj.GetComponent<UIColorFX>().DOSaturation(-1f, m_totalDuration)
			.From()
		);

		sq.Join(
			m_targetObj.transform.DOLocalRotate(new Vector3(0f, 0f, 180f), m_totalDuration, RotateMode.FastBeyond360)
			.From()
			.SetEase(m_ease)
		);

		// Fade by the end of the animation
		sq.Insert(
			m_totalDuration * 0.75f,
			m_animatedObj.GetComponent<Image>().DOFade(0f, m_totalDuration * 0.25f)
		);

		// Once finished, trigger flash animation (Scale + Brightness)
		float flashAnimStartTime = m_totalDuration * 0.9f;
		sq.Insert(
			flashAnimStartTime,
			m_targetObj.transform.DOScale(1.5f, 0.4f)
			.SetEase(flashEaseCurve)
		);

		sq.Insert(
			flashAnimStartTime,
			m_targetObj.GetComponent<UIColorFX>().DOBrightness(0.5f, 0.4f)
			.SetEase(flashEaseCurve)
		);

		sq.InsertCallback(
			flashAnimStartTime,
			() => m_calibratingInfoText.Localize("TID_CALIBRATION_DONE")
		);

		// Once completed, close popup
		sq.OnComplete(() => { GetComponent<PopupController>().Close(true); });

		// Launch animation!
		sq.Play();
	}
}