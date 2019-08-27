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
	public abstract class AIPilot : Pilot, ISerializationCallbackReceiver {
		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
		[SerializeField] private StateMachine m_brainResource;
		public StateMachine brainResource { get { return m_brainResource; }}

		[SerializeField] private Range m_railSeparation = new Range(0.5f, 1f);
		protected override float railSeparation { get { return m_railSeparation.GetRandom(); } }

		[SerializeField] private bool m_useSpawnerRotation = false;

		private float m_speedFactor = 1f;
		public override float speedFactor { get { return m_speedFactor; } set { m_speedFactor = value; } }


		//--------------------------------------------------------------------//
		public virtual bool avoidCollisions { get { return false; } set { } }
		public virtual bool avoidWater		{ get { return false; } set { } }

		private StateMachine m_brain;
		public StateMachine brain{ get{ return m_brain; } }

		protected Vector3 m_homePosition;
		public Vector3 homePosition { get { return m_homePosition; } }

		protected Transform m_homeTransform;
		public Transform homeTransform { get { return m_homeTransform; } set { m_homeTransform = value; } }

		protected Vector3 m_target;
		public override Vector3 target { get { return m_target; } }

		protected bool m_slowDown;


		//--------------------------------------------------------------------//
		// METHODS															  //
		//--------------------------------------------------------------------//
		public override void Spawn(ISpawner _spawner) {
			Vector3 pos = m_transform.position;
			pos.z += zOffset;
            m_transform.position = pos;

			SetArea(_spawner);

			if (m_useSpawnerRotation) {
				Quaternion rot = GameConstants.Quaternion.identity;
				if (_spawner != null) {
					rot = _spawner.rotation;
				}
				m_direction = rot * GameConstants.Vector3.forward;
				m_machine.upVector = rot * m_machine.upVector;
			} else { 
				if (UnityEngine.Random.Range(0f, 1f) < 0.5f) {
					m_direction = GameConstants.Vector3.right;
				} else {
					m_direction = GameConstants.Vector3.left;
				}
			}
			m_directionForced = false;

			Stop();

			m_target = m_transform.position;
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
            if (_spawner == null) {
                m_area = new RectAreaBounds(m_transform.position, Vector3.one * 2f);
                m_homePosition = m_transform.position;
                m_guideFunction = null;                
            } else {
                m_area = _spawner.area;
				m_homePosition = m_transform.position;
				m_guideFunction = _spawner.guideFunction;
			}

			m_target = m_homePosition;
		}

		void OnDisable() {
		
		}

		void OnDestroy() {
			if (m_brain) {
				m_brain.OnDestroy();
			}
		}

		public override void BrainExit() {
			if (m_brain != null) m_brain.Exit();
		}

		public override void OnTrigger(int _trigger, object[] _param = null) {
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

		public override void CustomUpdate() {
			base.CustomUpdate();

			// state machine updates
			if (m_brain != null) {
				if (!(m_machine.IsDead() || m_machine.IsDying() /*|| m_machine.GetSignal(Signals.Type.InLove)*/ )) {
					m_brain.Update();
				}
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

		// [AOC] Unfortunately Unity doesn't serialize custom abstract classes, nor behaves well with inheritance,
		//		 so we must manage serialization of the state component data on our own.
		// See http://docs.unity3d.com/Manual/script-Serialization.html
		// See HSX's GameData class
		[SerializeField][HideInInspector] private string m_serializedJson = "[]";

        // [MSF] We can't access the prefab name while serializing, so we need to store a Key to save and retrieve its data.
        //          We'll use the prefab name as a Key. The editor will update this value.
        [SerializeField]
        [HideInInspector]
        private string m_databaseKey = "";
        public string databaseKey { get { return m_databaseKey; } set { m_databaseKey = value; } }

        /// <summary>
        /// Gets the data linked to the target component type.
        /// </summary>
        /// <returns>The component data linked to the requested component type. <c>null</c> if there is no data for the given type.</returns>
        /// <typeparam name="T">Type of the state component whose data we want.</typeparam>
        public T GetComponentData<T>() where T : StateComponentData {
            // Since Unity doesn't serialize System.Type, use the Type.FullName to compare types
            string typeName = typeof(T).FullName;
            StateComponentDataKVP kvp = BrainDataBase.instance.GetDataFor(m_databaseKey, typeName);
            /*if (kvp != null){return (T)kvp.data;}*/

            try {
                T data = (T)kvp.data;
                return data;
            } catch {
                Fabric.Crashlytics.Crashlytics.RecordCustomException("Pilot - GetComponentData", "Data is NULL", "NPC " + name + " has a null value on " + typeName + " behaviour.");
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
                BrainDataBase.instance.ClearDataFor(m_databaseKey);
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
					if(validatedDataTypes.Add(typeName)) {      // Returns true if the value was not already in the hash
                        // Check whether we have a data object for this component type
                        // Inefficient, but since it's an editor code, we don't care
                        StateComponentDataKVP kvp = BrainDataBase.instance.GetDataFor(m_databaseKey, typeName);


                        // If data wasn't found, create one and add it to the pilot
                        if (kvp == null) {
							kvp = new StateComponentDataKVP();
							kvp.typeName = typeName;
							kvp.data = component.CreateData();  // [AOC] CreateData() will create the proper data object for this specific component type
                            BrainDataBase.instance.AddDataFor(m_databaseKey, kvp);
                        }
                    }
				}
			}
            
            BrainDataBase.instance.ValidateTypes(m_databaseKey, validatedDataTypes);
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
			LoadFromJson(false);
		}

        /// <summary>
        /// Save to the json string.
        /// </summary>
        public void SaveToJson() {
            // [AOC] Since Unity doesn't serialize custom classes inheriting from an abstract class, 
            //		 we'll serialize it on our own via reflection into a json
            //		 Copied from HSX

            if (string.IsNullOrEmpty(m_databaseKey)) {
                return;
            }

            // Make sure all required data components for this pilot are created
            // [AOC] Doing it every time can be costly, try to figure out a better way
            ValidateComponentsData();

            // Put all data into a JSON
            List<object> serializedDatas = new List<object>();
            Dictionary<string, StateComponentDataKVP> componentsData = BrainDataBase.instance.GetDataFor(m_databaseKey);
            if (componentsData != null) {
                foreach (StateComponentDataKVP componentData in componentsData.Values) {
                    // Create a new data dictionary for this component data
                    Dictionary<string, object> data;

                    // Add data structure fields
                    data = componentData.data.Serialize();
                    data.Add("dataType", componentData.typeName);

                    // Add extra info fields
                    data.Add("editorFolded", componentData.folded);

                    // Store it to the list
                    serializedDatas.Add(data);
                }
                m_serializedJson = FGOLMiniJSON.Json.Serialize(serializedDatas);
                //m_serializedJson = new JsonFormatter().PrettyPrint(m_serializedJson);		// [AOC] Optionally, for debug purposes mainly

                BrainDataBase.instance.ClearDataFor(m_databaseKey);
            }
        }

        /// <summary>
        /// Load from the json.
        /// </summary>
		public void LoadFromJson(bool _force) {
            // [AOC] Since Unity doesn't serialize custom classes inheriting from an abstract class, 
            //		 we'll serialize it on our own via reflection into a json
            //		 Copied from HSX

            if (string.IsNullOrEmpty(m_databaseKey)) {
                return;
            }

			if (_force || !BrainDataBase.instance.HasDataFor(m_databaseKey)) {
                // Parse the JSON
                if (string.IsNullOrEmpty(m_serializedJson)) m_serializedJson = "[]";    // [AOC] Make sure it's a valid json string
                List<object> serializedDatas = FGOLMiniJSON.Json.Deserialize(m_serializedJson) as List<object>;

                // Iterate all the elements in the json array - each one corresponds to a component data object
                for (int i = 0; i < serializedDatas.Count; i++) {
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
                    BrainDataBase.instance.AddDataFor(m_databaseKey, newKvp);
                }
            }
        }
    }
}