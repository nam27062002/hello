using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class DefinitionNode 
{
	private Dictionary<string, string> properties;

	/// <summary>
	/// It stores the original value for some properties that have been rewritten by calling <c>changeValue()</c>, passing <c>true</c> to the
    /// <c>keepOriginalValue</c> parameter. This is useful to implement some game modes that change the rules temporarily.
	/// </summary>
	private Dictionary<string, string> originalValues;
	
	public DefinitionNode()
	{
		properties = new Dictionary<string, string>();
	}
	
	public void LoadFromXml( XmlNode xml )
	{
		XmlAttributeCollection list = xml.Attributes;
		foreach( XmlAttribute attr in list)
		{
			properties.Add( attr.Name, attr.Value);
		}
		
	}
	
	public bool Has( string property )
	{
		return properties.ContainsKey( property );
	}
	
	public string Get( string property )
	{
		if ( properties.ContainsKey(property) )
			return properties[property];
		else
			return "";
	}
	
	public float GetAsFloat( string property, float defaultValue = 1.0f)
	{
		float result = 0;
		if (float.TryParse( Get(property), out result))
			return result;
		else
			return defaultValue;
	}
	
	public double GetAsDouble( string property, double defaultValue = 1.0)
	{
		double result = 0;
		if (double.TryParse( Get(property), out result))
			return result;
		else
			return defaultValue;
	}
	
	public int GetAsInt( string property, int defaultValue = 0)
	{
		int result = 0;
		if (int.TryParse( Get(property), out result))
			return result;
		else
			return defaultValue;
	}
	
	public long GetAsLong( string property, long defaultValue = 0 )
	{
		long result = 0;
		if ( long.TryParse(Get(property), out result) )
			return result;
		else
			return defaultValue;
	}
	
	public bool GetAsBool( string property, bool defaultValue = false )
	{
		bool result = false;
		string value = Get(property);
		if (bool.TryParse (value, out result))
			return result;
		else if (value == "true")
			return true;

		return defaultValue;
	}

	public bool ContainsProperty(string property)
	{
		return properties != null && properties.ContainsKey(property);
	}

	public void changeValueByCustomizer(string property, string value)
	{
		changeValue(property, value, false);
	}

	public void changeValue(string property, string value, bool keepOriginalValue)
	{
		if (ContainsProperty(property))
		{
			if (keepOriginalValue)
			{
				if (originalValues == null)
				{
					originalValues = new Dictionary<string, string>();
				}

				if (!originalValues.ContainsKey(property))
				{
					originalValues[property] = properties[property];
				}
			}

			properties[property] = value;
		}
	}
	
	public void SetValue( string property, string value)
	{
		if ( ContainsProperty( property ) )
		{
			properties[ property ] = value;
		}
		else
		{
			properties.Add( property, value);
		}
	}

	public void ResetToOriginalValues()
	{
		if (originalValues != null)
		{
			foreach(  KeyValuePair<string, string> property in originalValues )
			{
				changeValue(property.Key, property.Value, false); 
			}

			originalValues = null;
		}
	}

	public void Dump()
	{
		Debug.Log("===========================================");
		foreach( KeyValuePair<string, string> p  in properties)
		{
			Debug.Log( p.Key +": "+p.Value );
		}
		Debug.Log("===========================================");
	}
}
