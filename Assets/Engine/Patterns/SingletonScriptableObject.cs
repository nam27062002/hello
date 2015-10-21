// SingletonScriptableObject.cs
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
/// Algernative version of the SingletonMonoBehaviour allowing us to have Singletons not linked to game objects.
/// @see SingletonMonobehaviour
/// Unfortunately, we haven't found a better way to implement it than to replicate the whole code (no multiple inheritance in C# :-/).
/// </summary>
public class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Logic state
	private static Singleton.EState m_state = Singleton.EState.INIT;
	
	// Multithread lock - just in case instance getter is requested from two different threads at the same time
	private static object m_threadLock = new object();

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Only for very particular uses
	public static bool isInstanceCreated { get { return m_instance != null; }}
	
	//------------------------------------------------------------------//
	// SINGLETON INSTANCE												//
	//------------------------------------------------------------------//
	// Whenever the instance is requested for the first time, it will be created and instantiated into the SingletonScriptableObjects container object.
	// If the container hasn't been created yet, it will be created and added to the current scene, activating its DontDestroyOnLoad flag.
	// Give it protected access so only the class implementing the SingletonScriptableObject can touch it.
	// All operations must be performed by static funcions on the implementing class or by implementing a wrapper exposing the protected instance.
	private static T m_instance = null;
	public static T instance {
		get {
			// Avoid re-creating the instance while the application is quitting
			if(m_state == Singleton.EState.APPLICATION_QUITTING) {
				Debug.LogWarning("[SingletonScriptableObject] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
				return null;
			}
			
			// Make sure that only one thread is doing this!
			lock(m_threadLock) {
				// Is the static instance created?
				if(m_instance == null) {
					// If instance creation is locked, throw a warning
					if(m_state == Singleton.EState.CREATING_INSTANCE) {
						Debug.LogWarning("[SingletonScriptableObject] Instance for " + typeof(T) + " is currently being created. Avoid calling the instance getter during the Awake function of your SingletonScriptableObject class.");
						return null;
					}
					
					// Lock instance creation
					m_state = Singleton.EState.CREATING_INSTANCE;

					// Check if there is a stored SO at the resources directory
					m_instance = Resources.Load<T>(Singleton.RESOURCES_FOLDER + typeof(T).Name);
					if(m_instance == null) {
						// There is no stored object for this class, create a new SingletonScriptableObject instance
						m_instance = ScriptableObject.CreateInstance<T>();
					}

					// Make it persistent
					ScriptableObject.DontDestroyOnLoad(m_instance);
					
					// Instance has been created and stored, unlock instance creation
					m_state = Singleton.EState.READY;
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
	/// Destructor
	/// </summary>
	protected virtual void OnDestroy() {
		// Make sure instance reference is cleaned up
		// [AOC] I think it's useless, but they do it: https://youtu.be/64uOVmQ5R1k?t=20m16s
		// [AOC] In any case make sure to do it only if we're the SingletonScriptableObject instance!
		if(m_instance != null && m_instance == this) m_instance = null;
		
		// Avoid re-creating the instance while the application is quitting
		// Unless manually destroying the instance
		if(m_state != Singleton.EState.DESTROYING_INSTANCE) {
			m_state = Singleton.EState.APPLICATION_QUITTING;
		}
	}
	
	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Delete the SingletonScriptableObject instance of this type and the object containing it in the scene.
	/// </summary>
	public static void DestroyInstance() {
		// Skip if already quitting application or instance hasn't been created
		if(m_state == Singleton.EState.APPLICATION_QUITTING || m_instance == null) return;

		// Remember that we're manually destroying the SingletonScriptableObject so recreation of the instance is not banned
		m_state = Singleton.EState.DESTROYING_INSTANCE;
		
		// Immediately destroy game object holding the SingletonScriptableObject
		ScriptableObject.DestroyImmediate(m_instance);
	}
}

