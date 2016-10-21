// ProbabilitySet.cs
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

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
		// The weight of this element within the probability set. [0..1], always use the setter.
		[SerializeField] private float m_probability = 0f;
		public float probability {
			get { return m_probability; }
			set { m_probability = Mathf.Clamp01(value); }
		}

		// Label in Unity's editor inspector
		[SerializeField] private string m_label = "";
		public string label {
			get { return m_label; }
			set { m_label = value; }
		}

		// Whether to adjust this element's probabilty when other elements in the set change
		[SerializeField] private bool m_locked = false;
		public bool locked {
			get { return m_locked; }
			set { m_locked = value; }
		}

		//--------------------------------------------------------------------//
		// GENERIC METHODS													  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Empty constructor with default values.
		/// </summary>
		public Element() {
			// Using default values
		}

		/// <summary>
		/// Parametrized constructor.
		/// </summary>
		/// <param name="_label">Label.</param>
		/// <param name="_locked">If set to <c>true</c> locked.</param>
		public Element(string _label, bool _locked = false) {
			label = _label;
			locked = _locked;
			m_probability = 0f;
		}
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Collection of elements in the set
	[SerializeField] private List<Element> m_elements = new List<Element>();

	// Amount of elements in the set
	public int numElements { 
		get { return m_elements.Count; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Empty constructor.
	/// </summary>
	public ProbabilitySet() {

	}

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
		if(_elements != null) m_elements = new List<Element>(_elements);

		// Distribute probability uniformly at start
		Reset(false);
	}

	//------------------------------------------------------------------------//
	// ELEMENT MANAGEMENT METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Add a new element at the end of the set. Will be initialized with probability 0.
	/// </summary>
	/// <returns>The newly added element.</returns>
	/// <param name="_label">Label for the new element.</param>
	public Element AddElement(string _label) {
		// Create a new element with the given label
		Element newElement = new Element(_label);

		// Give it 0 value to avoid affecting the global balance, unless there's only one element, in which case we'll give it value 1
		if(numElements == 0) {
			newElement.probability = 1f;
		} else {
			newElement.probability = 0f;
		}

		// Add it to the list and return it - no need to redistribute since default probability is 0
		m_elements.Add(newElement);
		return newElement;
	}

	/// <summary>
	/// Add a new element and set a probability to it.
	/// Consistency of the set (aka total of the probabilities is 1) is not guaranteed after this method is called.
	/// You can call Validate() after all elements have been added to make sure the set is valid.
	/// </summary>
	/// <returns>The newly added element.</returns>
	/// <param name="_label">Label for the new element.</param>
	/// <param name="_probability">Probability for the new element [0..1].</param>
	public Element AddElement(string _label, float _probability) {
		// Just add new element and set its probability
		Element e = AddElement(_label);
		e.probability = _probability;
		return e;
	}

	/// <summary>
	/// Removes last element of the set.
	/// All other elements probabilities will be readjusted, regardless of their lock status.
	/// </summary>
	/// <returns>The newly added element.</returns>
	/// <param name="_label">Label for the new element.</param>
	public void RemoveElement() {
		// Ignore if there are no elements to remove
		if(numElements <= 0) return;

		// Get element to be removed
		Element toRemove = GetElement(numElements - 1);

		// Before removing, redistribute values simulating that the element we will remove goes to 0
		toRemove.probability = 0f;		// Set probability to 0
		Redistribute(true, toRemove);	// Redistribute, including locked elements for the case where all remaining elements are locked

		// Remove element from the list
		m_elements.Remove(toRemove);
	}

	/// <summary>
	/// Obtain the element at the given index.
	/// </summary>
	/// <returns>The element at index <paramref name="_idx"/>. <c>null</c> if index not valid.</returns>
	/// <param name="_idx">The index of the element we want.</param>
	private Element GetElement(int _idx) {
		if(_idx < 0 || _idx >= numElements) return null;
		return m_elements[_idx];
	}

	//------------------------------------------------------------------------//
	// ELEMENTS ACCESSORS													  //
	// We want to control any change on the elements, so don't allow direct   //
	// access to them														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Obtain the probability value of an element in this set.
	/// </summary>
	/// <returns>The probability [0..1] of the element with index <paramref name="_elementIdx"/>. <c>-1</c> if given index is not valid.</returns>
	/// <param name="_elementIdx">The index of the element to be checked.</param>
	public float GetProbability(int _elementIdx) {
		// Check params
		if(_elementIdx < 0 || _elementIdx >= numElements) return -1;

		// Return requested value
		return m_elements[_elementIdx].probability;
	}

	/// <summary>
	/// Define the probability of the target element.
	/// The probabilities of the whole set will be redistributed accordingly.
	/// Locked status of the target element will be ignored when calling this method.
	/// </summary>
	/// <param name="_elementIdx">The index of the element to be modified.</param>
	/// <param name="_probability">The new probability [0..1] for the target element.</param>
	/// <param name="_redistribute">If <c>true></c>, all probabilities will be adjusted to add up 1. Be careful when using <c>false</c>, since you could end up with an invalid probability set. You can call Validate() after having set all teh probabilities to make sure the set is valid.,</param>
	public void SetProbability(int _elementIdx, float _probability, bool _redistribute = true) {
		// Check params
		if(_elementIdx < 0 || _elementIdx >= numElements) return;

		// In any case probability can't be outside range
		_probability = Mathf.Clamp01(_probability);

		// Store new value
		m_elements[_elementIdx].probability = _probability;

		// Redistribute
		if(_redistribute) Redistribute(false, m_elements[_elementIdx]);
	}

	/// <summary>
	/// Obtain the label of an element in this set.
	/// </summary>
	/// <returns>The label of the element with index <paramref name="_elementIdx"/>.</returns>
	/// <param name="_elementIdx">The index of the element to be checked.</param>
	public string GetLabel(int _elementIdx) {
		// Check params
		if(_elementIdx < 0 || _elementIdx >= numElements) return "";

		// Return requested value
		return m_elements[_elementIdx].label;
	}

	/// <summary>
	/// Define the label of the target element.
	/// </summary>
	/// <param name="_elementIdx">The index of the element to be modified.</param>
	/// <param name="_label">The new label for the target element.</param>
	public void SetLabel(int _elementIdx, string _label) {
		// Check params
		if(_elementIdx < 0 || _elementIdx >= numElements) return;

		// Set new value
		m_elements[_elementIdx].label = _label;
	}

	/// <summary>
	/// Whether an element is locked or not.
	/// </summary>
	/// <returns><c>true</c> if this element with index <paramref name="_elementIdx"/> is locked; otherwise, <c>false</c>.</returns>
	/// <param name="_elementIdx">The index of the element to be checked.</param>
	public bool IsLocked(int _elementIdx) {
		// Check params
		if(_elementIdx < 0 || _elementIdx >= numElements) return false;

		// Return requested value
		return m_elements[_elementIdx].locked;
	}

	/// <summary>
	/// Lock/Unlock the target element.
	/// </summary>
	/// <param name="_elementIdx">The index of the element to be modified.</param>
	/// <param name="_locked">Whether to lock or unlock the target element.</param>
	public void SetLocked(int _elementIdx, bool _locked) {
		// Check params
		if(_elementIdx < 0 || _elementIdx >= numElements) return;

		// Set new value
		m_elements[_elementIdx].locked = _locked;
	}

	//------------------------------------------------------------------------//
	// OPERATIONS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Gets the index of a random element weighted following the probability value assigned to it.
	/// </summary>
	/// <returns>The index of the selected element.</returns>
	public int GetWeightedRandomElementIdx() {
		// Select a random value [0..1]
		// Since the weights of all elements sum exactly 1, iterate through elements until the selected value is reached
		// This should match weighted probability distribution
		float targetValue = UnityEngine.Random.Range(0f, 1f);
		for(int i = 0; i < m_elements.Count; i++) {
			targetValue -= m_elements[i].probability;
			if(targetValue <= 0f) return i;
		}

		// Should never reach this point unless there are no elements on the set
		return -1;
	}

	/// <summary>
	/// Get a random element weighted following the probability value assigned to it.
	/// </summary>
	/// <returns>The selected random element.</returns>
	public Element GetWeightedRandomElement() {
		return GetElement(GetWeightedRandomElementIdx());
	}

	/// <summary>
	/// Reset all probabilities to be equally distributed.
	/// </summary>
	/// <param name="_respectLocks">Whether to respect or ignore the <c>locked</c> flag of the elements.</param>
	public void Reset(bool _respectLocks) {
		// Respect locks
		if(_respectLocks) {
			// Count non-locked elements and actual amount to distribute
			float nonLockedCount = 0f;
			float toDistribute = 1f;
			for(int i = 0; i < numElements; i++) {
				if(m_elements[i].locked) {
					toDistribute -= m_elements[i].probability;
				} else {
					nonLockedCount++;
				}
			}

			// Distribute remaining amount uniformly among non-locked elements
			float newProbabiltyPerElement = toDistribute/nonLockedCount;
			for(int i = 0; i < numElements; i++) {
				if(!m_elements[i].locked) {
					m_elements[i].probability = newProbabiltyPerElement;
				}
			}
		}

		// Don't respect locks
		else {
			// Distribute uniformly among all elements
			for(int i = 0; i < numElements; i++) {
				m_elements[i].probability = 1f/(float)numElements;
			}
		}
	}

	/// <summary>
	/// Determines whether this probability set is valid.
	/// A probability set is considered valid when the sum of probabilities of all
	/// its elements adds up to exactly <c>1</c>.
	/// </summary>
	/// <returns><c>true</c> if the set is valid.</returns>
	/// <param name="_errorMargin">Optional tolerance since some precision errors may occur when redistributing.</param>
	public bool IsValid(float _errorMargin = 0.0001f) {
		float totalProb = 0f;
		for(int i = 0; i < m_elements.Count; i++) {
			totalProb += m_elements[i].probability;
		}
		return MathUtils.IsBetween(totalProb, 1f - _errorMargin, 1f + _errorMargin);
	}

	/// <summary>
	/// If the set is not valid, force a redistribution, ignoring locks and everything.
	/// </summary>
	public void Validate() {
		if(!IsValid()) Redistribute(true);
	}

	/// <summary>
	/// Redistribute probability proportionally between all elements to make sure
	/// all probabilities add up 1.
	/// </summary>
	/// <param name="_overrideLocks">If set to <c>true</c>, locked sliders will be modified too.</param> 
	/// <param name="_skipElement">Single element to be skipped from the redistribution (i.e. the element that has just been modified).</param>
	public void Redistribute(bool _overrideLocks = false, Element _skipElement = null) {
		// [AOC] From HumbleBundle's web page code:
		// Calculate the splits for all the elements.
		// Conceptually, we remove the active slider from the mix. Then we normalize the siblings to 1 to
		// determine their weights relative to each other. Then we divide the split that is left over from the moved
		// slider with these relative weights.

		// Amount to distribute between the elements
		float totalToDistribute = 1f;

		// Compute total amount of unlocked elements to distribute proportionally
		// Adjust amount to distribute by ignoring locked sliders
		float unlockedTotal = 0f;
		float unlockedCount = 0f;	// [AOC] Use float directly to avoid casting later on
		float lockedCount = 0f;
		for(int i = 0; i < numElements; i++) {
			// Remove from the total to distribute locked elements and element to skip
			if(m_elements[i] == _skipElement) {
				totalToDistribute -= _skipElement.probability;
			} else if(!_overrideLocks && m_elements[i].locked) {
				totalToDistribute -= m_elements[i].probability;
				lockedCount++;
			} else {
				unlockedTotal += m_elements[i].probability;
				unlockedCount++;
			}
		}

		// Locked sliders may limit new value, check it here
		// If the amount to distribute is negative (too many locked items), add (subtract) that amount to changed element
		// If we're not targeting any element, just ignore excess of probability
		if(!_overrideLocks && totalToDistribute < 0f) {
			if(_skipElement != null) _skipElement.probability += totalToDistribute;
			totalToDistribute = 0;
		}

		// Compute and assign new value to each element based on its weight
		float remainingToDistribute = totalToDistribute;
		for(int i = 0; i < numElements; i++) {
			// Skip current
			if(m_elements[i] == _skipElement) continue;

			// Skip if locked
			if(!_overrideLocks && m_elements[i].locked) continue;

			// Compute relative weight of this element in relation to all active elements
			float elementWeight = 0;
			if(unlockedTotal == 0f) {
				// If all elements except the target one are at 0, we split the movement evently amongst them
				elementWeight = 1f/unlockedCount;
			} else {
				elementWeight = m_elements[i].probability/unlockedTotal;
			}

			// Compute new value for the sibling and store it to the property
			m_elements[i].probability = totalToDistribute * elementWeight;
			remainingToDistribute -= m_elements[i].probability;
		}

		// If not everything could be distributed, limit the movement of the target slider.
		// This could happen when all elements are either locked or already 0
		if(remainingToDistribute > 0f && _skipElement != null) {
			_skipElement.probability += remainingToDistribute;
		}
	}
}