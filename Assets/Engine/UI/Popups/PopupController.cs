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
	private Animator m_anim = null;
	private bool m_destroyAfterClose = true;

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	// Default initialization to avoid null reference when invoking. 
	// Add as many listeners as you want to this specific event by using the += syntax

	public delegate void OnOpenPreAnimationDelegate();
	public OnOpenPreAnimationDelegate OnOpenPreAnimation = delegate() { };

	public delegate void OnOpenPostAnimationDelegate();
	public OnOpenPostAnimationDelegate OnOpenPostAnimation = delegate() { };

	public delegate void OnClosePreAnimationDelegate();
	public OnClosePreAnimationDelegate OnClosePreAnimation = delegate() { };

	public delegate void OnClosePostAnimationDelegate();
	public OnClosePostAnimationDelegate OnClosePostAnimation = delegate() { };

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake () {
		// Get required components
		m_anim = GetComponent<Animator>();
		DebugUtils.Assert(m_anim != null, "Required Component!!");

		// Dispatch message
		Messenger.Broadcast<PopupController>(EngineEvents.POPUP_CREATED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	protected virtual void OnDestroy() {
		// Dispatch message - it could be problematic using "this" at this point
		Messenger.Broadcast<PopupController>(EngineEvents.POPUP_DESTROYED, this);

		// Loose references to delegates
		OnOpenPreAnimation = null;
		OnOpenPostAnimation = null;
		OnClosePreAnimation = null;
		OnClosePostAnimation = null;
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launches the open animation.
	/// </summary>
	public void Open() {
		// Dispatch message
		Messenger.Broadcast<PopupController>(EngineEvents.POPUP_OPENED, this);

		// Invoke delegate
		OnOpenPreAnimation();

		// Launch anim
		m_anim.SetTrigger("open");
	}

	/// <summary>
	/// Launches the close animation.
	/// </summary>
	/// <param name="_bDestroy">Whether to destroy the popup once the close animation has finished.</param>
	public void Close(bool _bDestroy) {
		// Store flag
		m_destroyAfterClose = _bDestroy;

		// Invoke delegate
		OnClosePreAnimation();

		// Launch anim
		m_anim.SetTrigger("close");
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The open animation has finished. To be called via animation trigger.
	/// </summary>
	private void OnOpenAnimFinished() {
		// Invoke delegate
		OnOpenPostAnimation();
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	/// <param name="_iLevel">The index of the level that was just loaded.</param>
	private void OnCloseAnimationFinished() {
		// Invoke delegate
		OnClosePostAnimation();

		// Dispatch message
		Messenger.Broadcast<PopupController>(EngineEvents.POPUP_CLOSED, this);

		// Delete ourselves if required
		if(m_destroyAfterClose) {
			GameObject.Destroy(gameObject);
		}
	}
}
