using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace AI
{
	public static class StateMachineEditor
	{
		public static void Reset(this StateMachine sm)
		{
			TypeUtil.SetPrivateVar(sm, "m_firstState", -1);
			TypeUtil.SetPrivateVar(sm, "m_transitionData", new StateMachine.TransitionData[]{});
			TypeUtil.SetPrivateVar(sm, "m_states", new State[]{});
			TypeUtil.SetPrivateVar(sm, "m_useInstance", false);
			TypeUtil.SetPrivateVar(sm, "m_current", null);
			TypeUtil.SetPrivateVar(sm, "m_transitions", new Dictionary<State, Dictionary<string, State>>());

			UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(sm));
			for(int i = 0; i < objs.Length; i++)
			{
				if(!(objs[i] is StateMachine))
					UnityEngine.Object.DestroyImmediate(objs[i], true);
			}
		}

		public static State AddState(this StateMachine sm, string name)
		{
			State state = new State(name);
			AddToPrivateArray<State>(sm, "m_states", state);
			
			return state;
		}

		public static bool RemoveState(this StateMachine sm, State state)
		{
			State[] states = TypeUtil.GetPrivateVar<State[]>(sm, "m_states");
			int i = Array.IndexOf(states, state);
			if(i > -1)
			{
				// TODO: impl. Will be required for the node editor!
			}
			return i > -1;
		}

		public static T AddComponent<T>(this StateMachine sm, State state, bool addToSM = false) where T : StateComponent
		{
			T component = ScriptableObject.CreateInstance<T>();
			AddToPrivateArray<StateComponent>(state, "m_componentAssets", component);
			
			component.name = component.GetType().Name;

			if(addToSM)
				AssetDatabase.AddObjectToAsset(component, sm);
			
			return component;
		}
		
		public static void AddTransition(this StateMachine sm, string id, State from, State to)
		{
			var transitions = TypeUtil.GetPrivateVar< Dictionary<State, Dictionary<string, State> > >(sm, "m_transitions");

			if(!transitions.ContainsKey(from))
			{
				transitions.Add(from, new Dictionary<string, State>());
			}
			Dictionary<string, State> stateTrans = transitions[from];
			stateTrans.Add(id, to);
		}
		
		public static void InitialState(this StateMachine sm, State firstState)
		{
			State[] states = TypeUtil.GetPrivateVar<State[]>(sm, "m_states");
			int i = Array.IndexOf(states, firstState);
			
			TypeUtil.SetPrivateVar(sm, "m_firstState", i);
		}
		
		public static void UseInstance(this StateMachine sm, bool value)
		{
			TypeUtil.SetPrivateVar(sm, "m_useInstance", value);
		}
		
		// Misc util/helper functions
		
		static void AddToPrivateArray<T>(object obj, string varName, T value)
		{
			T[] array = TypeUtil.GetPrivateVar<T[]>(obj, varName);
			List<T> list = new List<T>(array);
			list.Add(value);
			TypeUtil.SetPrivateVar(obj, varName, list.ToArray());
		}
	}
}