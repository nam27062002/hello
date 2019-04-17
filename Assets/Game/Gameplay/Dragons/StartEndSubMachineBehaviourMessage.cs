using UnityEngine;
using System.Collections;

public class StartEndSubMachineBehaviourMessage : StateMachineBehaviour 
{
	public string m_message = "TurboLoop";
	private string m_startMessage;
	private string m_endMessage;
	void Awake()
	{
		m_startMessage = m_message + "Start";
		m_endMessage = m_message + "End";
	}

    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
	{
		base.OnStateMachineEnter(animator, stateMachinePathHash);
		animator.gameObject.SendMessage(m_startMessage);			
	}

    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
	{
		base.OnStateMachineExit(animator, stateMachinePathHash);
		animator.gameObject.SendMessage(m_endMessage);
		
	}
}
