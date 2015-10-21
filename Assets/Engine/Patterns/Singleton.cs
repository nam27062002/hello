// Singleton.cs
// 
// Created by Alger Ortín Castellví on 20/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Shared constants and utils between different singleton implementations.
/// </summary>
public static class Singleton {
	//----------------------------------------------------------------------//
	// CONSTANTS															//
	//----------------------------------------------------------------------//
	public static readonly string RESOURCES_FOLDER = "Singletons/";
	public static readonly string PARENT_OBJECT_NAME = "Singletons";

	/// <summary>
	/// State of a singleton class.
	/// </summary>
	public enum EState {
		// Instance hasn't been created yet
		INIT,
		
		// If the instance getter is called while the instance is being created (i.e. from T's Awake() function)
		// we will enter into an infinite loop of instances creation.
		// This variable will be used to prevent this.
		CREATING_INSTANCE,
		
		// Everything ok
		READY,
		
		// When Unity quits, it destroys objects in a random order.
		// In principle, a SingletonScriptableObject is only destroyed when application quits.
		// If any script calls instance after it has been destroyed, it will create a buggy ghost object that will stay on the Editor scene even after stopping playing the Application. Really bad!
		// So, this was made to be sure we're not creating that buggy ghost object.
		DESTROYING_INSTANCE,
		
		// Skip the previous statement if we want manually destroying the SingletonScriptableObject
		APPLICATION_QUITTING
	};
}