using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

public static class TypeUtil {
	/// <summary>
	/// Find a Type by its class name.
	/// Can be either the assembly-qualified name (faster search) or the simple 
	/// name (much more expensive).
	/// The first Type matching the name will be returned. Check GetTypesByClassName method to get all Types matching the name.
	/// </summary>
	/// <returns>The target Type, <c>null</c> if no Type with the given name was found.</returns>
	/// <param name="_typeName">Type name to look for.</param>
	public static Type GetTypeByClassName(string _typeName) {
		// Try using Type directly (assembly-qualified name)
		Type type = Type.GetType(_typeName);
		if(type != null) return type;

		// Type not found, check all assemblies
		// Unfortunately Assembly.GetType(_typeName) only works with full type name, so we have to do a full search (sloooooow)
		type = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(_assembly => _assembly.GetTypes())
			.FirstOrDefault(_type => _type.Name == _typeName);
		return type;
	}

	/// <summary>
	/// Find all Types matching a given class name.
	/// Quite expensive, use carefully.
	/// </summary>
	/// <returns>All Types matching _typename, <c>null</c> if no Type with the given name was found.</returns>
	/// <param name="_typeName">Type name to look for.</param>
	public static Type[] GetTypesByClassName(string _typeName) {
		// Type not found, check all assemblies
		// Unfortunately Assembly.GetType(_typeName) only works with full type name, so we have to do a full search (sloooooow)
		IEnumerable<Type> matchingTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(_assembly => _assembly.GetTypes())
			.Where(_type => _type.Name == _typeName);
		return matchingTypes.ToArray();
	}

	/// <summary>
	/// Find all types derived from a given one.
	/// </summary>
	/// <returns>All the types derived from _targetType.</returns>
	/// <param name="_targetType">Type whose derived we want.</param>
	public static List<Type> FindAllDerivedTypes(Type _targetType) {
		Assembly targetAssembly = Assembly.GetAssembly(_targetType);
		Type[] allTypesInAssembly = targetAssembly.GetTypes();
		List<Type> derivedTypes = allTypesInAssembly.Where(
			(Type _t) => {
				return _t != _targetType && _targetType.IsAssignableFrom(_t);
			}).ToList();
		return derivedTypes;
	}

	/// <summary>
	/// 
	/// </summary>
	public static void SetPrivateVar(object obj, string varName, object value) {
		FieldInfo field = GetTypeField(obj.GetType(), varName, true);
		if(field != null)
			field.SetValue(obj, value);
	}

	/// <summary>
	/// 
	/// </summary>
	public static T GetPrivateVar<T>(object obj, string varName) {
		FieldInfo field = GetTypeField(obj.GetType(), varName, true);
		return field != null ? (T)field.GetValue(obj) : default(T);
	}

	/// <summary>
	/// 
	/// </summary>
	public static FieldInfo GetTypeField(Type t, string fieldName, bool recurse = true) {
		FieldInfo field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if(field == null && recurse && t != typeof(System.Object))
			return GetTypeField(t.BaseType, fieldName, recurse);

		if(field == null)
			throw new Exception("Could not find field \"" + fieldName + "\" in type \"" + t + "\"");

		return field;
	}

	/// <summary>
	/// TODO!! Allow params
	/// </summary>
	public static object CallMethod(object obj, string methodName) {
		MethodInfo method = GetTypeMethod(obj.GetType(), methodName, true);
		return method != null ? method.Invoke(obj, null) : null;
	}

	/// <summary>
	/// 
	/// </summary>
	public static MethodInfo GetTypeMethod(Type t, string methodName, bool recurse = true) {
		MethodInfo method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if(method == null && recurse && t != typeof(System.Object))
			return GetTypeMethod(t.BaseType, methodName, recurse);

		if(method == null)
			throw new Exception("Could not find method \"" + methodName + "\" in type \"" + t + "\"");

		return method;
	}

