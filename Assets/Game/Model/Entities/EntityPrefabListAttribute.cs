// EntityPrefabListAttribute.cs
// 
// Created by Miguel Ángel Linares on 7/09/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom attribute to select a prefab name from a list containing all the prefabs inside Resources/Game/Entities
/// Usage: [EntityPrefabListAttribute]
/// Usage: [EntityPrefabListAttribute(true)]
/// </summary>
public class EntityPrefabListAttribute : PropertyAttribute {
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
	public EntityPrefabListAttribute(bool _allowNullValue = false) {
		m_allowNullValue = _allowNullValue;
	}
}

