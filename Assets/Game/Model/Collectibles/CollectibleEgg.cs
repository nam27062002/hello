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


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();
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
		// Dispatch global event
		Messenger.Broadcast<CollectibleEgg>(GameEvents.EGG_COLLECTED, this);
	}
}