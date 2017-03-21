// SMBTemplate.cs
// @projectName
// 
// Created by @author on @dd/@mm/@yyyy.
// Copyright (c) @yyyy Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom state machine behaviour template.
/// see https://unity3d.com/learn/tutorials/modules/beginner/5-pre-order-beta/state-machine-behaviours
/// </summary>
public class SMBTemplate : StateMachineBehaviour {
	//------------------------------------------------------------------//
	// StateMachineBehaviour IMPLEMENTATION								//
	//------------------------------------------------------------------//
	/// <summary>
	/// Called on the first frame of the state being played.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateInfo">The state we just entered.</param>
	/// <param name="_layerIndex">The index of the layer where the state belongs to.</param>
	public override void OnStateEnter(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {

	}

	/// <summary>
	/// Called after MonoBehaviour Updates on every frame whilst the animator is 
	/// playing the state this behaviour belongs to.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateInfo">The state being updated.</param>
	/// <param name="_layerIndex">The index of the layer where the state belongs to.</param>
	public override void OnStateUpdate(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {
		
	}

	/// <summary>
	/// Called on the last frame of a transition to another state.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateInfo">The state we just exit.</param>
	/// <param name="_layerIndex">The index of the layer where the state belongs to.</param>
	public override void OnStateExit(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {

	}
	
	/// <summary>
	/// Called before OnAnimatorMove would be called in MonoBehaviours for every 
	/// frame the state is playing. When OnStateMove is called, it will stop 
	/// OnAnimatorMove from being called in MonoBehaviours.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateInfo">The state we just exit.</param>
	/// <param name="_layerIndex">The index of the layer where the state belongs to.</param>
	public override void OnStateMove(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {
		
	}

	/// <summary>
	/// Called after OnAnimatorIK on MonoBehaviours for every frame the while the state
	/// is being played. It is important to note that OnStateIK will only be called if 
	/// the state is on a layer that has an IK pass. By default, layers do not have an 
	/// IK pass and so this function will not be called. For more information on IK see 
	/// the information linked below.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateInfo">The state we're running.</param>
	/// <param name="_layerIndex">The index of the layer where the state belongs to.</param>
	public override void OnStateIK(Animator _animator, AnimatorStateInfo _stateInfo, int _layerIndex) {

	}

	/// <summary>
	/// Called on the first frame that the animator plays the contents of a Sub-State Machine.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateMachinePathHash">The full path hash for this state machine.</param>
	public override void OnStateMachineEnter(Animator _animator, int _stateMachinePathHash) {

	}

	/// <summary>
	/// Called on the last frame of a transition from a Sub-State Machine.
	/// </summary>
	/// <param name="_animator">The Animator playing this state machine.</param>
	/// <param name="_stateMachinePathHash">The full path hash for this state machine.</param>
	public override void OnStateMachineExit(Animator _animator, int _stateMachinePathHash) {

	}
}