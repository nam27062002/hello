// HDPassiveEventDefinition.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDPassiveEventDefinition : HDLiveEventDefinition {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private Modifier m_mainMod = null;
	public Modifier mainMod {
		get { return m_mainMod; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDPassiveEventDefinition() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~HDPassiveEventDefinition() {
		m_mainMod = null;
	}

	override public SimpleJSON.JSONClass ToJson() {
		if (m_otherMods.Count == 0 && m_dragonMods.Count == 0) {
			return null;
		}
		return base.ToJson();
	}

	/// <summary>
	/// Initialize the definition with JSON data.
	/// </summary>
	/// <param name="_data">Data.</param>
	override public void ParseInfo(SimpleJSON.JSONNode _data) {
		// Let parent do its job
		base.ParseInfo(_data);

		// Initialize internal vars
		// Store the first valid mod as main
		if(m_otherMods.Count > 0) {
			m_mainMod = m_otherMods[0];
		} else if(m_dragonMods.Count > 0) {
			m_mainMod = m_dragonMods[0];
		} else {
			Debug.LogError(Color.red.Tag("ERROR! Passive event has no modifiers defined!"));
		}
	}

	/// <summary>
	/// Clean this instance. Remove all information so this definition is not valid.
	/// </summary>
	override public void Clean() {
		m_mainMod = null;
		base.Clean();
	}

    /// <summary>
    /// Initialize this passiveevent definition from the data in the content
    /// We use this method to load a quest from the localPassiveEvent
    /// </summary>
    /// <param name="_def">The welcomeBack definition node</param>
    public void InitFromDefinition(DefinitionNode _def)
    {
        // Find the modifier defined in the welcome back content
        string modSku = _def.GetAsString("passiveModSku");
        DefinitionNode modDef =
            DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.LIVE_EVENTS_MODIFIERS, 
                "sku", modSku);

        // Make sure the mod sku is valid
        if (modDef == null)
        {
            Debug.LogError("The Passive Event modifier in the Welcome Back definition doesnt exist.");
            return;
        }
        
        // Create the modifier
        m_mainMod = Modifier.CreateFromDefinition(modDef);

        // Set the initialized flag to true (otherwise wont show in the UI)
        m_initialized = true;


    }
}