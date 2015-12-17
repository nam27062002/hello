// DisableAtExitStateMachineBehaviour.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// EXPERIMENTAL! Custom state machine behaviour to disable the game object linked to
/// an animator once the animator state machine exits.
/// see https://unity3d.com/learn/tutorials/modules/beginner/5-pre-order-beta/state-machine-behaviours
/// </summary>
public class DisableAtExitSMB : StateMachineBehaviour {
	//------------------------------------------------------------------//
	// StateMachineBehaviour IMPLEMENTATION								//
	//------------------------------------------------------------------//
	/// <summary>
	/// The state machine is exiting.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateMachinePathHash">The full path hash for this state machine.</param>
	public override void OnStateMachineExit(Animator _animator, int _stateMachinePathHash) {
		_animator.gameObject.SetActive(false);
	}
}