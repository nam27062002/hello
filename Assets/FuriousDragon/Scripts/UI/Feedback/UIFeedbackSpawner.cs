// UIFeedbackSpawner.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/04/2015.
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
public class UIFeedbackSpawner : MonoBehaviour {
	#region POOL ---------------------------------------------------------------------------------------------------
	// We don't want objects instantiating in real time!
	public UIFeedbackType[] typesSetup;
	private UIFeedbackController[][] mPool;	// One pool per type
	#endregion

	#region STANDARD MESSAGES ------------------------------------------------------------------------------------------
	public UIFeedbackMessage starvingMessage;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Pre-initialization.
	/// </summary>
	void Awake() {
		// Allocate pools
		mPool = new UIFeedbackController[typesSetup.Length][];
		for(int i = 0; i < typesSetup.Length; i++) {
			// Initialize the pool for this type
			mPool[i] = new UIFeedbackController[typesSetup[i].poolSize];

			// Create as many instances as defined of the target feedback prefab for this type
			DebugUtils.Assert(typesSetup[i].prefab != null, "Missing prefab for feedback type " + typesSetup[i].type.ToString() + "!");
			for(int j = 0; j < typesSetup[i].poolSize; j++) {
				// Create the new instance and store a reference to its controller
				GameObject feedbackObj = Instantiate(typesSetup[i].prefab);
				mPool[i][j] = feedbackObj.GetComponent<UIFeedbackController>();
				
				// Add the object as child of the spawner (to keep hierarchy clean) and start inactive
				feedbackObj.transform.SetParent(this.transform, false);
				feedbackObj.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Subscribe to external events
		Messenger.AddListener<long, GameEntity>(GameEvents.REWARD_SCORE, OnScoreReward);
		Messenger.AddListener<GameEntity>(GameEvents.ENTITY_EATEN, OnEntityEaten);
		Messenger.AddListener<GameEntity>(GameEvents.ENTITY_BURNED, OnEntityBurned);
		Messenger.AddListener<float, DamageDealer>(GameEvents.PLAYER_IMPACT_RECEIVED, OnPlayerDamage);
		Messenger.AddListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnPlayerStarving);
		Messenger.AddListener<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnScoreMultiplierChanged);
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<long, GameEntity>(GameEvents.REWARD_SCORE, OnScoreReward);
		Messenger.RemoveListener<GameEntity>(GameEvents.ENTITY_EATEN, OnEntityEaten);
		Messenger.RemoveListener<GameEntity>(GameEvents.ENTITY_BURNED, OnEntityBurned);
		Messenger.RemoveListener<float, DamageDealer>(GameEvents.PLAYER_IMPACT_RECEIVED, OnPlayerDamage);
		Messenger.RemoveListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnPlayerStarving);
		Messenger.RemoveListener<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnScoreMultiplierChanged);
	}
	#endregion

	#region PUBLIC UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Find an available slot in the pool and launch a feedback with the given message.
	/// </summary>
	/// <param name="_message">The message to be launched</param>
	/// <param name="_entity">Entity linked to this feedback. Can be null.</param> 
	public void LaunchFeedback(UIFeedbackMessage _message, GameEntity _entity) {
		// Checks
		if(_message == null) return;

		// Find an available controller from the pool assigned to this type
		// Pick the first inactive controller of the target type
		// If there are no inactive controllers, pick the one with the longest lifetime
		UIFeedbackController targetController = null;
		UIFeedbackController oldestActiveController = null;
		foreach(UIFeedbackController c in mPool[(int)_message.type]) {
			// Is it active?
			if(c.gameObject.activeSelf) {
				// Yes!! Check whether it's the oldest one
				if(oldestActiveController == null || oldestActiveController.lifetime < c.lifetime) {
					oldestActiveController = c;
				}
			} else {
				// No! Use this instance, break loop
				targetController = c;
				break;
			}
		}

		// If no inactive controllers were found, use the oldest one
		if(targetController == null) {
			targetController = oldestActiveController;
		}

		// Just in case, don't do anything if no controller was found (shouldn't happen unless pool size is 0)
		if(targetController == null) return;

		// If the target feedback has a WorldSpacePositioner, link it to the target entity
		WorldSpacePositioner positioner = targetController.gameObject.GetComponent<WorldSpacePositioner>();
		if(positioner && _entity) {
			// If the target entity has a child named "FeedbackPoint", use it as initial position. Otherwise use object's position.
			Transform spawnPoint = _entity.transform.FindChild("FeedbackPoint");
			if(spawnPoint != null) {
				positioner.targetWorldPosition = spawnPoint.position;
			} else {
				positioner.targetWorldPosition = _entity.transform.position;
			}
		}

		// Restart the controller
		targetController.Launch(_message);
	}

	/// <summary>
	/// Stop all feedbacks of the given type.
	/// </summary>
	/// <param name="_eType">The type of feedback to be stopped.</param>
	public void StopFeedback(EUIFeedbackType _eType) {
		foreach(UIFeedbackController c in mPool[(int)_eType]) {
			// Is it active?
			if(c.gameObject.activeSelf) {
				// Yes!! Trigger the end animation
				c.End();
			}
		}
	}
	#endregion

	#region INTERNAL UTILS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Pick a random message from a given list.
	/// </summary>
	/// <returns>A random message picked from the input list. <c>null<c/> if it was not possible to pick one.</returns>
	/// <param name="_candidates">The list of possible messages to be picked.</param>
	UIFeedbackMessage PickRandomMessage(List<UIFeedbackMessage> _candidates) {
		// Checks
		if(_candidates.Count == 0) return null;

		// Easy
		return _candidates[UnityEngine.Random.Range(0, _candidates.Count)];
	}
	#endregion

	#region CALLBACKS -------------------------------------------------------------------------------------------------
	/// <summary>
	/// The score has been changed.
	/// </summary>
	/// <param name="_iAmount">Rewarded amount.</param>
	/// <param name="_entity">Optionally the entity that triggered this score change. <c>null</c> if none.</param>
	void OnScoreReward(long _iAmount, GameEntity _entity) {
		// Ignore if this score change doesn't come from an entity
		if(_entity == null) return;

		// Ignore if score to give is 0
		if(_iAmount <= 0) return;

		// Create a new message of score type
		UIFeedbackMessage msg = new UIFeedbackMessage();
		msg.text = String.Format("+{0}", _iAmount);
		msg.type = EUIFeedbackType.SCORE;
		LaunchFeedback(msg, _entity);
	}

	/// <summary>
	/// An entity has been eaten.
	/// </summary>
	/// <param name="_entity">The eaten entity.</param>
	void OnEntityEaten(GameEntity _entity) {
		// Does it have any feedback to be displayed?
		if(_entity) {
			// Entity should always have an edible behaviour if it has been eaten, but just in case
			EdibleBehaviour edible = _entity.GetComponent<EdibleBehaviour>();
			if(edible && edible.eatFeedbacks.Count > 0) {
				// Probability of spawning a feedback message
				if(UnityEngine.Random.Range(0f, 1f) < edible.feedbackProbability) {
					// Pick a random one and launch it
					LaunchFeedback(PickRandomMessage(edible.eatFeedbacks), _entity);
				}
			}
		}
	}

	/// <summary>
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The burned entity.</param>
	void OnEntityBurned(GameEntity _entity) {
		// Does it have any feedback to be displayed?
		if(_entity) {
			// Entity should always have a flamable behaviour if it has been burned, but just in case
			FlamableBehaviour flamable = _entity.GetComponent<FlamableBehaviour>();
			if(flamable && flamable.burnFeedbacks.Count > 0) {
				// Probability of spawning a feedback
				if(UnityEngine.Random.Range(0f, 1f) < flamable.feedbackProbability) {
					// Pick a random one and launch it
					LaunchFeedback(PickRandomMessage(flamable.burnFeedbacks), _entity);
				}
			}
		}
	}

	/// <summary>
	/// The player has received damage from an entity.
	/// </summary>
	/// <param name="_fDamage">The amaount of damage dealt.</param>
	/// <param name="_source">The object that dealt the damage.</param>
	void OnPlayerDamage(float _fDamage, DamageDealer _source) {
		// Don't show message if starving - already have the starving warning
		if(App.Instance.gameLogic.player.IsStarving()) return;

		// Does it have any feedback to be displayed?
		if(_source && _source.damageFeedbacks.Count > 0) {
			// Probability of spawning a feedback
			if(UnityEngine.Random.Range(0f, 1f) < _source.feedbackProbability) {
				// Pick a random one and launch it
				LaunchFeedback(PickRandomMessage(_source.damageFeedbacks), null);	// [AOC] No need to attach an entity since the damage feedback is not attached to any object
			}
		}
	}

	/// <summary>
	/// The player has gone in/out of the starving state.
	/// </summary>
	/// <param name="_bIsStarving">Whether the player is starving or not.</param>
	void OnPlayerStarving(bool _bIsStarving) {
		// Show or hide?
		if(_bIsStarving) {
			LaunchFeedback(starvingMessage, null);
		} else {
			StopFeedback(EUIFeedbackType.STARVING);
		}
	}

	/// <summary>
	/// The current score multiplier has changed.
	/// </summary>
	/// <param name="_oldMultiplier">Old value of the score multiplier.</param>
	/// <param name="_newMultiplier">New value of the score multiplier.</param>
	void OnScoreMultiplierChanged(ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier) {
		// If score multiplier is back to 1, hide any active feedback
		if(_newMultiplier.multiplier == 1) {
			StopFeedback(EUIFeedbackType.MULTIPLIER);
		} else {
			// Create a new message showing the multiplier amount
			UIFeedbackMessage msg = new UIFeedbackMessage();
			msg.text = String.Format("x{0}", _newMultiplier.multiplier);
			msg.type = EUIFeedbackType.MULTIPLIER;
			LaunchFeedback(msg, null);

			// Launch also a fun string feedback message
			if(_newMultiplier.feedbackMessages.Count > 0) {
				// Pick a random one and launch it
				LaunchFeedback(PickRandomMessage(_newMultiplier.feedbackMessages), null);
			}
		}
	}
	#endregion
}
#endregion