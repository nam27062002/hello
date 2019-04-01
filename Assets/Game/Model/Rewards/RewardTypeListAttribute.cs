// RewardTypeListAttribute.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Helper attribute to display a list of all Metagame.Reward supported types.
/// Usage: [RewardTypeList(_allowEmptyValue)]
/// </summary>
public class RewardTypeListAttribute : ListAttribute {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private bool m_allowEmptyValue = true;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_allowEmptyValue">If set to <c>true</c>, the "NONE" option will be available.</param>
	public RewardTypeListAttribute(bool _allowEmptyValue = false) {
		m_allowEmptyValue = _allowEmptyValue;
		ValidateOptions();
	}

	/// <summary>
	/// Make sure the options array is updated.
	/// </summary>
	public override void ValidateOptions() {
		// Get a list of all types inheriting from Metagame.Reward
		List<Type> derivedTypes = TypeUtil.FindAllDerivedTypes(typeof(Metagame.Reward));

		// Fill options list
		// Filter only those containing a TYPE_CODE constant
		List<object> typeCodes = new List<object>();
		for(int i = 0; i < derivedTypes.Count; ++i) {
			FieldInfo field = derivedTypes[i].GetField("TYPE_CODE", BindingFlags.Public | BindingFlags.Static);
			if(field != null) {
				typeCodes.Add(field.GetValue(null));   // No need to give an instance since it's a static field :)
			}
		}

		// Add the empty option if required
		if(m_allowEmptyValue) {
			typeCodes.Insert(0, "");
		}

		// Convert to object array
		m_options = typeCodes.ToArray();
	}
}