﻿// PopupController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
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
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Properties
	private bool m_isOpen = false;
	public bool isOpen { get { return m_isOpen; }}

	private bool m_isReady = false;
	public bool isReady { get { return m_isReady; }}

	// Internal
	private Animator m_anim = null;
	private bool m_destroyAfterClose = true;
	private bool m_reopening = false;

	//------------------------------------------------------------------//
	// EVENTS															//
	//------------------------------------------------------------------//
	// Add as many listeners as you want to this specific event by using the .AddListener() method
	// No need to remove them, events will be cleared upon popup's destruction
	public UnityEvent OnOpenPreAnimation = new UnityEvent();
	public UnityEvent OnOpenPostAnimation = new UnityEvent();
	public UnityEvent OnClosePreAnimation = new UnityEvent();
	public UnityEvent OnClosePostAnimation = new UnityEvent();

    protected PopupManagementInfo m_popupManagementInfo = new PopupManagementInfo();

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

        m_popupManagementInfo.popupController = this;

		// By default popups are closed
		m_isOpen = false;

		// Dispatch message
		Broadcaster.Broadcast(BroadcastEventType.POPUP_CREATED, m_popupManagementInfo);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	protected virtual void OnDestroy() {
        if (ApplicationManager.IsAlive) {
            // Dispatch message - it could be problematic using "this" at this point
            Broadcaster.Broadcast(BroadcastEventType.POPUP_DESTROYED, m_popupManagementInfo);

            // Clear all events
            OnOpenPreAnimation.RemoveAllListeners();
            OnOpenPostAnimation.RemoveAllListeners();
            OnClosePreAnimation.RemoveAllListeners();
            OnClosePostAnimation.RemoveAllListeners();
        }
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launches the open animation.
	/// </summary>
	public void Open() {
		// Change status
		m_isOpen = true;

		// Reopening?
		if(m_reopening) {
			// Reset flag
			m_reopening = false;
		} else {
			// Send message
			Broadcaster.Broadcast(BroadcastEventType.POPUP_OPENED, m_popupManagementInfo);
		}

		// Invoke event
		OnOpenPreAnimation.Invoke();

		// Launch anim
		m_anim.ResetTrigger( GameConstants.Animator.CLOSE );
		m_anim.SetTrigger( GameConstants.Animator.OPEN );
	}

	/// <summary>
	/// Launches the close animation.
	/// </summary>
	/// <param name="_bDestroy">Whether to destroy the popup once the close animation has finished.</param>
	public void Close(bool _bDestroy) {
		// Store flag
		m_destroyAfterClose = _bDestroy;

		// Invoke event
		OnClosePreAnimation.Invoke();

		// Launch anim
		m_anim.ResetTrigger( GameConstants.Animator.OPEN );
		m_anim.SetTrigger( GameConstants.Animator.CLOSE );
	}

	/// <summary>
	/// Launch the close and open animation in sequence.
	/// POPUP_CLOSED and POPUP_OPENED events won't be broadcasted.
	/// </summary>
	public void Reopen() {
		// Toggle reopening flag
		m_reopening = true;

		// If the popup is opened, close it, otherwise just open it
		if(m_isOpen) {
			Close(false);
		} else {
			Open();
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The open animation has finished. To be called via animation trigger.
	/// </summary>
	private void OnOpenAnimFinished() {
		// Invoke event
		OnOpenPostAnimation.Invoke();
		m_isReady = true;
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	/// <param name="_iLevel">The index of the level that was just loaded.</param>
	private void OnCloseAnimationFinished() {
		// Update state
		m_isOpen = false;
		m_isReady = false;

		// Invoke event
		OnClosePostAnimation.Invoke();

		// Reopening?
		if(m_reopening) {
			// Launch open animation
			Open();
		} else {
			// Dispatch message
			Broadcaster.Broadcast(BroadcastEventType.POPUP_CLOSED, m_popupManagementInfo);

			// Delete ourselves if required
			if(m_destroyAfterClose) {
				GameObject.Destroy(gameObject);
			}
		}
	}
}
