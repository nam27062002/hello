// DOTweenElasticityTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
public class DOTweenElasticityTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public RectTransform[] targets;
	public RectTransform[] targets2;
	public Ease ease = Ease.Linear;
	public float duration = 1f;
	public float offset = 1000f;
	public Range overshootOrAmplitude = new Range(DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEaseOvershootOrAmplitude);
	public Range period = new Range(DOTween.defaultEasePeriod, DOTween.defaultEasePeriod);

	private DeltaTimer timer = new DeltaTimer();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	public void RunTest() {
		for(int i = 0; i < targets.Length; i++) {
			// Kill any active tween on this object
			DOTween.Kill(targets[i], true);

			// Reset pos
			Vector2 pos = targets[i].anchoredPosition;
			pos.x = 0f;
			targets[i].anchoredPosition = pos;

			// Launch new tween with target params
			float delta = (float)i/(float)(targets.Length - 1);
			Tweener t = targets[i].DOAnchorPosX(offset, duration).SetEase(ease);
			t.easeOvershootOrAmplitude = overshootOrAmplitude.Lerp(delta);	// Excluding ourselves
			t.easePeriod = period.Lerp(delta);
			//t.OnComplete(() => { t.Rewind(); });
		}

		for(int i = 0; i < targets2.Length; i++) {
			// Reset pos
			Vector2 pos = targets2[i].anchoredPosition;
			pos.x = 0f;
			targets2[i].anchoredPosition = pos;
		}
		timer.Start(duration * 1000f);
	}

	public void Update() {
		if(!timer.IsFinished()) {
			for(int i = 0; i < targets2.Length; i++) {
				float delta = timer.GetDelta(CustomEase.EaseType.elasticOut_01);
				float x = Mathf.Lerp(0f, offset, delta);
				Debug.Log("Delta: " + delta + "\nX: " + x + "\nTime: " + timer.GetTime());
				Vector2 pos = targets2[i].anchoredPosition;
				pos.x = x;
				targets2[i].anchoredPosition = pos;
			}
		}
	}
}