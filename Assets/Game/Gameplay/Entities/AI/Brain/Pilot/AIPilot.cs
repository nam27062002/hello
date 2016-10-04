// AOCQuickTest.cs
// Hungry Dragon
// 
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace AI {
	public abstract class AIPilot : Pilot, ISpawnable, ISerializationCallbackReceiver {
		protected static int m_groundMask;

		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
		[SerializeField] private StateMachine m_brainResource;
		public StateMachine brainResource { get { return m_brainResource; }}

		[SerializeField] private Range m_speedFactorRange = new Range(1f, 1f);
		private float m_speedFactor;
		protected override float speedFactor { get { return m_speedFactor; } }
		private float m_speedVariation;
		protected override float speedVariation { get { return m_speedVariation; } }


		public virtual bool avoidCollisions { get { return false; } set { } }

		private StateMachine m_brain;
		public StateMachine brain{ get{ return m_brain; } }

		protected Vector3 m_homePosition;
		public Vector3 homePosition { get { return m_homePosition; } }

		protected Transform m_homeTransform;
		public Transform homeTransform { get { return m_homeTransform; } set{ m_homeTransform = value; } }

		protected Vector3 m_target;
		public override Vector3 target { get { return m_target; } }

		protected bool m_slowDown;


		//--------------------------------------------------------------------//
		// METHODS															  //
		//--------------------------------------------------------------------//
		public virtual void Spawn(ISpawner _spawner) {
			m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions");

			SetArea(_spawner);

			if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
				m_direction = Vector2.right;
			} else {
				m_direction = Vector2.left;
			}

			m_speedFactor = m_speedFactorRange.GetRandom();
			m_speedVariation = UnityEngine.Random.Range(0.98f, 1.02f); 

			m_target = transform.position;
			m_slowDown = false;

			// braaiiiinnn ~ ~ ~ ~ ~
			if (m_brain == null) {
				if (m_brainResource != null) {
					m_brain = UnityEngine.Object.Instantiate(m_brainResource) as StateMachine;
					m_brain.Initialise(gameObject, true);
				}
			} else {
				m_brain.Reset();
			}
		}

		public void SetArea(ISpawner _spawner) {
			if (_spawner != null) {
				m_area = _spawner.area;
				m_homePosition = _spawner.transform.position;
				m_guideFunction = _spawner.guideFunction;
				m_target = m_homePosition;
			}
		}

		void OnDisable() {
		}

		public override void OnTrigger(string _trigger, object[] _param = null) {
			if (m_brain != null) m_brain.Transition(_trigger, _param);
		}

		public void GoTo(Vector3 _target) {
			m_target = _target;
		}

		public void RotateTo( Quaternion _rotation ){
			m_targetRotation = _rotation;
		}

		public void SlowDown(bool _value) {
			m_slowDown = _value;
		}

		protected override void Update() {
			base.Update();

			// state machine updates
			if (m_brain != null) {
				m_brain.Update();
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
			// [AOC] Since Unity doesn't serialize System.Type, use the Type.FullName to compare component data types
			//		 Type of the StateComponentData
			public string typeName = "";
			public StateComponentData data;
			public bool folded = false;			// [AOC] For the editor
		}

		// Custom data for each state machine component
		// [AOC] Although public, it shouldn't be modified (only by the editor).
		// [AOC] Don't show in the default inspector, we will draw the list via the AIPilot's custom editor
		[HideInInspector] private List<StateComponentDataKVP> m_componentsData = new List<StateComponentDataKVP>();
		public List<StateComponentDataKVP> componentsData { get { return m_componentsData; }}

		// [AOC] Unfortunately Unity doesn't serialize custom abstract classes, nor behaves well with inheritance,
		//		 so we must manage serialization of the state component data on our own.
		// See http://docs.unity3d.com/Manual/script-Serialization.html
		// See HSX's GameData class
		[SerializeField][HideInInspector] private string m_serializedJson = "[]";

		/// <summary>
		/// Gets the data linked to the target component type.
		/// </summary>
		/// <returns>The component data linked to the requested component type. <c>null</c> if there is no data for the given type.</returns>
		/// <typeparam name="T">Type of the state component whose data we want.</typeparam>
		public T GetComponentData<T>() where T : StateComponentData {
			// Iterate the components data list looking for the target data type
			// Since Unity doesn't serialize System.Type, use the Type.FullName to compare types
			string typeName = typeof(T).FullName;
			for(int i = 0; i < m_componentsData.Count; i++) {
				// Is it the target component data type?
				if(m_componentsData[i].typeName == typeName) {
					return (T)m_componentsData[i].data;
				}
			}
			return null;
		}

		/// <summary>
		/// Make sure the target AI Pilot has exactly one data per type component.
		/// Will add data objects when missing and remove them when component not 
		/// found in any state of the state machine (brain).
		/// Very expensive method, should be use with moderation.
		/// </summary>
		public void ValidateComponentsData() {
			// Special case: brainResource not initialized
			if(brainResource == null) {
				m_componentsData.Clear();
				return;
			}

			// Iterate all components in all states of the state machine
			// If a data object for that component type doesn't exist, add it
			HashSet<string> validatedDataTypes = new HashSet<string>();	// Store component names for later usage
			foreach(State state in brainResource.states) {
				foreach(StateComponent component in state.componentAssets) {
					// If this component doesn't require a data object, skip it
					System.Type dataType = component.GetDataType();
					if(dataType == null) continue;

					// If this component data type has already been checked, skip it
					string typeName = component.GetDataType().FullName;
					if(validatedDataTypes.Add(typeName)) {		// Returns true if the value was not already in the hash
						// Check whether we have a data object for this component type
						// Inefficient, but since it's an editor code, we don't care
						StateComponentDataKVP kvp = m_componentsData.Find(x => x.typeName == typeName);

						// If data wasn't found, create one and add it to the pilot
						if(kvp == null) {
							kvp = new StateComponentDataKVP();
							kvp.typeName = typeName;
							kvp.data = component.CreateData();	// [AOC] CreateData() will create the proper data object for this specific component type
							m_componentsData.Add(kvp);	
						}
					}
				}
			}

			// Iterate all data objects.
			// If a data type is no longer required by any of the components on the state machine, delete it
			// Reverse iteration since we'll be deleting items from the same list we're iterating
			for(int i = m_componentsData.Count - 1; i >= 0; i--) {
				// Is it a validated data type?
				if(!validatedDataTypes.Contains(m_componentsData[i].typeName)) {
					// No, delete it
					m_componentsData.RemoveAt(i);
				}
			}
		}

		//--------------------------------------------------------------------//
		// ISerializationCallbackReceiver IMPLEMENTATION					  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Serialization is about to start.
		/// </summary>
		public void OnBeforeSerialize() {
			// Nothing to do
		}

		/// <summary>
		/// Deserialization just finished.
		/// </summary>
		public void OnAfterDeserialize() {
			// Load from the json string
			LoadFromJson();
		}

		/// <summary>
		/// Save to the json string.
		/// </summary>
		public void SaveToJson() {
			// [AOC] Since Unity doesn't serialize custom classes inheriting from an abstract class, 
			//		 we'll serialize it on our own via reflection into a json
			//		 Copied from HSX

			// Make sure all required data components for this pilot are created
			// [AOC] Doing it every time can be costly, try to figure out a better way
			ValidateComponentsData();

			// Put all data into a JSON
			List<object> serializedDatas = new List<object>();
			for(int i = 0; i < m_componentsData.Count; i++) {
				// Create a new data dictionary for this component data
				Dictionary<string, object> data;

				// Add data structure fields
				data = m_componentsData[i].data.Serialize();
				data.Add("dataType", m_componentsData[i].typeName);

				// Add extra info fields
				data.Add("editorFolded", m_componentsData[i].folded);

				// Store it to the list
				serializedDatas.Add(data);
			}
			m_serializedJson = FGOLMiniJSON.Json.Serialize(serializedDatas);
			//m_serializedJson = new JsonFormatter().PrettyPrint(m_serializedJson);		// [AOC] Optionally, for debug purposes mainly
		}

		/// <summary>
		/// Load from the json.
		/// </summary>
		public void LoadFromJson() {
			// [AOC] Since Unity doesn't serialize custom classes inheriting from an abstract class, 
			//		 we'll serialize it on our own via reflection into a json
			//		 Copied from HSX

			// Parse the JSON
			if(string.IsNullOrEmpty(m_serializedJson)) m_serializedJson = "[]";	// [AOC] Make sure it's a valid json string
			List<object> serializedDatas = FGOLMiniJSON.Json.Deserialize(m_serializedJson) as List<object>;

			// Clear components data list
			m_componentsData.Clear();

			// Iterate all the elements in the json array - each one corresponds to a component data object
			for(int i = 0; i < serializedDatas.Count; i++) {
				// Create a new component data kvp and initialize it with the json's values
				StateComponentDataKVP newKvp = new StateComponentDataKVP();
				Dictionary<string, object> data = serializedDatas[i] as Dictionary<string, object>;

				// Create and initialize the data object
				newKvp.typeName = data["dataType"] as string;
				Type dataType = Type.GetType(newKvp.typeName);
				newKvp.data = (StateComponentData)Activator.CreateInstance(dataType);
				newKvp.data.Deserialize(data);

				// Add other custom values
				newKvp.folded = (bool)data["editorFolded"];

				// Store into the components data list
				m_componentsData.Add(newKvp);
			}
		}
	}
}