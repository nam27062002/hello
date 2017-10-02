﻿// Singleton.cs
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
/// A new GameObject will be created for each one of them (called as the class) and
/// added as child of a single GameObject in the current scene's hierarchy which 
/// will persist throughout scene changes.
/// Alternatively, if a prefab named after the class with the "PF_" (i.e. PF_MySingleton) prefix exists
/// in a "Singletons" folder within the Resources folder, it will be used instead of creating a new GameObject
/// for that class. This allows users to initialize singletons from Unity's inspector by editing the prefab values.
/// The singleton instance has protected access, so only the implementing class can access it.
/// All operations must be performed via public static funcions on the implementing class or by implementing a wrapper exposing the protected instance.
/// Example:
/// <code>
/// public class MySingleton : Singleton<MySingleton> {
///     private int num;
///	    public static void AddNum() {
///	        instance.num++;
///	        Debug.Log("MySingleton: " + instance.num);
///     }
/// }
/// </code>
/// <see cref="http://wiki.unity3d.com/index.php/Singleton"/>
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
namespace UbiBCN
{
	public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T> {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Logic state
		private static ISingleton.EState m_state = ISingleton.EState.INIT;
		
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
		// Whenever the instance is requested for the first time, it will be created and instantiated into the singletons container object.
		// If the container hasn't been created yet, it will be created and added to the current scene, activating its DontDestroyOnLoad flag.
		// Give it protected access so only the class implementing the singleton can touch it.
		// All operations must be performed by static funcions on the implementing class or by implementing a wrapper exposing the protected instance.
		private static T m_instance = null;
		public static T instance {
			get {
				// CreateInstance method does the hard work
				CreateInstance();
				return m_instance;
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
			// [AOC] In any case make sure to do it only if we're the singleton instance!
			if(m_instance != null && m_instance == this) m_instance = null;
			
			// Avoid re-creating the instance while the application is quitting
			// Unless manually destroying the instance
			if(m_state != ISingleton.EState.DESTROYING_INSTANCE) {
				m_state = ISingleton.EState.APPLICATION_QUITTING;
			}
		}

		protected virtual void OnApplicationQuit() {			
			// Avoid re-creating the instance while the application is quitting
			// Unless manually destroying the instance
			m_state = ISingleton.EState.APPLICATION_QUITTING;            
		}       
		
		//------------------------------------------------------------------//
		// PUBLIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Create the singleton instance if not created.
		/// </summary>
		/// <param name="_force">If <c>true</c>, re-create instance.</param>
		public static void CreateInstance(bool _force = false) {
			// Avoid re-creating the instance while the application is quitting
			if(m_state == ISingleton.EState.APPLICATION_QUITTING) {
				Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
				return;
			}
			
			// Make sure that only one thread is doing this!
			lock(m_threadLock) {
				// If forced, destroy existing instance
				if(_force && m_instance != null) {
					DestroyInstance();
				}

				// Is the static instance created?
				if(m_instance == null) {
					// If instance creation is locked, throw a warning
					if(m_state == ISingleton.EState.CREATING_INSTANCE) {
						Debug.LogWarning("[Singleton] Instance for " + typeof(T) + " is currently being created. Avoid calling the instance getter during the Awake function of your singleton class.");
						return;
					}
					
					// Lock instance creation
					m_state = ISingleton.EState.CREATING_INSTANCE;
					
					// If the singletons container has not been created, do it now
					GameObject containerObj = GameObject.Find(ISingleton.PARENT_OBJECT_NAME);
					if(containerObj == null) {
						containerObj = new GameObject(ISingleton.PARENT_OBJECT_NAME);
						if (Application.isPlaying) {
							GameObject.DontDestroyOnLoad(containerObj);	// Persist throughout scene changes
						}
					}
					
					// Is there a pre-made prefab for this class in the Resources folder?
					GameObject singletonObj = null;
					GameObject prefabObj = Resources.Load<GameObject>(ISingleton.PARENT_OBJECT_NAME + "/PF_" + typeof(T).Name);
					if(prefabObj != null) {
						// Make sure the prefab contains a component of the required type
						if(prefabObj.GetComponent<T>() == null) {
							// Component wasn't found, throw a warning
							Debug.LogWarning("[Singleton] Prefab for singleton " + typeof(T) + " doesn't contain a component of type " + typeof(T) + ".\nCreating singleton with default values instead.");
							
							// Destroy the object we just loaded
							GameObject.DestroyImmediate(prefabObj);
							prefabObj = null;
						}
						
						// Everything's ok, use it as singleton object and get the instance from it
						else {
							// Instantiate the loaded prefab
							singletonObj = GameObject.Instantiate(prefabObj);
							singletonObj.name = prefabObj.name;		// Get rid of the "(Clone)" that Unity adds by default
							
							// Get the singleton's instance from it
							m_instance = singletonObj.GetComponent<T>();

                            // Prevents this game object which has been created by scripts to be saved in the scene if a instance stayed in the scene after playing by mistake
                            m_instance.hideFlags = HideFlags.DontSaveInEditor;
                        }
					}
					
					// If there wasn't a valid prefab, create a new object to hold the instance
					if(singletonObj == null) {
						// Create the object and give it the name of the class
						singletonObj = new GameObject(typeof(T).Name);
						
						// Create the instance by adding it as a component of the game object we just created
						// Store its reference so this is only done once
						m_instance = singletonObj.AddComponent<T>();

                        // Prevents this game object which has been created by scripts to be saved in the scene if a instance stayed in the scene after playing by mistake
                        m_instance.hideFlags = HideFlags.DontSaveInEditor;
                    }                    

                    // Attach the singleton object as child of the Singletons container to make it have the DontDestroyOnLoad flag and to keep the hierarchy clean
                    singletonObj.transform.SetParent(containerObj.transform, false);
					
					// Instance has been created and stored, unlock instance creation
					m_state = ISingleton.EState.READY;
				}
			}
		}

		/// <summary>
		/// Delete the singleton instance of this type and the object containing it in the scene.
		/// </summary>
		public static void DestroyInstance() {
			// Skip if already quitting application or instance hasn't been created
			if(m_state == ISingleton.EState.APPLICATION_QUITTING || m_instance == null) return;

			// Remember that we're manually destroying the singleton so recreation of the instance is not banned
			m_state = ISingleton.EState.DESTROYING_INSTANCE;
			
			// Immediately destroy game object holding the singleton
			DestroyImmediate(m_instance.gameObject);
		}
	}
}
