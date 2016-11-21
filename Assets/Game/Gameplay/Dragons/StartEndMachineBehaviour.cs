using UnityEngine;
using System.Collections;

public class StartEndMachineBehaviour : StateMachineBehaviour {

	public delegate void MachineEvent(int stateNameHash);
	public MachineEvent onStart;
	public MachineEvent onEnd;
	private float m_lastNormalizedTime;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		m_lastNormalizedTime = -1;
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {		
		if ( (int)stateInfo.normalizedTime > (int)m_lastNormalizedTime )
		{
			// End Event if needed
			if (m_lastNormalizedTime < 0 && onEnd != null)
				onEnd( stateInfo.shortNameHash );

			// Start event
			if ( onStart != null )
				onStart(stateInfo.shortNameHash);

			m_lastNormalizedTime = stateInfo.normalizedTime;
		}
	}
}
