// SerializationExt.cs
// 
// Created by Alger Ortín Castellví on 20/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to serialization system classes.
/// </summary>
public static class SerializationExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create a new object copying all the current values of the given object.
	/// From http://stackoverflow.com/questions/78536/deep-cloning-objects.
	/// </summary>
	/// <returns>An exact copy of the source object.</returns>
	/// <typeparam name="T">The type of object being copied.</typeparam>
	/// <param name="_obj">The object we're cloning.</param>
	public static T Clone<T>(this T _obj) {
		// Object must be serializable
		if(!typeof(T).IsSerializable) {
			throw new ArgumentException("The type must be serializable.", "_obj");
		}
		
		// Don't serialize a null object, simply return the default for that object
		if(System.Object.ReferenceEquals(_obj, null)) {
			return default(T);
		}

		// Since some system types are not serializabe, we must add custom serialization surrogates
		// @see http://forum.unity3d.com/threads/vector3-not-serializable.7766/
		SurrogateSelector ss = new SurrogateSelector();
		ss.AddSurrogate(typeof(AnimationCurve), new StreamingContext(StreamingContextStates.All), new AnimationCurveSS());

		// Perform the copy via serialization
		IFormatter formatter = new BinaryFormatter();
		formatter.SurrogateSelector = ss;
		Stream stream = new MemoryStream();
		using(stream) {
			formatter.Serialize(stream, _obj);
			stream.Seek(0, SeekOrigin.Begin);
			return (T)formatter.Deserialize(stream);
		}
	}
}

/// <summary>
/// Auxiliar class to serialize AnimationCurves.
/// </summary>
sealed class AnimationCurveSS : ISerializationSurrogate {
	/// <summary>
	/// Method called to serialize an AnimationCurve object.
	/// </summary>
	/// <param name="_obj">The object to be serialized.</param>
	/// <param name="_info">The object where to put the serialization data.</param>
	/// <param name="_context">_context.</param>
	public void GetObjectData(System.Object _obj, SerializationInfo _info, StreamingContext _context) {
		AnimationCurve curve = (AnimationCurve)_obj;
		_info.AddValue("preWrapMode", curve.preWrapMode);
		_info.AddValue("postWrapMode", curve.postWrapMode);
		int numKeys = curve.keys.Length;
		_info.AddValue("numKeys", numKeys);
		for(int i = 0; i < numKeys; i++) {
			_info.AddValue("key" + i + "time", curve.keys[i].time);
			_info.AddValue("key" + i + "value", curve.keys[i].value);
			_info.AddValue("key" + i + "inTangent", curve.keys[i].inTangent);
			_info.AddValue("key" + i + "outTangent", curve.keys[i].outTangent);
			_info.AddValue("key" + i + "tangentMode", curve.keys[i].tangentMode);
		}
	}
	
	/// <summary>
	/// Method called to deserialized an AnimationCurve object.
	/// </summary>
	/// <returns>The deserialized object.</returns>
	/// <param name="_obj">_obj.</param>
	/// <param name="_info">The object containing the serialized data.</param>
	/// <param name="_context">_context.</param>
	/// <param name="_selector">_selector.</param>
	public System.Object SetObjectData(System.Object _obj, SerializationInfo _info, StreamingContext _context, ISurrogateSelector _selector) {
		//AnimationCurve curve = (AnimationCurve)_obj;
		AnimationCurve curve = new AnimationCurve();
		int numKeys = (int)_info.GetValue("numKeys", typeof(int));
		Keyframe[] keys = new Keyframe[numKeys];
		for(int i = 0; i < numKeys; i++) {
			keys[i].time = (float)_info.GetValue("key" + i + "time", typeof(float));
			keys[i].value = (float)_info.GetValue("key" + i + "value", typeof(float));
			keys[i].inTangent = (float)_info.GetValue("key" + i + "inTangent", typeof(float));
			keys[i].outTangent = (float)_info.GetValue("key" + i + "outTangent", typeof(float));
			keys[i].tangentMode = (int)_info.GetValue("key" + i + "tangentMode", typeof(int));
		}
		curve.keys = keys;
		curve.preWrapMode = (WrapMode)_info.GetValue("preWrapMode", typeof(WrapMode));
		curve.postWrapMode = (WrapMode)_info.GetValue("postWrapMode", typeof(WrapMode));
		_obj = curve;
		return _obj;   // Formatters ignore this return value // Seems to have been fixed!
	}
}
