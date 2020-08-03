using UnityEngine;
using System.Collections.Generic;

namespace AI
{
	[System.Serializable]
	public sealed class State
	{
		[SerializeField]
		string m_name;

		[SerializeField]
		StateComponent[] m_componentAssets = new StateComponent[]{};
		public StateComponent[] componentAssets { get { return m_componentAssets; }}
		
		StateComponent[] m_componentInstances = null;

		public string name { get { return m_name; } }

		public State(){}

		public State(string name)
		{
			m_name = name;
		}

        public void Instantiate(ref List<StateComponent> sharedComponents) {
            m_componentInstances = new StateComponent[m_componentAssets.Length];

            for (int i = 0; i < m_componentAssets.Length; i++) {
                bool newInstance = false;
                // if we have found this component in the shared list, use the shared instance instead of instantiating a new one
                StateComponent comp = sharedComponents.Find(x => x.GetType().Equals(m_componentAssets[i].GetType()));
                if (comp == null) {
                    comp = Object.Instantiate(m_componentAssets[i]);
                    newInstance = true;
                }
                m_componentInstances[i] = comp;

                if (comp.sharedBetweenStates && newInstance) {
                    sharedComponents.Add(comp);
                }
            }
        }


        public void Initialise(StateMachine stateMachine)
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				m_componentInstances[i].Initialise(stateMachine, this);
			}
		}

		public void Remove()
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				m_componentInstances[i].Remove();
			}
		}
		
		public void Enter(State oldState, object[] param = null)
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				StateComponent comp = m_componentInstances[i];
				if(!comp.sharedBetweenStates || oldState == null || oldState.GetComponent(comp.GetType()) == null)
				{
					comp.Enter(oldState, param);
				}
			}
		}
		
		public void Exit(State newState)
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				StateComponent comp = m_componentInstances[i];
				if(!comp.sharedBetweenStates || newState == null || newState.GetComponent(comp.GetType()) == null)
				{
					comp.Exit(newState);
				}
			}
		}

		public void Update()
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				m_componentInstances[i].Update();
			}
		}

		public T GetComponent<T>() where T : StateComponent
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				T comp = m_componentInstances[i] as T;
				if(comp != null)
				{
					return comp;
				}
			}
			return default(T);
		}

		public StateComponent GetComponent(System.Type stateType)
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				if(m_componentInstances[i].GetType() == stateType)
				{
					return m_componentInstances[i];
				}
			}
			return default(StateComponent);
		}

#if UNITY_EDITOR
		public void OnGUI()
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				m_componentInstances[i].OnGUI();
			}
		}
		
		public void OnDrawGizmos()
		{
			for(int i = 0; i < m_componentInstances.Length; i++)
			{
				m_componentInstances[i].OnDrawGizmos();
			}
		}
#endif
	}
}