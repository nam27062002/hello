// Collectible.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Collectible behaviour.
/// </summary>
public class Collectible : MonoBehaviour {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	[HideInInspector] public int collectedCount = 0;
	#endregion
	
	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private GameEntity mEntity = null;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Related components
		mEntity = GetComponent<GameEntity>();
	
		// Subscribe to game events
		Messenger.AddListener<GameEntity>(GameEvents.ENTITY_EATEN, OnEntityEaten);
	}
	
	/// <summary>
	/// Default destructor
	/// </summary>
	void OnDestroy() {
		// Unsubscribe from game events
		Messenger.RemoveListener<GameEntity>(GameEvents.ENTITY_EATEN, OnEntityEaten);
	}
	#endregion
	
	#region PUBLIC UTILS -----------------------------------------------------------------------------------------------
	/// <summary>
	/// Compute the score given when the entity is eaten/destroyed, taking in account 
	/// current game state.
	/// </summary>
	/// <returns><c>true</c> if the item has been collected at least once.</returns>
	public bool IsCollected() {
		return collectedCount > 0;
	}
	#endregion
	
	#region CALLBACKS -----------------------------------------------------------------------------------------------
	/// <summary>
	/// Raises the entity eaten event.
	/// </summary>
	/// <param name="_entity">The entity that was eaten.</param>
	private void OnEntityEaten(GameEntity _entity) {
		// If it's the collectible, count
		if(_entity == mEntity) {
			collectedCount++;

			// Dispatch game event
			Messenger.Broadcast<Collectible>(GameEvents.COLLECTIBLE_COLLECTED, this);
		}
	}
	#endregion
}
#endregion