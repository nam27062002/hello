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
		public static readonly string GROUND = "Ground";
		public static readonly string TERRAIN = "Terrain";
		public static readonly string SPAWNERS = "Spawners";
		public static readonly string DECO = "Deco";

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//
		// Mandatory objects
		private GameObject m_groundObj = null;
		public GameObject groundObj { get { return m_groundObj; }}

		private GameObject m_terrainObj = null;
		public GameObject terrainObj { get { return m_terrainObj; }}

		private GameObject m_spawnersObj = null;
		public GameObject spawnersObj { get { return m_spawnersObj; }}

		private GameObject m_decoObj = null;
		public GameObject decoObj { get { return m_decoObj; }}

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		private void Awake() {
			// Get mandatory object references. If they don't exist, create them
			m_groundObj = InitComponent(GROUND);
			m_terrainObj = InitComponent(TERRAIN);
			m_spawnersObj = InitComponent(SPAWNERS);
			m_decoObj = InitComponent(DECO);

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
		/// Get the reference to one of the components of the scene. If the object is 
		/// not found, it will be created. If found outside the level object, it will 
		/// be moved into it.
		/// </summary>
		/// <param name="_componentName">The name of the component to be found.</param>
		private GameObject InitComponent(string _componentName) {
			// Look for the object within the group hierarchy
			//GameObject obj = GameObject.Find(_componentName);
			GameObject obj = gameObject.FindSubObject(_componentName);

			// Does object exist?
			if(obj == null) {
				// No! Create it
				obj = new GameObject(_componentName);
			}

			// Is object in the right hierarchy?
			if(obj.transform.parent != this.gameObject) {
				// No! Put it as child of the level object
				obj.transform.SetParent(this.transform, true);
			}

			// Make it static, we don't want to accidentally move the parent object
			obj.isStatic = true;

			// Everything fine, return the object
			return obj;
		}
	}
}

