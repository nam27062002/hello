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
public class Singleton<T> where T : Singleton<T>, new() {
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
    public static bool isInstanceCreated { get { return m_instance != null; } }

    //------------------------------------------------------------------//
    // SINGLETON INSTANCE												//
    //------------------------------------------------------------------//
    // Whenever the instance is requested for the first time, it will be created and instantiated into the SingletonScriptableObjects container object.
    // If the container hasn't been created yet, it will be created and added to the current scene, activating its DontDestroyOnLoad flag.
    // Give it protected access so only the class implementing the SingletonScriptableObject can touch it.
    // All operations must be performed by static funcions on the implementing class or by implementing a wrapper exposing the protected instance.
    protected static T m_instance = null;
    public static T instance {
        get {
            // CreateInstance() method does all the hard work
            CreateInstance();
            return m_instance;
        }
    }

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    protected virtual void OnApplicationQuit() {
        // Make sure instance reference is cleaned up
        // [AOC] I think it's useless, but they do it: https://youtu.be/64uOVmQ5R1k?t=20m16s
        // [AOC] In any case make sure to do it only if we're the singleton instance!
        if (m_instance != null && m_instance == this) m_instance = null;

        // Avoid re-creating the instance while the application is quitting
        // Unless manually destroying the instance
        m_state = ISingleton.EState.APPLICATION_QUITTING;

        Messenger.RemoveListener(MessengerEvents.APPLICATION_QUIT, OnApplicationQuit);
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
        if (m_state == ISingleton.EState.APPLICATION_QUITTING) {
            Debug.LogWarning("[SingletonScriptableObject] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
            return;
        }

        // Make sure that only one thread is doing this!
        lock (m_threadLock) {
            // If forced, destroy existing instance
            if (_force && m_instance != null) {
                DestroyInstance();
            }

            // Is the static instance created?
            if (m_instance == null) {

                // If instance creation is locked, throw a warning
                if (m_state == ISingleton.EState.CREATING_INSTANCE) {
                    Debug.LogWarning("[SingletonScriptableObject] Instance for " + typeof(T) + " is currently being created. Avoid calling the instance getter during the Awake function of your SingletonScriptableObject class.");
                    return;
                }

                // Lock instance creation
                m_state = ISingleton.EState.CREATING_INSTANCE;

                // Check if there is a stored SO at the resources directory
                m_instance = new T();
                // m_instance = Resources.Load<T>(ISingleton.RESOURCES_FOLDER + typeof(T).Name);

                // Make it persistent
                // [AOC] Not needed since scriptable objects already persist through scenes (and doing this throws a warning in runtime)
                //		 See http://answers.unity3d.com/questions/1115856/scriptableobject-vs-dontdestroyonload.html
                //ScriptableObject.DontDestroyOnLoad(m_instance);

                Messenger.AddListener(MessengerEvents.APPLICATION_QUIT, m_instance.OnApplicationQuit);

                // Instance has been created and stored, unlock instance creation
                m_state = ISingleton.EState.READY;
                m_instance.OnCreateInstance();
            }
        }
    }

    /// <summary>
    /// Delete the SingletonScriptableObject instance of this type and the object containing it in the scene.
    /// </summary>
    public static void DestroyInstance() {
        // Skip if already quitting application or instance hasn't been created
        if (m_state == ISingleton.EState.APPLICATION_QUITTING || m_instance == null) return;

        // Remember that we're manually destroying the SingletonScriptableObject so recreation of the instance is not banned
        m_state = ISingleton.EState.DESTROYING_INSTANCE;
        m_instance.OnDestroyInstance();

        Messenger.RemoveListener(MessengerEvents.APPLICATION_QUIT, m_instance.OnApplicationQuit);

        // Immediately destroy game object holding the SingletonScriptableObject
        m_instance = null;


    }

    protected virtual void OnCreateInstance() { }
    protected virtual void OnDestroyInstance() { }
}