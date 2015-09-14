// SerializableClass.cs
// 
// Created by Alger Ortín Castellví on 10/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Dummy implementation of the ISerializationCallbackReceiver interface to avoid
/// having to implement all methods when implementing the interface.
/// Use it to initialize your serialized classes by overriding the OnAfterSerialize method,
/// otherwie Unity inspector will overwrite the class' default values with its own default values.
/// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.html
/// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html
/// </summary>
[Serializable]
public class SerializableClass : ISerializationCallbackReceiver {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private bool m_firstDeserializationDone = false;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html
	/// </summary>
	public virtual void OnBeforeSerialize() {
		// To be overriden by heirs if needed
	}

	/// <summary>
	/// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnAfterDeserialize.html
	/// </summary>
	public void OnAfterDeserialize() {
		// If it's the first time, call the virtual method
		if(!m_firstDeserializationDone) {
			m_firstDeserializationDone = true;
			OnFirstDeserialization();
		}
	}

	/// <summary>
	/// To be implemented by heirs needing to initialize stuff after the first deserialization.
	/// Will be only called once.
	/// </summary>
	protected virtual void OnFirstDeserialization() {
		// To be overriden by heirs if needed
	}
}