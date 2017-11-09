using UnityEngine;
using System.Collections;

public class StartEndMachineBehaviourMessage : StateMachineBehaviour 
{
	public string m_message = "TurboLoop";
	private string m_startMessage;
	private string m_endMessage;
	void Awake()
	{
		m_startMessage = m_message + "Start";
		m_endMessage = m_message + "End";
	}

	override public void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, animatorStateInfo,layerIndex);
		animator.gameObject.SendMessage(m_startMessage);			
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		base.OnStateExit(animator, animatorStateInfo,layerIndex);
		animator.gameObject.SendMessage(m_endMessage);
		
	}
}
