// Group.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Default behaviour to be added to a group within a level.
	/// </summary>
	[ExecuteInEditMode]
	public class Group : MonoBehaviour {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		// Mandatory objects names
		public static readonly string GROUND = "Ground";		// Only ref physical objects for level design, won't be used in game
		public static readonly string TERRAIN = "Terrain";		// Art representation of the ground
		public static readonly string SPAWNERS = "Spawners";	// All game spawners
		public static readonly string DECO = "Deco";			// Random art decorations
		public static readonly string EDITOR = "Editor";		// Internal editor stuff, won't make it to the game

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		// Mandatory objects
		private GameObject m_groundObj = null;
		public GameObject groundObj { get { return CreateIfNull(ref m_groundObj, GROUND); }}

		private GameObject m_terrainObj = null;
		public GameObject terrainObj { get { return CreateIfNull(ref m_terrainObj, TERRAIN); }}

		private GameObject m_spawnersObj = null;
		public GameObject spawnersObj { get { return CreateIfNull(ref m_spawnersObj, SPAWNERS); }}

		private GameObject m_decoObj = null;
		public GameObject decoObj { get { return CreateIfNull(ref m_decoObj, DECO); }}

		private GameObject m_editorObj = null;
		public GameObject editorObj { get { return CreateIfNull(ref m_editorObj, EDITOR); }}

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		private void Awake() {
			// Make ouselves static, we don't want to accidentally move the parent object
			this.gameObject.isStatic = true;
		}

		/// <summary>
		/// First update.
		/// </summary>
		private void Start() {
		
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update() {

		}

		/// <summary>
		/// Destructor.
		/// </summary>
		private void OnDestroy() {

		}

		//------------------------------------------------------------------//
		// INTERNAL METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Very specific internal usage. Checks wether the input object is null.
		/// If it's not, returns it without any modifications.
		/// If it is, looks for a game object within the group with the given name.
		/// If found, makes sure it's in the right hierarchy and assigns it to the param object (and returns it).
		/// If not found, creates a new object and does the same with it.
		/// </summary>
		/// <returns>The passed object, properly initialized.</returns>
		/// <param name="_obj">The object to be checked.</param>
		/// <param name="_name">The name identifying to the object.</param>
		private GameObject CreateIfNull(ref GameObject _obj, string _name) {
			// a) Object is valid
			if(_obj != null) return _obj;
			
			// b) Object exists in the hierarchy - only care if it's an immediate children, allowing us to put groups as children of other groups
			foreach(Transform t in this.transform) {
				if(t.name == _name) {
					_obj = t.gameObject;
					break;
				}
			}
			
			// c) Object doesn't exist
			if(_obj == null) {
				_obj = new GameObject(_name);
				_obj.transform.position = this.transform.position;	// Trick to place the new object at 0,0,0 after the SetParent call
			}
			
			// Put it in the right place
			_obj.transform.SetParent(this.transform, true);

			// Make sure it's static, we don't want to accidentally move the parent object
			_obj.isStatic = true;

			// Some objects must be in specific layers
			if(_name == EDITOR) {
				_obj.SetLayerRecursively("LevelEditor");
			}
			
			return _obj;
		}
	}
}

