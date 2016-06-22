using UnityEngine;

namespace AISM
{
	public class StateComponent : ScriptableObject
	{
		[SerializeField]
		bool m_sharedBetweenStates;

		bool m_initialised = false;

		protected StateMachine m_stateMachine;
		State m_state;

		public State state { get { return m_sharedBetweenStates ? m_stateMachine.current : m_state; } }
		public bool sharedBetweenStates { get { return m_sharedBetweenStates; } }

		public void Initialise(StateMachine stateMachine, State state)
		{
			if (!m_initialised)
			{
				m_stateMachine = stateMachine;
				m_state = state;

				OnInitialise(m_stateMachine.gameObject);
				m_initialised = true;
			}
		}

		public void Remove()
		{
			OnRemove();
		}

		public void Enter(State oldState, object[] param)
		{
			OnEnter(oldState, param);
		}

		public void Exit(State newState)
		{
			OnExit(newState);
		}

		public void Update()
		{
			OnUpdate();
		}

		protected void Transition(string transitionID, params object[] param)
		{
			m_stateMachine.Transition(transitionID, state, param);
		}

		protected virtual void OnInitialise(GameObject _go){}
		protected virtual void OnRemove(){}
		protected virtual void OnEnter(State oldState, object[] param){}
		protected virtual void OnExit(State newState){}
		protected virtual void OnUpdate(){}

	#if UNITY_EDITOR
		public virtual void OnGUI(){}
		public virtual void OnDrawGizmos(){}
	#endif
	}
}