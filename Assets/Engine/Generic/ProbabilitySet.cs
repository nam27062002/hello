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
	/// For now only designed to be setup from the inspector.
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

	public int numElements { get { return m_elements.Length; }}

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
		if(_elements != null) m_elements = _elements;

		// Distribute probability uniformly at start
		for(int i = 0; i < m_elements.Length; i++) {
			m_elements[i].value = 1f/(float)(m_elements.Length);
		}
	}

	/// <summary>
	/// Gets the index of a random element weighted following the probability value assigned to it.
	/// </summary>
	/// <returns>The index of the selected element.</returns>
	public int GetWeightedRandomElement() {
		// Select a random value [0..1]
		// Since the weights of all elements sum exactly 1, iterate through elements until the selected value is reached
		// This should match weighted probability distribution
		float targetValue = UnityEngine.Random.Range(0f, 1f);
		for(int i = 0; i < m_elements.Length; i++) {
			targetValue -= m_elements[i].value;
			if(targetValue <= 0f) return i;
		}

		// Should never reach this point unless there are no elements on the set
		return -1;
	}
}