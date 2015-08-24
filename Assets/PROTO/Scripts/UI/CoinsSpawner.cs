// CoinsFeedbackSpawner.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Listens to game events and spawns score feedback if required.
/// </summary>
public class CoinsSpawner : MonoBehaviour {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	[SerializeField] private int POOL_SIZE = 10;
	[SerializeField] private GameObject CoinsFeedbackPrefab = null;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private List<CoinsFXController> mPool = new List<CoinsFXController>();	// We don't want to be instantiating objects in real time!
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Pre-initialization.
	/// </summary>
	void Awake() {
		// Checks
		DebugUtils.Assert(CoinsFeedbackPrefab != null, "Required member!");

		// Create an empty object to group all the feedback instances
		// (we can't add them as children of the spawner since it's in a UI canvas and coins feedbacks are in the 3D space)
		GameObject container = new GameObject();
		container.name = "Coins Feedback Container";

		// Allocate pool
		for(int i = 0; i < POOL_SIZE; i++) {
			// Create a new instance of the score feedback prefab
			GameObject CoinsFeedbackObj = Instantiate(CoinsFeedbackPrefab);
			CoinsFXController CoinsFeedback = CoinsFeedbackObj.GetComponent<CoinsFXController>();
			mPool.Add(CoinsFeedback);

			// Add the object as child of the spawner (to keep hierarchy clean) and start inactive
			CoinsFeedbackObj.transform.SetParent(container.transform, false);
			CoinsFeedbackObj.SetActive(false);
		}
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Subscribe to external events
		Messenger.AddListener<long, GameEntity>(GameEvents_OLD.REWARD_COINS, OnCoinsReward);
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update() {
		// Nothing to do for now
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<long, GameEntity>(GameEvents_OLD.REWARD_COINS, OnCoinsReward);
	}
	#endregion

	#region PUBLIC UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Find an available slot in the pool and launch a feedback with the given setup.
	/// </summary>
	/// <param name="_setup">The setup to be launched</param>
	/// <param name="_entity">Entity assigned to this feedback</param> 
	public void LaunchFeedback(long _iAmount, GameEntity _entity) {
		// Find an available instance from the pool
		CoinsFXController targetInstance = null;
		for(int i = 0; i < POOL_SIZE; i++) {
			if(!mPool[i].gameObject.activeSelf) {
				// Object available! Break loop
				targetInstance = mPool[i];
				break;
			}
		}

		// Don't do anything if there are no available instances
		if(targetInstance == null) return;

		// Compute feedback's position
		Vector3 pos = new Vector3();
		// If the target entity has a child named "FeedbackPoint", use it as initial position
		Transform spawnPoint = _entity.transform.FindChild("FeedbackPoint");
		if(spawnPoint != null) {
			pos = spawnPoint.position;
		} else {	// Otherwise let's just use object's position
			pos = _entity.gameObject.transform.position;
		}

		// Launch the score feedback with the target setup
		targetInstance.Launch(pos, _iAmount);
	}
	#endregion

	#region CALLBACKS -------------------------------------------------------------------------------------------------
	/// <summary>
	/// A coins reward has been given
	/// </summary>
	/// <param name="_iAmount">Amount rewarded.</param>
	/// <param name="_entity">Optionally the entity that triggered this reward. <c>null</c> if none.</param>
	void OnCoinsReward(long _iAmount, GameEntity _entity) {
		// Ignore if this coins reward doesn't come from an entity
		if(_entity == null) return;

		// Ignore if coins given to give are 0
		if(_iAmount <= 0) return;

		// Launch a coins feedback
		LaunchFeedback(_iAmount, _entity);
	}
	#endregion
}
#endregion