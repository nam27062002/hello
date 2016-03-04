// Definitions.cs
// 
// Imported by Miguel Angel Linares
// Refactored by Alger Ortín Castellví on 04/03/2016
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
public class DefinitionNode {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private Dictionary<string, string> m_properties;

	/// <summary>
	/// It stores the original value for some properties that have been rewritten by calling <c>ChangeValue()</c>, passing <c>true</c> to the
	/// <c>keepOriginalValue</c> parameter. This is useful to implement some game modes that change the rules temporarily.
	/// </summary>
	private Dictionary<string, string> m_originalValues;

	// Fast access to sku property, which all definitions should have
	private string m_sku = "";
	public string sku { 
		get { return m_sku; } 
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public DefinitionNode() {
		m_properties = new Dictionary<string, string>();
	}

	/// <summary>
	/// Initializer.
	/// </summary>
	/// <param name="xml">Source xml node.</param>
	public void LoadFromXml(XmlNode xml) {
		XmlAttributeCollection list = xml.Attributes;
		foreach(XmlAttribute attr in list) {
			// Store sku apart
			if(attr.Name == "sku") m_sku = attr.Value;

			m_properties.Add(attr.Name, attr.Value);
		}
		
	}

	//------------------------------------------------------------------------//
	// GETTERS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this definition contains a property with a specific id.
	/// </summary>
	/// <returns>Whether this definitions contains a property with the given id.</returns>
	/// <param name="_property">The id of the property to be checked.</param>
	public bool Has(string _property) {
		return m_properties.ContainsKey(_property);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, empty string if not found.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	public string Get(string _property) {
		return GetAsString(_property);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public string GetAsString(string _property, string _defaultValue = "") {
		if(m_properties.ContainsKey(_property)) {
			return m_properties[_property];
		}
		return _defaultValue;
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public float GetAsFloat(string _property, float _defaultValue = 1.0f) {
		float result = 0f;
		if(float.TryParse(Get(_property), out result)) {
			return result;
		}
		return _defaultValue;
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public double GetAsDouble(string _property, double _defaultValue = 1.0) {
		double result = 0;
		if(double.TryParse(Get(_property), out result)) {
			return result;
		}
		return _defaultValue;
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public int GetAsInt(string _property, int _defaultValue = 0) {
		int result = 0;
		if(int.TryParse(Get(_property), out result)) {
			return result;
		}
		return _defaultValue;
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public long GetAsLong(string _property, long _defaultValue = 0) {
		long result = 0;
		if(long.TryParse(Get(_property), out result)) {
			return result;
		}
		return _defaultValue;
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public bool GetAsBool(string _property, bool _defaultValue = false) {
		bool result = false;
		string value = Get(_property);
		if(bool.TryParse(value, out result)) {
			return result;
		} else if(value == "true") {
			return true;
		}
		return _defaultValue;
	}

	//------------------------------------------------------------------------//
	// CUSTOMIZER METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_property"></param>
	/// <param name="_newValue"></param>
	public void ChangeValueByCustomizer(string _property, string _newValue) {
		ChangeValue(_property, _newValue, false);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_property"></param>
	/// <param name="_newValue"></param>
	/// <param name="_keepOriginalValue"></param>
	public void ChangeValue(string _property, string _newValue, bool _keepOriginalValue) {
		if(Has(_property)) {
			if(_keepOriginalValue) {
				if(m_originalValues == null) {
					m_originalValues = new Dictionary<string, string>();
				}

				if(!m_originalValues.ContainsKey(_property)) {
					m_originalValues[_property] = m_properties[_property];
				}
			}

			m_properties[_property] = _newValue;
		}
	}

	/// <summary>
	/// Sets the value.
	/// </summary>
	/// <param name="_property">Property.</param>
	/// <param name="_newValue">New value.</param>
	public void SetValue(string _property, string _newValue) {
		if(Has(_property)) {
			m_properties[_property] = _newValue;
		} else {
			m_properties.Add(_property, _newValue);
		}
	}

	/// <summary>
	/// Reset all modified properties to their original values.
	/// </summary>
	public void ResetToOriginalValues() {
		if(m_originalValues != null) {
			foreach(KeyValuePair<string, string> property in m_originalValues) {
				ChangeValue(property.Key, property.Value, false); 
			}
			m_originalValues = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Returns a string representation of the current <see cref="DefinitionNode"/>.
	/// </summary>
	/// <returns>A string that represents the current <see cref="DefinitionNode"/>.</returns>
	public override string ToString() {
		// [AOC] One property per line, sku as header
		StringBuilder sb = new StringBuilder();

		// Header with sku
		sb.Append("<").Append(sku).Append(">");

		// Rest of the properties
		foreach(KeyValuePair<string, string> p  in m_properties) {
			// Skip if sku
			if(p.Key == "sku") continue;
			sb.Append("\n\t").Append(p.Key).Append(": ").Append(p.Value);
		}

		// Done!
		return sb.ToString();
	}
}
