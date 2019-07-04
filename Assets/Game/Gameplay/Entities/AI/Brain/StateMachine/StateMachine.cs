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

		Dictionary<State, Dictionary<string, State> > m_transitions = new Dictionary<State, Dictionary<string, State>>();
		Dictionary<string, State> m_anyStateTransitions = new Dictionary<string, State>();

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

		public void Initialise(GameObject _go, bool enterFirstState = false) 
		{	
			m_gameObject = _go;	
			
			List<StateComponent> sharedComponents = new List<StateComponent>();
			for(int i = 0; i < m_states.Length; i++)
			{
				m_states[i].Initialise(this, ref sharedComponents);
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

		public void Transition(string id, object[] param = null)
		{
			Transition(id, m_current, param);
		}

		public void Transition(string id, State from, object[] param = null)
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
					m_anyStateTransitions.Add(m_transitionData[i].id, m_states[m_transitionData[i].to]);
				}
				else
				{
					State from = m_states[m_transitionData[i].from];
					
					if(!m_transitions.ContainsKey(from))
					{
						m_transitions.Add(from, new Dictionary<string, State>());
					}
					Dictionary<string, State> stateTrans = m_transitions[from];

					stateTrans.Add(m_transitionData[i].id, m_states[m_transitionData[i].to]);
				}
			}
		}

		public void OnSaveTransition()
		{
			int c = 0;
			foreach(var kvp in m_transitions)
			{
				c += kvp.Value.Count;
			}

			m_transitionData = new TransitionData[c + m_anyStateTransitions.Count];

			c = 0;
			foreach(var kvp in m_transitions)
			{
				foreach(var kvp2 in kvp.Value)
				{
					TransitionData data = new TransitionData();
					data.id = kvp2.Key;
					data.from = Array.IndexOf(m_states, kvp.Key);
					data.to = Array.IndexOf(m_states, kvp2.Value);

					m_transitionData[c++] = data;
				}
			}
			foreach(var kvp in m_anyStateTransitions)
			{
				TransitionData data = new TransitionData();
				data.id = kvp.Key;
				data.from = -1;
				data.to = Array.IndexOf(m_states, kvp.Value);
				
				m_transitionData[c++] = data;
			}
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