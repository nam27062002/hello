// Singleton.cs
// 
// Created by Alger Ortín Castellví on 19/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Generic singleton interface to simplify singleton implementations.
/// All singletons implementing this class will be automatically created the first time the instance is accessed.
/// They will be also added to a single object in the current scene's hierarchy which will persist throughout scene changes.
/// The singleton instance has protected access, so only the implementing class can access it.
/// All operations must be performed via public static funcions on the implementing class or by implementing a wrapper exposing the protected instance.
/// Example:
/// <code>
/// public class MySingleton1 : Singleton<MySingleton1> {
///     private int num;
///	    public static void AddNum() {
///	        instance.num++;
///	        Debug.Log("Singleton1: " + instance.num);
///     }
/// }
/// </code>
/// <see cref="http://wiki.unity3d.com/index.php/Singleton"/>
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string CONTAINER_NAME = "Singletons";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Multithread lock - just in case instance getter is requested from two different threads at the same time
	private static object m_threadLock = new object();

	// When Unity quits, it destroys objects in a random order.
	// In principle, a Singleton is only destroyed when application quits.
	// If any script calls instance after it has been destroyed, it will create a buggy ghost object that will stay on the Editor scene even after stopping playing the Application. Really bad!
	// So, this was made to be sure we're not creating that buggy ghost object.
	protected static bool m_applicationIsQuitting = false;

	//------------------------------------------------------------------//
	// SINGLETON INSTANCE												//
	//------------------------------------------------------------------//
	// Whenever the instance is requested for the first time, it will be created and instantiated into the singletons container object.
	// If the container hasn't been created yet, it will be created and added to the current scene, activating its DontDestroyOnLoad flag.
	// Give it protected access so only the class implementing the singleton can touch it.
	// All operations must be performed by static funcions on the implementing class or by implementing a wrapper exposing the protected instance.
	private static T m_instance = null;
	protected static T instance {
		get {
			// Avoid re-creating the instance while the application is quitting
			if(m_applicationIsQuitting) {
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
				return null;
			}

			// Make sure this only one thread is doing this!
			lock(m_threadLock) {
				// Is the static instance created?
				if(m_instance == null) {
					// If the singleton container has not been created, do it now
					GameObject containerObj = GameObject.Find(CONTAINER_NAME);
					if(containerObj == null) {
						containerObj = new GameObject(CONTAINER_NAME);
						GameObject.DontDestroyOnLoad(containerObj);	// Persist throughout scene changes
					}

					// Create the instance by adding it as a component of the game object we just created
					// Store its reference so this is only done once
					m_instance = containerObj.AddComponent<T>();
				}

				// Instance is created! Return it
				return m_instance;
			}
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Nothing to do
	}

	/// <summary>
	/// First update.
	/// </summary>
	protected virtual void Start() {
		// Nothing to do
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	protected virtual void Update() {
		// Nothing to do
	}
	
	/// <summary>
	/// Destructor
	/// </summary>
	protected virtual void OnDestroy() {
		// Make sure instance reference is cleaned up
		// [AOC] I think it's useless, but they do it: https://youtu.be/64uOVmQ5R1k?t=20m16s
		if(m_instance != null) m_instance = null;

		// Avoid re-creating the instance while the application is quitting
		m_applicationIsQuitting = true;
	}
}

