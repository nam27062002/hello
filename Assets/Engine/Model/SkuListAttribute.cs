// SkuListAttribute.cs
// 
// Created by Alger Ortín Castellví on 18/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Specialization of the List attribute to select from the list of skus of a given
/// definitions category.
/// Usage: [SkuList(Definitions.Category.TARGET_CATEGORY)]
/// </summary>
public class SkuListAttribute : ListAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public DefinitionsCategory m_category = DefinitionsCategory.UNKNOWN;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_category">The type of definition to be parsed.</param>
	/// <param name="_allowNullValue">If set to <c>true</c>, the "NONE" option will be available.</param>
	public SkuListAttribute(DefinitionsCategory _category, bool _allowNullValue = true) {
		m_category = _category;
		ValidateOptions();
	}

	/// <summary>
	/// Make sure the options array is updated.
	/// </summary>
	public override void ValidateOptions() {
		// Get sku list
		List<string> skus = DefinitionsManager.GetSkuList(m_category);

		// Convert to object array
		m_options = skus.Cast<string, object>().ToArray();
	}
}

