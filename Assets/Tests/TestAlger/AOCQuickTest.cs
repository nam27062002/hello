// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
//[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private AnimationCurve m_flashEaseCurve = new AnimationCurve();

	public UnityEvent m_theEvent = new UnityEvent();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		ScrollRect scrollList = GetComponent<ScrollRect>();
		if(scrollList != null) scrollList.onValueChanged.AddListener(OnScrollListValueChanged);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	/// <summary>
	/// Something changed on the inspector.
	/// </summary>
	private void OnValidate() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		AnimationCurve flashEaseCurve = new AnimationCurve();
		flashEaseCurve.AddKey(0f, 0f);
		flashEaseCurve.AddKey(0.25f, 1f);
		flashEaseCurve.AddKey(1f, 0f);

		UIColorFX colorFX = GetComponent<UIColorFX>();
		DOTween.Sequence()
			.Append(transform.DOLocalMoveY(300f, 0.25f).SetRelative())
			.Append(transform.DOLocalMoveY(-300f, 0.25f).SetRelative())

			.Append(colorFX.DOBrightness(0.5f, 0.5f).SetEase(flashEaseCurve))
			.Join(transform.DOScale(1.25f, 0.5f).SetEase(flashEaseCurve))

			.Play();
	}

	private float FlashEase(float _time, float _duration, float _overshootOrAmplitude, float _period) {
		float delta = _time/_duration;
		return delta;
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void OnAddListenersButton() {
		m_theEvent.AddListener(SampleCallback1);
		m_theEvent.AddListener(SampleCallback2);
		m_theEvent.AddListener(() => { Debug.Log("Inline Callback 1"); });
		m_theEvent.AddListener(() => { Debug.Log("Inline Callback 2"); });
	}

	public void OnTriggerEvent() {
		m_theEvent.Invoke();
	}

	private void SampleCallback1() {
		Debug.Log("Sample Callback 1");
	}

	private void SampleCallback2() {
		Debug.Log("Sample Callback 2");
	}

	private void OnScrollListValueChanged(Vector2 _newValue) {
		string color = "lime";
		if(_newValue.x < Mathf.Epsilon) {
			color = "red";
		} else if(_newValue.x > 1 - Mathf.Epsilon) {
			color = "blue";
		}
		ScrollRect scrollList = this.GetComponent<ScrollRect>();
		Debug.Log("<color=" + color + ">VALUE: (" + _newValue.x + ", " + _newValue.y + ")" + "</color>");
	}
}