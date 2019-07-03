using UnityEngine;
using System.Collections.Generic;

namespace AI
{
	[System.Serializable]
	public abstract class StateComponentData {
		/// <summary>
		/// Serialize the object into a dictionary with the format <fieldName, value>.
		/// </summary>
		public Dictionary<string, object> Serialize() {
			return TypeUtil.GetData(this);
		}

		/// <summary>
		/// Initialize the object with a dictionary with the format <fieldName, value>.
		/// </summary>
		/// <param name="_data">The serialized data with the format <fieldName, value>.</param>
		public void Deserialize(Dictionary<string, object> _data) {
			TypeUtil.ApplyData(this, _data, "", "");
		}
	}

	public class StateComponent : ScriptableObject
	{
		[SerializeField]
		bool m_sharedBetweenStates;

		bool m_initialised  = false;

		protected StateMachine m_stateMachine;
		protected AIPilot m_pilot;
		protected IMachine m_machine;
		protected IViewControl m_viewControl;

		State m_state;
		public State state { get { return m_sharedBetweenStates ? m_stateMachine.current : m_state; } }
		public bool sharedBetweenStates { get { return m_sharedBetweenStates; } }

		public void Initialise(StateMachine _stateMachine, State _state)
		{
			if (!m_initialised)
			{
				m_stateMachine 	= _stateMachine;
				m_pilot 		= m_stateMachine.gameObject.GetComponent<AIPilot>();
				m_machine		= m_stateMachine.gameObject.GetComponent<IMachine>();
				m_viewControl 	= m_stateMachine.gameObject.GetComponent<IViewControl>();

				m_state = _state;

				OnInitialise();
				m_initialised = true;
			}
		}

		public void Remove()
		{
			OnRemove();
		}

		public void Enter(State _oldState, object[] _param = null)
		{
			OnEnter(_oldState, _param);
		}

		public void Exit(State newState)
		{
			OnExit(newState);
		}

		public void Update()
		{
			OnUpdate();
		}

		public virtual StateComponentData CreateData() { return null; }
		public virtual System.Type GetDataType() { return null; }

		protected void Transition(string transitionID, object[] param = null)
		{
			m_stateMachine.Transition(transitionID, state, param);
		}

		protected virtual void OnInitialise(){}
		protected virtual void OnRemove(){}
		protected virtual void OnEnter(State _oldState, object[] _param = null){}
		protected virtual void OnExit(State _newState){}
		protected virtual void OnUpdate(){}


	#if UNITY_EDITOR
		public virtual void OnGUI(){}
		public virtual void OnDrawGizmos(){}
	#endif
	}
}