	/// <summary>
	/// 
	/// </summary>
	public static System.Type[] GetInheritedOfType(System.Type _t) {
		System.Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

		List<Type> subclasses = new List<Type>();

		for(int i = 0; i < types.Length; i++) {
			if(types[i].IsSubclassOf(_t)) {
				subclasses.Add(types[i]);
			}
		}

		return subclasses.ToArray();
	}

	/// <summary>
	/// Get all the public and non public fields of a given type.
	/// </summary>
	/// <returns>The fields of the given type.</returns>
	/// <param name="_type">The type to be checked.</param>
	public static FieldInfo[] GetFields(System.Type _type) {
		// From HSX
		return _type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
	}

	/// <summary>
	/// Get the value of a field within an object.
	/// Black magic from HSX.
	/// </summary>
	/// <returns>????.</returns>
	/// <param name="_fieldType">????.</param>
	/// <param name="_obj">????.</param>
	static object GetValue(System.Type _fieldType, object _obj) {
		object value = null;

		// Generics
		if(_fieldType.IsGenericType) {
			//List<>
			if(_fieldType.GetGenericTypeDefinition() == typeof(List<>)) {
				if(_obj is IList) {
					// Create list
					IList newList = System.Activator.CreateInstance(_fieldType) as IList;

					// If we are a list of Classes, 2d array/list, not primitive types
					System.Type listItemType = _fieldType.GetGenericArguments()[0];
					bool isClassType = listItemType.IsClass && listItemType != typeof(string);
					bool isGeneric = listItemType.IsGenericType;
					bool isArray = listItemType.IsArray;

					// Add from list
					foreach(object listValue in (_obj as IList)) {
						object v = null;
						if(isClassType || isGeneric || isArray) {
							v = GetValue(listItemType, listValue);
						}
						else {
							v = ChangeType(listValue, listItemType);
						}

						newList.Add(v);
					}

					value = newList;
				}
				else {
					Debug.LogError("Incorrect data format for a List<>. {0} but should be IList" + _obj.GetType().Name);
				}

			}
			else {
				Debug.LogError("No support to read in the type " + _fieldType.Name);
			}
			// No support for any other generics yet
		}

		// Array
		else if(_fieldType.IsArray) {
			if(_obj is IList) {
				// Create array
				System.Array array = System.Array.CreateInstance(_fieldType.GetElementType(), (_obj as IList).Count);

				// If we are a list of Classes, 2d array/list, not primitive types
				System.Type arrayItemType = _fieldType.GetElementType();
				bool isClassType = arrayItemType.IsClass && arrayItemType != typeof(string);
				bool isGeneric = arrayItemType.IsGenericType;
				bool isArray = arrayItemType.IsArray;

				// Add from list
				int i = 0;
				foreach(object listValue in (_obj as IList)) {
					object v = null;
					if(isClassType || isGeneric || isArray) {
						v = GetValue(arrayItemType, listValue);
					}
					else {
						v = ChangeType(listValue, arrayItemType);
					}

					array.SetValue(v, i++);
				}

				value = array;
			}
			else {
				Debug.LogError("Incorrect data format for an array. {0} but should be IList" + _obj.GetType().Name);
			}
		}

		// Object field
		else if(_fieldType.IsClass && _fieldType != typeof(string)) {
			Dictionary<string, object> objectData = _obj as Dictionary<string, object>;
			if(objectData != null) {
				value = System.Activator.CreateInstance(_fieldType);
				ApplyData(value, objectData, "", "");
			}
			else {
				Debug.LogError("Incorrect data format for a class. {0} but should be Dictionary<string, object>" + _obj.GetType().Name);
			}
		}

		// Individual field
		else {
			try {
				System.Type valueType = _obj.GetType();
				if(_fieldType.IsEnum && (valueType == typeof(int) || valueType == typeof(long))) {
					value = System.Enum.ToObject(_fieldType, _obj);
				}
				else {
					value = ChangeType(_obj, _fieldType);
					if(_fieldType.IsEnum) {
						if(value == null) {
							Debug.LogError(string.Format("Unable to convert {0} to type {1}", _obj, _fieldType));
						}
					}
				}
			} catch(System.Exception e) {
				Debug.LogError(e.Message);
				Debug.LogError(string.Format("Unable to convert {0} to type {1}", _obj.GetType(), _fieldType));
			}
		}
		return value;
	}

