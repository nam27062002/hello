// IncubatorEggAnchor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/02/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls the anchor of the eggs on the incubator.
/// </summary>
public class IncubatorEggAnchor : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private float m_snapDistance = 2f;
	public float snapDistance {
		get { return m_snapDistance; }
		set { m_snapDistance = Mathf.Max(0f, value); }
	}

	private EggController m_attachedEgg = null;
	public EggController attachedEgg {
		get { return m_attachedEgg; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// If there is an egg in the incubator, load it and anchor it
		if(EggManager.incubatingEgg != null) {
			EggController newEgg = EggManager.incubatingEgg.CreateInstance();
			newEgg.transform.SetParent(transform, false);
			AttachEgg(newEgg);
		}

		// Subscribe to external events
		Messenger.AddListener(GameEvents.EGG_INCUBATOR_CLEARED, OnIncubatorCleared);
	}

	/// <summary>
	/// Destructor,
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.EGG_INCUBATOR_CLEARED, OnIncubatorCleared);
	}

	/// <summary>
	/// Draw scene gizmos for this object.
	/// </summary>
	private void OnDrawGizmos() {
		if(m_snapDistance >= 0f) {
			Gizmos.color = Colors.WithAlpha(Colors.orange, 0.5f);
			Gizmos.DrawSphere(transform.position, m_snapDistance);
		}

		Gizmos.color = Colors.orange;
		Gizmos.DrawSphere(transform.position, 1f);

		Gizmos.color = Colors.white;
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Attachs the given egg view to this anchor.
	/// If another egg view was already attached, it will be destroyed.
	/// Nothing will happen if the given egg is <c>null</c>.
	/// </summary>
	/// <param name="_egg">The egg to be attached to this anchor.</param>
	public void AttachEgg(EggController _egg) {
		// Check params
		if(_egg == null) return;

		// If there is an egg already attached, destroy it
		DeattachEgg(true);

		// Put egg into position
		_egg.transform.position = this.transform.position;

		// Store reference
		m_attachedEgg = _egg;
	}

	/// <summary>
	/// Deattachs the current attached egg from the anchor.
	/// Optionally destroy it as well.
	/// Nothing will happen if there is no egg attached.
	/// </summary>
	/// <param name="_destroy">Whether to destroy the attached egg or not.</param>
	public void DeattachEgg(bool _destroy) {
		if(m_attachedEgg == null) return;

		// Destroy?
		if(_destroy) {
			GameObject.Destroy(m_attachedEgg.gameObject);
		}

		// Clear reference
		m_attachedEgg = null;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The incubator has been cleared, destroy ourselves.
	/// We should only receive this event if we're actually the incubating egg.
	/// </summary>
	private void OnIncubatorCleared() {
		// Destroy attached egg (if any)
		DeattachEgg(true);
	}
}

