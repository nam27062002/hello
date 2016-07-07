using UnityEngine;

namespace AISM
{
	[System.Serializable]
	public abstract class StateComponentData {}

	public class StateComponent : ScriptableObject
	{
		[SerializeField]
		bool m_sharedBetweenStates;

		bool m_initialised  = false;

		protected StateMachine m_stateMachine;
		State m_state;

		public State state { get { return m_sharedBetweenStates ? m_stateMachine.current : m_state; } }
		public bool sharedBetweenStates { get { return m_sharedBetweenStates; } }

		public void Initialise(StateMachine _stateMachine, State _state)
		{
			if (!m_initialised)
			{
				m_stateMachine = _stateMachine;
				m_state = _state;

				OnInitialise(m_stateMachine.gameObject);
				m_initialised = true;
			}
		}

		public void Remove()
		{
			OnRemove();
		}

		public void Enter(State _oldState, object[] _param)
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

		protected void Transition(string transitionID, params object[] param)
		{
			m_stateMachine.Transition(transitionID, state, param);
		}

		protected virtual void OnInitialise(GameObject _go){}
		protected virtual void OnRemove(){}
		protected virtual void OnEnter(State _oldState, object[] _param){}
		protected virtual void OnExit(State _newState){}
		protected virtual void OnUpdate(){}


	#if UNITY_EDITOR
		public virtual void OnGUI(){}
		public virtual void OnDrawGizmos(){}
	#endif
	}
}