	/// <summary>
	/// Changes the type of the given object (similar to casting).
	/// </summary>
	/// <returns>The object with the new type.</returns>
	/// <param name="_value">The object to be changed.</param>
	/// <param name="_type">The target type.</param>
	static object ChangeType(object _value, System.Type _type) {
		// From HSX
		if(_type.IsEnum) {
			return System.Convert.ChangeType(System.Enum.Parse(_type, _value as string), _type, System.Globalization.CultureInfo.InvariantCulture);
		} else {
			return System.Convert.ChangeType(_value, _type, System.Globalization.CultureInfo.InvariantCulture);
		}
	}

	/// <summary>
	/// Serialize the data of the given object into a dictionary.
	/// </summary>
	/// <returns>The serialized data, in the format <fieldName, value>.</returns>
	/// <param name="_obj">The object to be serialized.</param>
	public static Dictionary<string, object> GetData(object _obj) {
		// TODO: We may want to flag non serialisable (or use the Serialise tag) for variables we don't want saved out.
		Dictionary<string, object> data = new Dictionary<string, object>();

		FieldInfo[] fieldList = GetFields(_obj.GetType());
		for(int i = 0; i < fieldList.Length; i++) {
			FieldInfo field = fieldList[i];
			string fieldName = field.Name;
			if(fieldName.StartsWith("m_")) {
				fieldName = fieldName.Substring(2);
			}

			object value = field.GetValue(_obj);
			if (value != null) {
				System.Type valueType = value.GetType();

				if(value is IEnumerable && valueType != typeof(string)) {
					System.Type listItemType;
					if(valueType.IsArray) {
						listItemType = valueType.GetElementType();
					}
					else {
						listItemType = valueType.GetGenericArguments()[0];
					}

					bool isClassType = listItemType.IsClass && listItemType != typeof(string);
					bool isGeneric = listItemType.IsGenericType;
					bool isArray = listItemType.IsArray;

					List<object> list = new List<object>();
					IEnumerator enumerator = (value as IEnumerable).GetEnumerator();
					while(enumerator.MoveNext()) {
						object listItem = enumerator.Current;
						if(isClassType || isGeneric || isArray) {
							listItem = GetData(listItem);
						}

						list.Add(listItem);
					}
					value = list;
				}
				else if(valueType.IsClass && valueType != typeof(string)) {
					value = GetData(value);
				}
				else if(valueType.IsEnum) {
					// Lets save Enums as integers. May want an option to save as strings?
					value = (int)value;
				}

				data[fieldName] = value;
			}
		}

		return data;
	}

	/// <summary>
	/// Load the data serialized in the given dictionary into the given object.
	/// </summary>
	/// <param name="_target">The object where the data is to be loaded.</param>
	/// <param name="data">The data to be loaded.</param>
	/// <param name="baseName">????</param>
	/// <param name="baseNameAlt">????</param>
	public static void ApplyData(object _target, Dictionary<string, object> _data, string _baseName, string _baseNameAlt) {
		FieldInfo[] fieldList = GetFields(_target.GetType());
		for(int i = 0; i < fieldList.Length; i++) {
			FieldInfo field = fieldList[i];

			object dataValue = null;
			if(_data.TryGetValue(_baseName + field.Name, out dataValue) || (field.Name.StartsWith("m_") && _data.TryGetValue(_baseName + field.Name.Substring(2), out dataValue))
			   || _data.TryGetValue(_baseNameAlt + field.Name, out dataValue) || (field.Name.StartsWith("m_") && _data.TryGetValue(_baseNameAlt + field.Name.Substring(2), out dataValue))) {
				object fieldValue = GetValue(field.FieldType, dataValue);
				if(fieldValue != null) {
					field.SetValue(_target, fieldValue);
				}
			}
		}
	}
}