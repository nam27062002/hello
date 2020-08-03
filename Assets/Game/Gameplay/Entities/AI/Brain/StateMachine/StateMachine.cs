using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI
{
	[CreateAssetMenu(menuName = "Brain")]
	public class StateMachine : ScriptableObject
	{
		[SerializeField]
		State[] m_states = new State[]{};
		public State[] states { get { return m_states; }}

		[System.Serializable]
		public class TransitionData { public int to; public string id; public int from; }

		[SerializeField]
		TransitionData[] m_transitionData = new TransitionData[]{};

		[SerializeField]
		int m_firstState_A;

		[SerializeField]
		int m_firstState_B;

		State m_current;
		State m_previous;

		Dictionary<State, Dictionary<int, State> > m_transitions = new Dictionary<State, Dictionary<int, State>>();
		Dictionary<int, State> m_anyStateTransitions = new Dictionary<int, State>();

		State m_queuedTransition;
		object[] m_transitionParam;

		GameObject m_gameObject;

		public GameObject gameObject { get { return m_gameObject; } }
		public State current { get { return m_current; } }
		public State previous { get { return m_previous; } }

		void OnEnable()
		{
			OnLoadTransition();
			m_current = null;
			m_previous = null;
			m_queuedTransition = null;
			m_transitionParam = null;
		}

        public void Instantiate(GameObject _go) {
            m_gameObject = _go;

            List<StateComponent> sharedComponents = new List<StateComponent>();
            for (int i = 0; i < m_states.Length; i++) {
                m_states[i].Instantiate(ref sharedComponents);
            }
        }

        public void Initialise(bool enterFirstState = false) 
		{			
			for(int i = 0; i < m_states.Length; i++)
			{
				m_states[i].Initialise(this);
			}

			if(enterFirstState && m_states.Length > 0 && m_firstState_A > -1)
			{
				int state = m_firstState_A;
				if (m_firstState_B > -1 && UnityEngine.Random.Range(0f, 1f) > 0.5f) {
					state = m_firstState_B;
				}
				DoTransition(m_states[state], null);
			}
		}

		public void OnDestroy() {
			if (ApplicationManager.IsAlive) {
				if (m_current != null) {
					m_current.Exit(null);
					m_current = null;
				}

				for (int i = 0; i < m_states.Length; i++){
					m_states[i].Remove();
				}
			}
		}

		public void Exit() {			
			if (m_current != null) {
				m_current.Exit(null);
				m_current = null;
			}
		}

		public void Reset() {
			if (m_current != null) {
				m_current.Exit(null);
				m_current = null;
			}

			if (m_states.Length > 0 && m_firstState_A > -1) {
				int state = m_firstState_A;
				if (m_firstState_B > -1 && UnityEngine.Random.Range(0f, 1f) > 0.5f) {
					state = m_firstState_B;
				}
				DoTransition(m_states[state], null);
			}
		}

		public void Update() {            
			if (m_current != null) {
				m_current.Update();
			}

			if (m_queuedTransition != null) {
				DoTransition(m_queuedTransition, m_transitionParam);
			}
	    }

		public void Transition(int id, object[] param = null)
		{
			Transition(id, m_current, param);
		}

		public void Transition(int id, State from, object[] param = null)
		{
			State to = null;
			if(from != null && m_transitions.ContainsKey(from) && m_transitions[from].ContainsKey(id))
			{
				to = m_transitions[from][id];
			}
			else if(m_anyStateTransitions.ContainsKey(id))
			{
				to = m_anyStateTransitions[id];
			}

			if(to != null)
			{
				if(m_current != null)
				{
					if(to != m_current)
					{
						m_queuedTransition = to;
						m_transitionParam = param;
					}
				}
				else
				{
					DoTransition(to, param);
				}
			}
		}
	    
		void DoTransition(State to, object[] param = null)
	    {
			m_queuedTransition = null;
			m_transitionParam = null;

	        if(m_current != null)
			{
	            m_current.Exit(to);
			}
	        
			m_previous = m_current;
			State from = m_current;
			m_current = to;

			to.Enter(from, param);
	    }

		public void OnLoadTransition()
		{
			m_transitions.Clear();
			for(int i = 0; i < m_transitionData.Length; i++)
			{
				if(m_transitionData[i].from == -1)
				{
					m_anyStateTransitions.Add(UnityEngine.Animator.StringToHash(m_transitionData[i].id), m_states[m_transitionData[i].to]);
				}
				else
				{
					State from = m_states[m_transitionData[i].from];
					
					if(!m_transitions.ContainsKey(from))
					{
						m_transitions.Add(from, new Dictionary<int, State>());
					}
					Dictionary<int, State> stateTrans = m_transitions[from];

					stateTrans.Add(UnityEngine.Animator.StringToHash(m_transitionData[i].id), m_states[m_transitionData[i].to]);
				}
			}
		}

		public State GetState(string stateName)
		{
			State ret = null;
			for (int i = 0; i < m_states.Length && ret == null; i++)
			{
				if ( m_states[i].name.Equals( stateName ) )
					ret = m_states[i];
			}
			return ret;
		}
        
#if UNITY_EDITOR
		public void OnGUI()
		{
			if (m_current != null)
			{
				m_current.OnGUI();
			}
		}
		
		public void OnDrawGizmos()
		{
			if(m_current != null)
			{
				m_current.OnDrawGizmos();
			}
		}
#endif
	}
}