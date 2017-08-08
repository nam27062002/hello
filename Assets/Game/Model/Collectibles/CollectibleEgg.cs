// CollectibleEgg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// In-game collectible egg.
/// </summary>
public class CollectibleEgg : Collectible {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string TAG = "Egg";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[Space]
	[SerializeField] protected GameObject m_view = null;
	[SerializeField] private ParticleSystem m_idleFX = null;
	[SerializeField] private ParticleSystem m_collectFX = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Initialize FX
		m_idleFX.Play();
		m_collectFX.Stop();
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the unique tag identifying collectible objects of this type.
	/// </summary>
	/// <returns>The tag.</returns>
	override public string GetTag() {
		return TAG;
	}

	//------------------------------------------------------------------------//
	// VIRTUAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Override to check additional conditions when attempting to collect.
	/// </summary>
	/// <returns><c>true</c> if this collectible can be collected, <c>false</c> otherwise.</returns>
	override protected bool CanBeCollected() {
		// If inventory is full, don't collect
		if(EggManager.IsReady() && EggManager.isInventoryFull) {
			// Broadcast message to show some feedback
			Messenger.Broadcast<CollectibleEgg>(GameEvents.EGG_COLLECTED_FAIL, this);
			return false;
		}
		return true;
	}

	/// <summary>
	/// Override to perform additional actions when collected.
	/// </summary>
	override protected void OnCollect() {
		// Launch FX
		m_idleFX.Stop();
		m_idleFX.gameObject.SetActive(false);	// [AOC] There seems to be some kind of bug where the particles stay on screen. Disable the game object to be 100% sure they are not visible.
		m_collectFX.Play();

		// Disable view after a delay
		DOVirtual.DelayedCall(0.05f, HideAfterDelay, false);

		// Dispatch global event
		Messenger.Broadcast<CollectibleEgg>(GameEvents.EGG_COLLECTED, this);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Hide view after some delay.
	/// To be called via Invoke().
	/// </summary>
	private void HideAfterDelay() {
		// Hide the view
		if(m_view != null) m_view.gameObject.SetActive(false);

		// Let's move it down instead so it looks like debris
		/*if(m_view != null) {
			m_view.transform.Translate(0f, -1.15f, 0f, Space.World);	// [AOC] Magic Number >_<
		}*/
	}
}