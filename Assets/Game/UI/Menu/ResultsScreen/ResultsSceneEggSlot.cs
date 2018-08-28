// ResultsSceneEggSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Egg slot controller in the results scene.
/// </summary>
public class ResultsSceneEggSlot : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private MenuEggLoader m_eggLoader = null;
	public MenuEggLoader eggLoader {
		get { return m_eggLoader; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If the egg view hasn't been loaded, do it now!
		if(m_eggLoader.eggView == null) {
			m_eggLoader.Reload();
		}

		// We want to show all the FX in the results screen :)
		if(m_eggLoader.eggView != null) {
			m_eggLoader.eggView.forceIdleFX = true;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch the animation for the results screen.
	/// </summary>
	public void LaunchResultsAnim() {
		// Precompute durations (must add up to 1f)
		float eggAnimDuration = UIConstants.resultsEggDuration;
		float inDelay = eggAnimDuration * 0.1f;
		float inDuration = eggAnimDuration * 0.7f;
		float outDuration = eggAnimDuration * 0.2f;

		// Initialize
		float eggSlotScale = m_eggLoader.transform.localScale.x;
		m_eggLoader.gameObject.SetActive(true);
		m_eggLoader.transform.SetLocalScale(0f);

		// Stop particles during scale-up animation (they don't mix well with scale changes :s)
		ParticleSystem[] particles = m_eggLoader.eggView.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < particles.Length; ++i) {
			particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}

		// Launch sequence!
		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(inDelay);	// Initial delay

		// Up
		seq.Append(m_eggLoader.transform.DOScale(eggSlotScale, inDuration * 0.9f).SetEase(Ease.OutBack));
		seq.Join(m_eggLoader.transform.DOLocalMoveY(0.25f, inDuration * 0.9f).SetRelative(true).SetEase(Ease.OutBack));

		// Restore particles and make sure scale is ok
		seq.AppendCallback(() => {
			for(int i = 0; i < particles.Length; ++i) {
				particles[i].Play(true);
			}

			ParticleScaler scaler = m_eggLoader.GetComponentInChildren<ParticleScaler>();
			if(scaler != null) scaler.DoScale();
		});

		// Down
		seq.Append(m_eggLoader.transform.DOLocalMoveY(-0.25f, outDuration).SetRelative(true).SetEase(Ease.OutQuad));

		// Rotation
		seq.Insert(inDelay, m_eggLoader.transform.DOBlendableLocalRotateBy(Vector3.up * 360f, inDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));

		// Done!
		seq.Play();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}