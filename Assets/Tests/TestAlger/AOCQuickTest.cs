// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	[Serializable]
	public class EaseCurve {
		public Ease easeType;
		public AnimationCurve curve = new AnimationCurve();
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public List<EaseCurve> m_curves = new List<EaseCurve>();
	public float m_overshoot = 1.7f;
	public float m_amplitude = 1f;
	public float m_period = 1f;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnClick() {
		/*float duration = 1f;
		float offsetX = 1000f;
		for(int i = 0; i < 5; i++) {
			GameObject go = GameObject.Find("Test" + i);
			DOTween.Kill(go);
			go.transform.SetLocalPosX(0f);
			//go.transform.DOLocalMoveX(offsetX, duration).SetEase(Ease.OutElastic, (float)(i + 1));
			//go.transform.DOLocalMoveX(offsetX, duration).SetEase(Ease.OutElastic, (float)(i + 1), (float)(i + 1));
			//go.transform.DOLocalMoveX(offsetX, duration).SetEase(Ease.OutElastic, (float)(i + 1), 1f);
			go.transform.DOLocalMoveX(offsetX, duration).SetEase(Ease.OutQuart, 1f, (float)(i + 1) * 10f);
		}*/

		int samples = 100;
		m_curves = new List<EaseCurve>();
		int easeCount = Enum.GetNames(typeof(Ease)).Length;
		for(int i = 0; i < easeCount; i++) {
			EaseCurve curve;
			if(i < m_curves.Count) {
				curve = m_curves[i];
				curve.curve.keys = new Keyframe[0];
			} else {
				curve = new EaseCurve();
				curve.easeType = (Ease)i;
				m_curves.Add(curve);
			}

			float value = 0f;
			//Tweener t = DOTween.To(x => value = x, 0f, 1f, 1f).SetEase(curve.easeType, m_overshoot);
			Tweener t = DOTween.To(x => value = x, 0f, 1f, 1f).SetEase(curve.easeType, m_amplitude, m_period);
			for(int j = 0; j < samples; j++) {
				float delta = (float)j/(float)samples;
				t.Goto(delta);
				curve.curve.AddKey(new Keyframe(delta, value));
			}
			DOTween.Kill(value);
		}
	}
}