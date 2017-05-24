//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom attribute to select a prefab name from a list containing all the prefabs inside Resources/Game/Entities/Junk
/// Usage: [EntityJunkPrefabListAttribute]
/// Usage: [EntityJunkPrefabListAttribute(true)]
/// </summary>
public class EntityJunkPrefabListAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public bool m_allowNullValue = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_allowNullValue">If set to <c>true</c>, the "NONE" option will be available.</param>
	public EntityJunkPrefabListAttribute(bool _allowNullValue = false) {
		m_allowNullValue = _allowNullValue;
	}
}

