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

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// If there is an egg in the incubator, load it and anchor it
		if(EggManager.incubatingEgg != null) {
			GameObject newEggObj = EggManager.incubatingEgg.CreateInstance();
			newEggObj.transform.SetParent(transform.parent, false);
			newEggObj.transform.position = transform.position;
		}
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
	// CALLBACKS														//
	//------------------------------------------------------------------//
}

