using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AI
{
	[CustomEditor(typeof(StateMachine))] 
	public class StateMachineInspector : Editor
	{
		bool m_mainTransitionFoldout = false;
		bool m_anyStateTransition = false;
		List<bool> m_transitionFoldout = new List<bool>();
		
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			
			StateMachine stateMachine = target as StateMachine;
			State[] states = TypeUtil.GetPrivateVar<State[]>(stateMachine, "m_states");
			
			string[] stateNames = new string[states.Length];
			for(int i = 0; i < states.Length; i++)
				stateNames[i] = states[i].name;
			
			// States
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_states"), true);
			
			if(states.Length != m_transitionFoldout.Count)
			{
				while(states.Length > m_transitionFoldout.Count)
					m_transitionFoldout.Add(false);
				while(states.Length < m_transitionFoldout.Count)
					m_transitionFoldout.RemoveAt(m_transitionFoldout.Count - 1);
			}

			StateMachine.TransitionData[] td = TypeUtil.GetPrivateVar<StateMachine.TransitionData[]>(stateMachine, "m_transitionData");
			// Check all transition data is valid
			List<StateMachine.TransitionData> validTd = new List<StateMachine.TransitionData>();
			for( int i = 0;i<td.Length; i++ )
			{
				if ( td[i].from < states.Length && td[i].to < states.Length)
					validTd.Add( td[i] );
			}
			td = validTd.ToArray();
			TypeUtil.SetPrivateVar( stateMachine, "m_transitionData", td);

			Dictionary<int, List<StateMachine.TransitionData>> transitions = new Dictionary<int, List<StateMachine.TransitionData>>();
			List<StateMachine.TransitionData> anyStateTransitions = new List<StateMachine.TransitionData>();

			for(int i = 0; i < td.Length; i++)
			{
				if(td[i].from == -1)
				{
					anyStateTransitions.Add(td[i]);
				}
				else
				{
					if(!transitions.ContainsKey(td[i].from))
					{
						transitions.Add(td[i].from, new List<StateMachine.TransitionData>());
					} 
					transitions[td[i].from].Add(td[i]);
				}
			}

			m_mainTransitionFoldout = EditorGUILayout.Foldout(m_mainTransitionFoldout, "Transitions");
			if(m_mainTransitionFoldout)
			{				
				string[] machineTransitionsTriggers = GetAllFieldsWithAttributeRecursive(typeof(AI.SignalTriggers), typeof(StateTransitionTrigger));

				EditorGUI.indentLevel++;

				for(int i = 0; i < states.Length; i++)
				{
					m_transitionFoldout[i] = EditorGUILayout.Foldout(m_transitionFoldout[i], states[i].name);
					if(m_transitionFoldout[i])
					{
						EditorGUI.indentLevel++;
						int currentSize = transitions.ContainsKey(i)? transitions[i].Count : 0;
						int size = EditorGUILayout.IntField("Size", currentSize);
						
						if(transitions.ContainsKey(i))
						{
							List<StateMachine.TransitionData> list = transitions[i];

							for(int j = 0; j < list.Count; j++)
							{
								string[] stateTransitionsTriggers = GetStateTransitions(states[i]);

								string[] allTransitionsTriggers = new string[stateTransitionsTriggers.Length + machineTransitionsTriggers.Length];
								stateTransitionsTriggers.CopyTo(allTransitionsTriggers, 0);
								machineTransitionsTriggers.CopyTo(allTransitionsTriggers, stateTransitionsTriggers.Length);

								DisplayTransition(list[j], allTransitionsTriggers, stateNames);
							}
						}
						if(size != currentSize)
						{
							while(currentSize < size)
							{
								if(!transitions.ContainsKey(i))
								{
									transitions.Add(i, new List<StateMachine.TransitionData>());
								}
								StateMachine.TransitionData transData = new StateMachine.TransitionData();
								transData.from = i;
								transitions[i].Add(transData);
								currentSize++;
							}
							while(size < currentSize)
							{
								transitions[i].RemoveAt(transitions[i].Count-1);
								currentSize--;
							}
						}
						EditorGUI.indentLevel--;
					}
				}
				EditorGUI.indentLevel--;
			}
				
			m_anyStateTransition = EditorGUILayout.Foldout(m_anyStateTransition, "Any State Transitions");
			if(m_anyStateTransition)
			{				
				EditorGUI.indentLevel++;

				string[] machineTransitionsTriggers = GetAllFieldsWithAttributeRecursive(typeof(AI.SignalTriggers), typeof(StateTransitionTrigger));

				int currentSize = anyStateTransitions.Count;
				int size = EditorGUILayout.IntField("Size", currentSize);
				foreach(StateMachine.TransitionData anyTrans in anyStateTransitions)
				{
					DisplayTransition(anyTrans, machineTransitionsTriggers, stateNames);
				}
				if(size != currentSize)
				{
					while(currentSize < size)
					{
						StateMachine.TransitionData transData = new StateMachine.TransitionData();
						transData.from = -1;
						anyStateTransitions.Add(transData);
						currentSize++;
					}
					while(size < currentSize)
					{
						anyStateTransitions.RemoveAt(anyStateTransitions.Count-1);
						currentSize--;
					}
				}
				EditorGUI.indentLevel--;
			}

			if (m_mainTransitionFoldout || m_anyStateTransition)
			{
				List<StateMachine.TransitionData> newTransitions = new List<StateMachine.TransitionData>();

				foreach (var l in transitions.Values)
				{
					newTransitions.AddRange(l);
				}
				newTransitions.AddRange(anyStateTransitions);
				TypeUtil.SetPrivateVar(stateMachine, "m_transitionData", newTransitions.ToArray());
			}

			// FirstState
			int firstStateIndex = TypeUtil.GetPrivateVar<int>(stateMachine, "m_firstState_A");
			firstStateIndex = Mathf.Min(stateNames.Length - 1, firstStateIndex);
			firstStateIndex = EditorGUILayout.Popup("FirstState", firstStateIndex, stateNames);
			TypeUtil.SetPrivateVar(stateMachine, "m_firstState_A", firstStateIndex);

			// FirstState
			int altFirstStateIndex = TypeUtil.GetPrivateVar<int>(stateMachine, "m_firstState_B");
			altFirstStateIndex = Mathf.Min(stateNames.Length - 1, altFirstStateIndex);
			altFirstStateIndex = EditorGUILayout.Popup("FirstStateAlt", altFirstStateIndex, stateNames);
			TypeUtil.SetPrivateVar(stateMachine, "m_firstState_B", altFirstStateIndex);

			//
			EditorUtility.SetDirty(stateMachine);
			serializedObject.ApplyModifiedProperties();
		}

		void DisplayTransition(StateMachine.TransitionData transData, string[] transitionsList, string[] options)
		{
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			
			int idIndex = Array.FindIndex(transitionsList, x => x == transData.id);
			
			idIndex = EditorGUILayout.Popup(idIndex, transitionsList);
			
			transData.id = idIndex == -1? "" : transitionsList[idIndex];
			transData.to = EditorGUILayout.Popup(transData.to, options);
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}

		string[] GetStateTransitions(State state)
		{
			List<string> transList = new List<string>();
			
			StateComponent[] components = TypeUtil.GetPrivateVar<StateComponent[]>(state, "m_componentAssets");
			foreach(StateComponent comp in components)
			{
				string[] fields = GetAllFieldsWithAttribute(comp.GetType(), typeof(StateTransitionTrigger));
				transList.AddRange(fields);
			}
			return transList.ToArray();
		}

		string[] GetAllFieldsWithAttribute(Type type, Type attrType)
		{
			List<string> transList = new List<string>();

			FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach(FieldInfo fi in fieldInfos)
			{
				if(fi.GetCustomAttributes(typeof(StateTransitionTrigger), true).Length > 0)
				{
					transList.Add((string)fi.Name);
				}
			}
			if(type.BaseType != typeof(System.Object))
			{
				transList.AddRange(GetAllFieldsWithAttribute(type.BaseType, attrType));
			}
			return transList.ToArray();
		}


		string[] GetAllFieldsWithAttributeRecursive(Type type, Type attrType)
		{
			List<string> fields = new List<string>();

			System.Type[] types = TypeUtil.GetInheritedOfType(type);

			fields.AddRange(GetAllFieldsWithAttribute(type, attrType));
			for (int i = 0; i < types.Length; i++) {
				fields.AddRange(GetAllFieldsWithAttribute(types[i], attrType));
			}

			return fields.ToArray();
		}
	}
}