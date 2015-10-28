// PopupController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple control for popups.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PopupController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private Animator mAnimator = null;
	private bool mDestroyAfterClose = true;

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	public delegate void OnOpenPreAnimationDelegate();
	public OnOpenPreAnimationDelegate onOpenPreAnimationDelegate;

	public delegate void OnOpenPostAnimationDelegate();
	public OnOpenPostAnimationDelegate onOpenPostAnimationDelegate;

	public delegate void OnClosePreAnimationDelegate();
	public OnClosePreAnimationDelegate onClosePreAnimationDelegate;

	public delegate void OnClosePostAnimationDelegate();
	public OnClosePostAnimationDelegate onClosePostAnimationDelegate;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake () {
		// Get required components
		mAnimator = GetComponent<Animator>();
		DebugUtils.Assert(mAnimator != null, "Required Component!!");
	}

	/// <summary>
	/// Destructor
	/// </summary>
	protected virtual void OnDestroy() {
		// Loose references to delegates
		onOpenPreAnimationDelegate = null;
		onOpenPostAnimationDelegate = null;
		onClosePreAnimationDelegate = null;
		onClosePostAnimationDelegate = null;
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launches the open animation.
	/// </summary>
	public void Open() {
		// Invoke delegate
		if(onOpenPreAnimationDelegate != null) {
			onOpenPreAnimationDelegate();
		}

		// Launch anim
		mAnimator.SetTrigger("open");
	}

	/// <summary>
	/// Launches the close animation.
	/// </summary>
	/// <param name="_bDestroy">Whether to destroy the popup once the close animation has finished.</param>
	public void Close(bool _bDestroy) {
		// Store flag
		mDestroyAfterClose = _bDestroy;

		// Invoke delegate
		if(onClosePreAnimationDelegate != null) {
			onClosePreAnimationDelegate();
		}

		// Launch anim
		mAnimator.SetTrigger("close");
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The open animation has finished. To be called via animation trigger.
	/// </summary>
	private void OnOpenAnimFinished() {
		// Invoke delegate
		if(onOpenPostAnimationDelegate != null) {
			onOpenPostAnimationDelegate();
		}
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	/// <param name="_iLevel">The index of the level that was just loaded.</param>
	private void OnCloseAnimationFinished() {
		// Invoke delegate
		if(onClosePostAnimationDelegate != null) {
			onClosePostAnimationDelegate();
		}

		// Delete ourselves if required
		if(mDestroyAfterClose) {
			GameObject.Destroy(gameObject);
		}
	}
}
