using System;
using System.Reflection;
using System.Collections.Generic;

public static class TypeUtil {
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
	/// Changes the type of the given object (similar to casting).
	/// </summary>
	/// <returns>The object with the new type.</returns>
	/// <param name="_value">The object to be changed.</param>
	/// <param name="_type">The target type.</param>
	static object ChangeType(object _value, System.Type _type) {
		// From HSX
		if(_type.IsEnum) {
			return System.Convert.ChangeType(System.Enum.Parse(_type, _value as string), _type);
		} else {
			return System.Convert.ChangeType(_value, _type);
		}
	}
}