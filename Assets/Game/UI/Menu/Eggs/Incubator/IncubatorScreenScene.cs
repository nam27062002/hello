// IncubatorScreenScene.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Special implementation for the Incubator 3D scene.
/// </summary>
public class IncubatorScreenScene : MenuScreenScene {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("There should be exactly as many anchors as slots in the incubator!")]
	[SerializeField] private IncubatorEggAnchor[] m_eggAnchors = new IncubatorEggAnchor[EggManager.INVENTORY_SIZE];
	public IncubatorEggAnchor[] eggAnchors { 
		get { return m_eggAnchors; }
	}
}