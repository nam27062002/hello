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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public Ease m_ease = Ease.InOutCubic;
	public GameObject m_target = null;
	public TextMeshProUGUI m_text = null;

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
	public void OnTestButton() {
		DOTween.Kill("AOCTEST", true);
		m_target.transform.DOKill(true);

		Sequence seq = DOTween.Sequence()
			.AppendInterval(0.5f)
			.Append(m_target.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack))
			//.Join(m_target.transform.DOLocalRotate(m_target.transform.localRotation.eulerAngles + Vector3.up * 360f, 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetDelay(0.5f).SetRecyclable(true).SetAutoKill(false))
			.Join(DOVirtual.DelayedCall(
				0.5f,
				() => {
					m_target.transform.DOLocalRotate(m_target.transform.localRotation.eulerAngles + Vector3.up * 360f, 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetRecyclable(true);
				},
				false
			))
			.AppendInterval(1f)
			.Append(m_target.transform.DOScale(2f, 0.25f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutBack))
			.AppendCallback(() => { 
				m_text.text = "Sequence ended";
			})
			.SetId("AOCTEST");
		seq.OnUpdate(() => {
					m_text.text = seq.Elapsed().ToString("#.##") + " (" + seq.ElapsedPercentage().ToString("#.##") + ")\n" 
						+ seq.Elapsed(false).ToString("#.##") + " (" + seq.ElapsedPercentage(false).ToString("#.##") + ")";
		});
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}