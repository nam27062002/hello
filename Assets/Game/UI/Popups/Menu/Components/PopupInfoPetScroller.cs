// PopupInfoPetScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
public struct PetScrollerItem : IUISelectorItem {
	public DefinitionNode def;
	public bool CanBeSelected() { return true; }
}

/// <summary>
/// Auxiliar class to help with the scrolling logic.
/// </summary>
public class PopupInfoPetScroller : UISelectorTemplate<PetScrollerItem> { 
	
}