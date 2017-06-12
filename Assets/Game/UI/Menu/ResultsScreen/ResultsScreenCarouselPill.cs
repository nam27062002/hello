// ResultsScreenCarouselPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Abstract version of the pill, to be inherited.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public abstract class ResultsScreenCarouselPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External References
	private ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get {
			if(m_animator == null) m_animator = GetComponent<ShowHideAnimator>();
			return m_animator;
		}
	}

	// Events
	public UnityEvent OnFinished = new UnityEvent();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Kill any possible active sequence/tween linked to this pill
		DOTween.Kill(this);
	}

	/// <summary>
	/// Initializes, shows and animates the pill.
	/// The <c>OnFinished</c> event will be invoked once the animation has finished.
	/// </summary>
	/// <param name="_delay">Seconds to wait before showing this pill.</param>
	public void ShowAndAnimate(float _delay) {
		// Super easy with DOTween
		CoroutineManager.DelayedCall(StartInternal, _delay, false);
	}

	//------------------------------------------------------------------------//
	// ABSTARCT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this pill must be displayed on the carousel or not.
	/// </summary>
	/// <returns><c>true</c> if the pill must be displayed on the carousel, <c>false</c> otherwise.</returns>
	public abstract bool MustBeDisplayed();

	/// <summary>
	/// Initializes, shows and animates the pill.
	/// The <c>OnFinished</c> event will be invoked once the animation has finished.
	/// </summary>
	protected abstract void StartInternal();

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Mark the pill as finished after some delay.
	/// </summary>
	/// <param name="_delay">Delay in seconds.</param>
	protected void DelayedFinish(float _delay) {
		// Notify finish after some delay
		DOTween.Sequence()
			.SetId(this)
			.AppendInterval(_delay)
			.AppendCallback(() => { OnFinished.Invoke(); })
			.Play();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}