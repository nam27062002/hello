using UnityEngine;
using System.Collections;

public class FlyLoopBehaviour : StateMachineBehaviour {

	public Range m_timeToGlide = new Range(3f, 4f);
	private float m_timer = 4f;
	private bool m_allowGlide = true;
	public bool allowGlide
	{
		get{ return m_allowGlide; }
		set{ m_allowGlide = value; }
	}
	
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// ResetTimer();
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {	
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if ( m_allowGlide )
		{
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				animator.SetBool("glide", true);
				ResetTimer();
			}
		}
	}

	public void ResetTimer()
	{
		m_timer = m_timeToGlide.GetRandom();
	}
}
