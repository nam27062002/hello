// ProbabilitySet.cs
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple class to distribute a probability of 1 within a collection via a nice
/// inspector interface (Humble Bundle style).
/// </summary>
[Serializable]
public class ProbabilitySet {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar class representing an element of the probability set.
	/// </summary>
	[Serializable]
	public class Element {
		//--------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											  //
		//--------------------------------------------------------------------//
		public float value = 0f;
		public string label = "";
		public bool locked = false;

		//--------------------------------------------------------------------//
		// GENERIC METHODS													  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		/// <param name="_label">Label.</param>
		/// <param name="_locked">If set to <c>true</c> locked.</param>
		public Element(string _label, bool _locked = false) {
			label = _label;
			locked = _locked;
		}
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Element[] m_elements = new Element[0];

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initializes a new instance of the <see cref="ProbabilitySet"/> class.
	/// All values will be reset.
	/// Usage:
	/// <c>
	/// ProbabilitySet myProbabilitySet = new ProbabilitySet(new Element[] {
	/// 	new ProbabilitySet.Element("label0"), 
	/// 	new ProbabilitySet.Element("label1", true), 
	/// 	new ProbabilitySet.Element("label2"), 
	/// 	new ProbabilitySet.Element("label3")
	/// });
	/// </c>
	/// </summary>
	/// <param name="_elements">Elements.</param>
	public ProbabilitySet(Element[] _elements) {
		// Store elements
		m_elements = _elements;

		// Distribute probability uniformly at start
		for(int i = 0; i < m_elements.Length; i++) {
			m_elements[i].value = 1f/(float)(m_elements.Length);
		}
	}
}