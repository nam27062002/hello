// AnimatedRankedRewardView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Small extension of the RankedRewardView UI class to include an animator.
/// </summary>
public class AnimatedRankedRewardView : RankedRewardView {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Space]
	[SerializeField] private ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get { return m_animator; }
	}
}