﻿using UnityEngine;
using System.Collections;

public class StartEndStateMachineBehaviour : StateMachineBehaviour {

	public delegate void MachineEvent(int stateNameHash);
	public MachineEvent onStateEnter;
	public MachineEvent onStateExit;
	private float m_lastNormalizedTime;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( onStateEnter != null )
			onStateEnter( stateInfo.shortNameHash );
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( onStateExit != null )
			onStateExit( stateInfo.shortNameHash );
	}


}
