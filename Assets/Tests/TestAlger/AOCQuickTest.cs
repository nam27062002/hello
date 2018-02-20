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
	private static readonly Color PRE_SHOW_COLOR = new Color(1f, 1f, 0.25f);
	private static readonly Color PRE_SHOW_AFTER_DELAY_COLOR = new Color(1f, 1f, 0.5f);
	private static readonly Color POST_SHOW_COLOR = new Color(1f, 1f, 0.75f);
	private static readonly Color PRE_HIDE_COLOR = new Color(1f, 0.75f, 0.75f);
	private static readonly Color POST_HIDE_COLOR = new Color(1f, 0.5f, 0.5f);

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public Transform t;
	public DG.Tweening.Sequence seq;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		ShowHideAnimator anim = this.GetComponent<ShowHideAnimator>();
		if(anim != null) {
			anim.OnShowPreAnimation.AddListener(
				(ShowHideAnimator _anim) => {
					DebugUtils.Log(PRE_SHOW_COLOR.Tag("PRE_SHOW"), _anim);
				}
			);

			anim.OnShowPreAnimationAfterDelay.AddListener(
				(ShowHideAnimator _anim) => {
					DebugUtils.Log(PRE_SHOW_AFTER_DELAY_COLOR.Tag("PRE_SHOW_AFTER_DELAY"), _anim);
				}
			);

			anim.OnShowPostAnimation.AddListener(
				(ShowHideAnimator _anim) => {
					DebugUtils.Log(POST_SHOW_COLOR.Tag("POST_SHOW"), _anim);
				}
			);

			anim.OnHidePreAnimation.AddListener(
				(ShowHideAnimator _anim) => {
					DebugUtils.Log(PRE_HIDE_COLOR.Tag("PRE_HIDE"), _anim);
				}
			);

			anim.OnHidePostAnimation.AddListener(
				(ShowHideAnimator _anim) => {
					DebugUtils.Log(POST_HIDE_COLOR.Tag("POST_HIDE"), _anim);
				}
			);
		}
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
		if(seq != null) {
			seq.Complete();
			seq.Kill();
			seq = null;
		}

		seq = DOTween.Sequence()
			.SetAutoKill(false)
			.OnStepComplete(() => { Debug.Log("SEQUENCE COMPLETED"); })
			.SetUpdate(UpdateType.Normal, false);

		float duration = 5f;
		seq.Join(t.DOScale(2f, duration));
		seq.Join(t.DOLocalMoveX(50f, duration));

		seq.PrependCallback(() => {
			Debug.Log("SEQUENCE STARTED");
		});

		seq.Pause();

		// Insert delay at the beginning of the sequence
		//seq.PrependInterval(2);
	}

	public void OnTestButton2() {
		seq.Goto(0f);
		seq.PlayForward();
	}

	public void OnTestButton3() {
		seq.Goto(1f);
		seq.PlayBackwards();
	}

	public void OnTestButton4() {
		if(seq.isBackwards) {
			seq.PlayForward();
		} else {
			seq.PlayBackwards();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {

	}

	private void ReadProperties() {
		
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

}