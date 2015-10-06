// Level.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Default behaviour to be added to any editable level.
/// </summary>
[ExecuteInEditMode]
public class Level : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Mandatory objects names
	public static readonly string EDITOR = "Editor";
	public static readonly string TERRAIN = "Terrain";
	public static readonly string SPAWNERS = "Spawners";
	public static readonly string DECO = "Deco";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Mandatory objects
	private GameObject m_editorObj = null;
	public GameObject editorObj { get { return m_editorObj; }}

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
		m_editorObj = InitComponent(EDITOR);
		m_terrainObj = InitComponent(TERRAIN);
		m_spawnersObj = InitComponent(SPAWNERS);
		m_decoObj = InitComponent(DECO);
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
	/// <param name="_componentName">_component name.</param>
	private GameObject InitComponent(string _componentName) {
		// Look for the object
		GameObject obj = GameObject.Find(_componentName);

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

		// Everything fine, return the object
		return obj;
	}
}

