// AOCQuickTest.cs
// Hungry Dragon
// 
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace AI {
	public abstract class AIPilot : Pilot, Spawnable, ISerializationCallbackReceiver {
		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
		[SerializeField] private StateMachine m_brainResource;
		public StateMachine brainResource { get { return m_brainResource; }}

		private StateMachine m_brain;

		protected Vector3 m_homePosition;
		public Vector3 homePosition { get { return m_homePosition; } }

		protected Vector3 m_target;

		//--------------------------------------------------------------------//
		// METHODS															  //
		//--------------------------------------------------------------------//
		public void Spawn(Spawner _spawner) {
			m_area = _spawner.area.bounds;
			m_homePosition = _spawner.transform.position;

			m_target = transform.position;

			// braaiiiinnn ~ ~ ~ ~ ~
			if (m_brain == null) {
				m_brain = Object.Instantiate(m_brainResource) as StateMachine;
			}
			m_brain.Initialise(gameObject, true);
		}

		public override void OnTrigger(string _trigger) {
			m_brain.Transition(_trigger);
		}

		public void GoTo(Vector3 _target) {
			m_target = _target;
		}

		protected override void Update() {
			base.Update();

			// state machine updates
			if (m_brain != null) {
				m_brain.Update();
			}
			
			// if this machine is outside his area, go back to home position (if it has this behaviour)
			if (!m_area.Contains(transform.position)) {
				m_machine.SetSignal(Signals.BackToHome.name, true);
			}
		}

		void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawSphere(m_target, 0.25f);
		}

		//--------------------------------------------------------------------//
		// STATE MACHINE DATA												  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Since dictionaries are not serializable, we must create an auxiliar kvp
		/// class to be able to store the dictionary as a list.
		/// </summary>
		[System.Serializable]
		public sealed class StateComponentDataKVP {
			// [AOC] Since Unity doesn't serialize System.Type, use the AssemblyQualifiedName to compare component types
			//		 Type of the StateComponent, not the StateComponentData!!!
			public string typeName = "";
			public StateComponentData data;		// Can be null!!
			public bool folded = false;			// [AOC] For the editor
		}

		// Custom data for each state machine component
		// [AOC] Although public, it shouldn't be modified (only by the editor).
		// [AOC] Don't show in the default inspector, we will draw the list via the AIPilot's custom editor
		[HideInInspector] public List<StateComponentDataKVP> componentsData = new List<StateComponentDataKVP>();

		// [AOC] Unfortunately Unity doesn't serialize custom abstract classes, nor behaves well with inheritance,
		//		 so we must manage serialization of the state component data on our own.
		// See http://docs.unity3d.com/Manual/script-Serialization.html
		// See HSX's GameData class
		[SerializeField][HideInInspector] private string m_serializedJson = "";

		/// <summary>
		/// Gets the data linked to the target component type.
		/// </summary>
		/// <returns>The component data linked to the requested component type. <c>null</c> if there is no data for the given type.</returns>
		/// <typeparam name="T">Type of the state component whose data we want.</typeparam>
		public StateComponentData GetComponentData<T>() where T : StateComponent {
			// Iterate the components data list looking for the target component
			// Since Unity doesn't serialize System.Type, use the AssemblyQualifiedName to compare types
			string typeName = typeof(T).AssemblyQualifiedName;
			for(int i = 0; i < componentsData.Count; i++) {
				// Is it the target component type?
				if(componentsData[i].typeName == typeName) {
					return componentsData[i].data;
				}
			}
			return null;
		}

		//--------------------------------------------------------------------//
		// ISerializationCallbackReceiver IMPLEMENTATION					  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Serialization is about to start.
		/// </summary>
		public void OnBeforeSerialize() {
			// Put all data into a JSON
		}

		/// <summary>
		/// Deserialization just finished.
		/// </summary>
		public void OnAfterDeserialize() {
			// Parse the JSON
		}
	}
}