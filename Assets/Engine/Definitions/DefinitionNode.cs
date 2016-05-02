// Definitions.cs
// 
// Imported by Miguel Angel Linares
// Refactored by Alger Ortín Castellví on 04/03/2016
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Linq;
using System.Globalization;

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
	private Dictionary<string, DefinitionNode> m_childNodesBySku;
	private Dictionary<string, List<DefinitionNode>> m_childNodesByTag;

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

	// Original tag of the xml node
	private string m_tag = "";
	public string tag {
		get { return m_tag; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public DefinitionNode() {
		m_properties = new Dictionary<string, string>();
		m_childNodesBySku = new Dictionary<string, DefinitionNode>();
		m_childNodesByTag = new Dictionary<string, List<DefinitionNode>>();
	}

	/// <summary>
	/// Initializer.
	/// </summary>
	/// <param name="xml">Source xml node.</param>
	public void LoadFromXml(XmlNode xml) {
		// Store tag
		m_tag = xml.LocalName;

		// Store properties
		XmlAttributeCollection list = xml.Attributes;
		foreach(XmlAttribute attr in list) {
			// Store sku apart
			if(attr.Name == "sku") m_sku = attr.Value;

			m_properties.Add(attr.Name, attr.Value);
		}

		// Parse nested nodes
		int autoSkuGenerationIdx = 0;
		StringBuilder sb = new StringBuilder();
		foreach(XmlNode childNode in xml.ChildNodes) {
			// Create and parse child node
			DefinitionNode childDef = new DefinitionNode();
			childDef.LoadFromXml(childNode);

			// If the node didn't have a sku (quite normal in nested nodes), generate one automatically
			if(string.IsNullOrEmpty(childDef.sku)) {
				// [AOC] StringBuilder.Clear() is only available for .NET 4 and above -_-
				//		 This does the trick for previous versions such as Unity's Mono
				sb.Length = 0;
				childDef.m_sku = sb.Append("auto_sku_").Append(autoSkuGenerationIdx).ToString();
				autoSkuGenerationIdx++;
			}

			// Add to the sku dictionary
			if(!m_childNodesBySku.ContainsKey(childDef.sku)) {
				m_childNodesBySku.Add(childDef.sku, childDef);
			} else {
				Debug.LogError("This DefinitionNode (" + sku + ") already contains a child node with sku " + childDef.sku);
			}

			// Add to the by tag dictionary
			if(!m_childNodesByTag.ContainsKey(childDef.tag)) {
				m_childNodesByTag.Add(childDef.tag, new List<DefinitionNode>());
			}
			m_childNodesByTag[childDef.tag].Add(childDef);
		}
	}

	//------------------------------------------------------------------------//
	// BASIC GETTERS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get a list with the IDs of all the properties in this definition.
	/// </summary>
	/// <returns>The list of properties in this definition.</returns>
	public List<string> GetPropertyList() {
		return m_properties.Keys.ToList();
	}

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
	/// Generic getter implementation.
	/// Can be used to get child nodes as well.
	/// </summary>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	/// <typeparam name="T">Type of the property. Limited to string, numeric types and bool.</typeparam>
	public T Get<T>(string _property, T _defaultValue = default(T)) {
		// Use the internal ParseValue() method
		if(m_properties.ContainsKey(_property)) {
			return ParseValue<T>(m_properties[_property], _defaultValue);
		}

		// Special case for child nodes
		else if(typeof(T) == typeof(DefinitionNode)) {
			return (T)(object)GetChildNode(_property);
		}
		return _defaultValue;
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public string GetAsString(string _property, string _defaultValue = "") {
		return Get<string>(_property, _defaultValue);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public float GetAsFloat(string _property, float _defaultValue = 1.0f) {
		return Get<float>(_property, _defaultValue);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public double GetAsDouble(string _property, double _defaultValue = 1.0) {
		return Get<double>(_property, _defaultValue);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public int GetAsInt(string _property, int _defaultValue = 0) {
		return Get<int>(_property, _defaultValue);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public long GetAsLong(string _property, long _defaultValue = 0) {
		return Get<long>(_property, _defaultValue);
	}

	/// <summary>
	/// Get the property with the given id in the desired format.
	/// </summary>
	/// <returns>The value of the requested property in this definition, <paramref name="_defaultValue"/> if not found or type not appliable.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_defaultValue">The value to be returned if the property wasn't found in this definition.</param>
	public bool GetAsBool(string _property, bool _defaultValue = false) {
		return Get<bool>(_property, _defaultValue);
	}

	//------------------------------------------------------------------------//
	// SPECIAL GETTERS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get and localize a property using the current localization language.
	/// </summary>
	/// <returns>The value of the requested property in this definition, localized. Empty string if property not found.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_replacements">Replacement parameters, as they would be used in the Localization.Localize() method.</param>
	public string GetLocalized(string _property, params string[] _replacements) {
		if(m_properties.ContainsKey(_property)) {
			return Localization.Localize(m_properties[_property], _replacements);
		}
		return "";
	}

	/// <summary>
	/// Get as a list of values, defined in Excel as a single string with a separator
	/// character between items in the list.
	/// </summary>
	/// <returns>The given property as a list of values.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_separator">The separator used to split between items in the list.</param>
	/// <typeparam name="T">The type of the list elements.</typeparam>
	public List<T> GetAsList<T>(string _property, string _separator = ";") {
		// Get raw string value
		string strValue = GetAsString(_property);

		// Use the separator string to split the string value
		string[] splitResult = strValue.Split(new string[] { _separator }, StringSplitOptions.None);

		// Convert each split part into the target type
		List<T> finalList = new List<T>(splitResult.Length);
		for(int i = 0; i < splitResult.Length; i++) {
			finalList.Add(ParseValue<T>(splitResult[i]));
		}

		// Return the list
		return finalList;
	}

	/// <summary>
	/// Get as an array of values, defined in Excel as a single string with a separator
	/// character between items in the array.
	/// </summary>
	/// <returns>The given property as a array of values.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	/// <param name="_separator">The separator used to split between items in the array.</param>
	/// <typeparam name="T">The type of the array elements.</typeparam>
	public T[] GetAsArray<T>(string _property, string _separator = ";") {
		// Use list getter
		return GetAsList<T>(_property, _separator).ToArray();
	}

	/// <summary>
	/// Get a pair of properties as a range.
	/// The properties must share the same preffix <paramref name="_property"/>and 
	/// have the "Min" and "Max" suffixes respectively (e.g. <c>"healthMin"</c> and <c>"healthMax"</c>).
	/// </summary>
	/// <returns>A new range composed by the values of the properties named with the preffix <paramref name="_property"/>and the suffixes "Min" and "Max".</returns>
	/// <param name="_property">Property.</param>
	/// <param name="_defaultMin">The min value to be returned if the property wasn't found in this definition.</param>
	/// <param name="_defaultMax">The max value to be returned if the property wasn't found in this definition.</param>
	public Range GetAsRange(string _property, float _defaultMin = 0f, float _defaultMax = 1f) {
		Range newRange = new Range();
		newRange.min = GetAsFloat(_property + "Min", _defaultMin);
		newRange.max = GetAsFloat(_property + "Max", _defaultMax);
		return newRange;
	}

	/// <summary>
	/// Get a pair of properties as a range.
	/// The properties must share the same preffix <paramref name="_property"/>and 
	/// have the "Min" and "Max" suffixes respectively (e.g. <c>"healthMin"</c> and <c>"healthMax"</c>).
	/// </summary>
	/// <returns>A new range composed by the values of the properties named with the preffix <paramref name="_property"/>and the suffixes "Min" and "Max".</returns>
	/// <param name="_property">Property.</param>
	/// <param name="_defaultMin">The min value to be returned if the property wasn't found in this definition.</param>
	/// <param name="_defaultMax">The max value to be returned if the property wasn't found in this definition.</param>
	public RangeInt GetAsRangeInt(string _property, int _defaultMin = 0, int _defaultMax = 1) {
		RangeInt newRange = new RangeInt();
		newRange.min = GetAsInt(_property + "Min", _defaultMin);
		newRange.max = GetAsInt(_property + "Max", _defaultMax);
		return newRange;
	}

	/// <summary>
	/// Get the property with the given id as a color.
	/// Must be formatted as a rrggbb/rrggbbaa hex color string, with either the 
	/// prefix "0x", "#" or no prefix at all.
	/// </summary>
	/// <returns>The value of the property as a color.</returns>
	/// <param name="_property">The id of the property to be obtained.</param>
	public Color GetAsColor(string _property) {
		Color c = Colors.red;
		try {
			c = Colors.ParseHexString(GetAsString(_property).ToLowerInvariant());
		} catch {
			c = Colors.red;
		}
		return c;
	}

	//------------------------------------------------------------------------//
	// CHILD NODES MANAGEMENT												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get a definition node nested to this one.
	/// </summary>
	/// <returns>The child node with the given sku, <c>null</c> if not found.</returns>
	/// <param name="_sku">The identifier of the child node to be returned.</param>
	public DefinitionNode GetChildNode(string _sku) {
		if(m_childNodesBySku.ContainsKey(_sku)) {
			return m_childNodesBySku[_sku];
		}
		return null;
	}

	/// <summary>
	/// Get a list with all the definition nodes nested to this one.
	/// </summary>
	/// <returns>All the child nodes of this definition node.</returns>
	public List<DefinitionNode> GetChildNodes() {
		return m_childNodesBySku.Values.ToList();
	}

	/// <summary>
	/// Get a list with all the definition nodes nested to this one matching a specific tag.
	/// </summary>
	/// <returns>All the child nodes of this definition node.</returns>
	/// <param name="_tag">The xml tag of the child nodes.</param>
	public List<DefinitionNode> GetChildNodesByTag(string _tag) {
		if(m_childNodesByTag.ContainsKey(_tag)) {
			return m_childNodesByTag[_tag];
		} else {
			return new List<DefinitionNode>();
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Converts the given value, in string format, to a target type.
	/// </summary>
	/// <returns>The value parsed from the input string.</returns>
	/// <param name="_rawValue">The string representation of the value to be converted.</param>
	/// <param name="_defaultValue">A default value that will be returned if input value couldn't be parsed.</param>
	/// <typeparam name="T">The desired type of value.</typeparam>
	private T ParseValue<T>(string _rawValue, T _defaultValue = default(T)) {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] There might be a better way to do this, no time to research
		// [AOC] Double cast trick to prevent compilation errors: http://stackoverflow.com/questions/4092393/value-of-type-t-cannot-be-converted-to
		// [AOC] Important to use invariant culture when using C# native parsing methods! Othwerwise it will depend on current localization language and won't be properly read.
		Type t = typeof(T);

		// String
		if(t == typeof(string)) {
			return (T)(object)_rawValue;	// No treatment required!
		}

		// Float
		else if(t == typeof(float)) {
			float result = 0f;
			if(float.TryParse(_rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
				return (T)(object)result;
			}
		}

		// Double
		else if(t == typeof(double)) {
			double result = 0;
			if(double.TryParse(_rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
				return (T)(object)result;
			}
		}

		// Int or enum
		else if(t == typeof(int) || t.IsEnum) {
			int result = 0;
			if(int.TryParse(_rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
				return (T)(object)result;
			}
		}

		// Long
		else if(t == typeof(long)) {
			long result = 0;
			if(long.TryParse(_rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
				return (T)(object)result;
			}
		}

		// Bool
		else if(t == typeof(bool)) {
			// We will accept either text (any capitalization options) or numbers (0, > 0)
			string cleanRawValue = _rawValue.ToLowerInvariant();
			if(cleanRawValue == bool.TrueString.ToLowerInvariant()) {
				return (T)(object)true;
			} else if(cleanRawValue == bool.FalseString.ToLowerInvariant()) {
				return (T)(object)false;
			} else {
				// Numerical compare
				int intValue = ParseValue<int>(_rawValue, -1);
				if(intValue == 0) {
					return (T)(object)false;
				} else if(intValue > 0) {
					return (T)(object)true;
				}
			}
		}

		else {
			Debug.LogError("Type " + t.Name + " uncompatible with Definition's generic Get, returning default value");
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
		return ToStringInternal(0);
	}

	/// <summary>
	/// Helper method for ToString() allowing us to indent child nodes.
	/// </summary>
	/// <returns>A string that represents the current <see cref="DefinitionNode"/>.</returns>
	/// <param name="_indentationLevel">Indentation level.</param>
	private string ToStringInternal(int _indentationLevel) {
		// [AOC] One property per line, sku as header
		StringBuilder sb = new StringBuilder();

		// Header with tag
		sb.Append('\t', _indentationLevel);	// Indentation
		sb.Append("<").Append(tag).Append(">");

		// Sku first
		sb.Append('\n');	// New line
		sb.Append('\t', _indentationLevel + 1);	// Indentation (properties are indented in)
		sb.Append("sku: ").Append(sku);

		// Rest of the properties
		foreach(KeyValuePair<string, string> kvp  in m_properties) {
			// Skip if sku
			if(kvp.Key == "sku") continue;

			// Compose the string
			sb.Append('\n');	// New line
			sb.Append('\t', _indentationLevel + 1);	// Indentation (properties are indented in)
			sb.Append(kvp.Key).Append(": ").Append(kvp.Value);	// Key + value
		}

		// Child nodes
		// Recursive call, indented
		foreach(KeyValuePair<string, DefinitionNode> kvp in m_childNodesBySku) {
			sb.Append('\n');	// New line
			sb.Append(
				kvp.Value.ToStringInternal(_indentationLevel + 1)
			);
		}

		// Done!
		return sb.ToString();
	}
}
