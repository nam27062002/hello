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
	public string m_category = DefinitionsCategory.UNKNOWN;
	private bool m_allowNullValue = true;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_category">The type of definition to be parsed.</param>
	/// <param name="_allowNullValue">If set to <c>true</c>, the "NONE" option will be available.</param>
	public SkuListAttribute(string _category, bool _allowNullValue = true) {
		m_category = _category;
		ValidateOptions();
	}

	/// <summary>
	/// Make sure the options array is updated.
	/// </summary>
	public override void ValidateOptions() {
		// If definitions are not loaded, do it now
		if(!ContentManager.ready){
			ContentManager.InitContent();
		}

		// Get sku list
		// Create a duplicate, we are inserting data!!
		List<string> skus = new List<string>(DefinitionsManager.SharedInstance.GetSkuList(m_category));

		// Add the empty option if required
		if(m_allowNullValue) {
			skus.Insert(0, "");
		}

		// Convert to object array
		m_options = skus.Cast<string, object>().ToArray();
	}
}

