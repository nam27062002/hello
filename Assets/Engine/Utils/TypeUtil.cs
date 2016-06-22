using System;
using System.Reflection;
using System.Collections.Generic;

public static class TypeUtil
{
	/**
	 * Variables
	 */

	public static void SetPrivateVar(object obj, string varName, object value)
	{
		FieldInfo field = GetTypeField(obj.GetType(), varName, true);
		if(field != null)
			field.SetValue(obj, value);
	}

	public static T GetPrivateVar<T>(object obj, string varName)
	{
		FieldInfo field = GetTypeField(obj.GetType(), varName, true);
		return field != null? (T)field.GetValue(obj) : default(T);
	}

	public static FieldInfo GetTypeField(Type t, string fieldName, bool recurse = true)
	{
		FieldInfo field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if(field == null && recurse && t != typeof(System.Object))
			return GetTypeField(t.BaseType, fieldName, recurse);
		
		if(field == null)
			throw new Exception("Could not find field \""+fieldName+"\" in type \""+t+"\"");
		
		return field;
	}

	/**
	 * Methods TODO: allow params
	 */
	public static object CallMethod(object obj, string methodName)
	{
		MethodInfo method = GetTypeMethod(obj.GetType(), methodName, true);
		return method != null? method.Invoke(obj, null) : null;
	}

	public static MethodInfo GetTypeMethod(Type t, string methodName, bool recurse = true)
	{
		MethodInfo method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		if(method == null && recurse && t != typeof(System.Object))
			return GetTypeMethod(t.BaseType, methodName, recurse);

		if(method == null)
			throw new Exception("Could not find method \""+methodName+"\" in type \""+t+"\"");

		return method;
	}

	/**
	 * 
	 */
	public static System.Type[] GetInheritedOfType(System.Type _t) {
		System.Type[] types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();

		List<Type> subclasses = new List<Type>();

		for (int i = 0; i < types.Length; i++) {
			if (types[i].IsSubclassOf(_t)) {
				subclasses.Add(types[i]);
			}
		}

		return subclasses.ToArray();
	}
}