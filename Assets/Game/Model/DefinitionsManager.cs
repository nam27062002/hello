// DefinitionsManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/12/2015.
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
/// Singleton scriptable object referencing all the definition sets in the game.
/// </summary>
//[CreateAssetMenu]
public class DefinitionsManager : SingletonScriptableObject<DefinitionsManager> {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Add here as much definition sets as needed
	// References must be initialized from inspector
	[Separator("Entities")]
	[SerializeField] private EntityDefinitions m_entities = null;
	public static EntityDefinitions entities { get { return instance.m_entities; }}

	[SerializeField] private EntityCategoryDefinitions m_entityCategories = null;
	public static EntityCategoryDefinitions entityCategories { get { return instance.m_entityCategories; }}

	[Separator("Meta-game")]
	[SerializeField] private LevelDefinitions m_levels = null;
	public static LevelDefinitions levels { get { return instance.m_levels; }}

	[SerializeField] private MissionDefinitions m_missions = null;
	public static MissionDefinitions missions { get { return instance.m_missions; }}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets a definition of a given type.
	/// The definition will be requested from the right set based on its type.
	/// This method is a bit less efficient than directly request the definition 
	/// to the target set (remember that sets are exposed), so use it consciously.
	/// </summary>
	/// <returns>The requested definition. <c>null</c> if definition couldn't be found or type doesn't match any of the definition sets.</returns>
	/// <param name="_sku">Id of the definition to be get.</param>
	/// <typeparam name="T">Type of definition to be obtained.</typeparam>
	public static T GetDef<T>(string _sku) where T : Definition {
		// Find out target definition set
		DefinitionSet<T> targetSet = GetDefSet<T>();
		if(targetSet == null) return null;

		// Just ask the target set for the definition with the given sku
		// It will already be of type T (or null if not found)
		return targetSet.GetDef(_sku);
	}

	/// <summary>
	/// Get the definition set of the given definition type.
	/// </summary>
	/// <returns>The requested definition set. <c>null</c> if type doesn't match any of the definition sets defined in the manager.</returns>
	/// <param name="_sku">Id of the definition whose set we want.</param>
	/// <typeparam name="T">Type of definition whose set we want.</typeparam>
	public static DefinitionSet<T> GetDefSet<T>() where T : Definition {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] There might be a better way to do this, no time to research
		// Compact code
		Type defType = typeof(T);

		// Entities
		if(defType == typeof(EntityDef)) {
			return entities as DefinitionSet<T>;
		} else if(defType == typeof(EntityCategoryDef)) {
			return entityCategories as DefinitionSet<T>;
		}

		// Meta-game
		else if(defType == typeof(LevelDef)) {
			return levels as DefinitionSet<T>;
		} else if(defType == typeof(MissionDef)) {
			return missions as DefinitionSet<T>;
		}

		// Definition type not identified
		Debug.Log("Definitions Set for type " + defType.Name + " not found");
		return null;
	}
}