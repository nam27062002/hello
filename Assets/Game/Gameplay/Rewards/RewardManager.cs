// ScoreManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Aux class to represent the score multipliers.
/// </summary>
[Serializable]
public class ScoreMultiplier {
	public float multiplier = 1;
	public int requiredKillStreak = 1;	// Eat, burn and destroy count
	public float duration = 20f;	// Seconds to keep the streak alive
	public List<string> feedbackMessages = new List<string>();
}

/// <summary>
/// Global rewards controller. Keeps current game score, coins earned, etc.
/// Singleton class, access it via its static methods.
/// </summary>
public class RewardManager : UbiBCN.SingletonMonoBehaviour<RewardManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Score multiplier
	[SerializeField] private ScoreMultiplier[] m_scoreMultipliers;
	private int m_scoreMultiplierStreak = 0;	// Amount of consecutive eaten/burnt/destroyed entities without taking damage

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Basic rewards
	// Exposed for easier debugging
	[SerializeField] private long m_score = 0;
	public static long score { 
		get { return instance.m_score; }
	}

	[SerializeField] private long m_coins = 0;
	public static long coins {
		get { return instance.m_coins; }
	}

	[SerializeField] private long m_pc = 0;
	public static long pc {
		get { return instance.m_pc; }
	}

	[SerializeField] private float m_xp = 0;
	public static float xp {
		get { return instance.m_xp; }
	}

	// Score multiplier
	[SerializeField] private int m_currentScoreMultiplierIndex = 0;
	public static ScoreMultiplier currentScoreMultiplierData {
		get { return instance.m_scoreMultipliers[instance.m_currentScoreMultiplierIndex]; }
	}

	[SerializeField] private float m_currentFireRushMultiplier = 1;
	public static float currentFireRushMultiplier {
		get { return instance.m_currentFireRushMultiplier; }
		set { instance.m_currentFireRushMultiplier = value; }
	}


	public static float currentScoreMultiplier{
		get{ return instance.m_scoreMultipliers[instance.m_currentScoreMultiplierIndex].multiplier * instance.m_currentFireRushMultiplier; }
	}

	public static ScoreMultiplier defaultScoreMultiplier {
		get { return instance.m_scoreMultipliers[0]; }
	}

	// Progress to the next multiplier
	public static float scoreMultiplierProgress {
		get { 
			// Skip last multiplier!
			if(instance.m_currentScoreMultiplierIndex < instance.m_scoreMultipliers.Length - 1) {
				return Mathf.InverseLerp(
					(float)currentScoreMultiplierData.requiredKillStreak, 
					(float)instance.m_scoreMultipliers[instance.m_currentScoreMultiplierIndex + 1].requiredKillStreak, 
					(float)instance.m_scoreMultiplierStreak);
			}
			return 0f;
		}
	}

	// Time to end the current killing streak
	[SerializeField] private float m_scoreMultiplierTimer = -1;
	public static float scoreMultiplierTimer {
		get { return instance.m_scoreMultiplierTimer; }
	}

	[SerializeField] private float m_burnCoinsMultiplier = 1;
	public static float burnCoinsMultiplier {
		get { return instance.m_burnCoinsMultiplier; }
		set { instance.m_burnCoinsMultiplier = value; }
	}

	// Survival Bonus Data
	List<Dictionary<string, int>> m_survivalBonus = new List<Dictionary<string, int>>();
	int m_lastAwardedSurvivalBonusMinute = -1;
	private GameSceneControllerBase m_sceneController;

	// Highscore
	private bool m_isHighScore;
	public static bool isHighScore
	{
		get {  return instance.m_isHighScore; }
	}

	// Dragon progression - store dragon progression at the beginning of the game
	private int m_dragonInitialLevel = 0;
	public static int dragonInitialLevel {
		get { return instance.m_dragonInitialLevel; }
	}

	private float m_dragonInitialLevelProgress = 0;
	public static float dragonInitialLevelProgress {
		get { return instance.m_dragonInitialLevelProgress; }
	}

	// Chests - store chest progression at the beginning of the game
	private int m_initialCollectedChests = 0;
	public static int initialCollectedChests {
		get { return instance.m_initialCollectedChests; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		InitFromDef();
	}

	/// <summary>
	/// The manager has been enabled.
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnKill);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurned);
		Messenger.AddListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnKill);
		Messenger.AddListener<Transform, Reward>(GameEvents.FLOCK_EATEN, OnFlockEaten);
		Messenger.AddListener<Reward>(GameEvents.LETTER_COLLECTED, OnLetterCollected);
		Messenger.AddListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryRush);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// The manager has been disabled.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_EATEN, OnKill);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_BURNED, OnBurned);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.ENTITY_DESTROYED, OnKill);
		Messenger.RemoveListener<Transform, Reward>(GameEvents.FLOCK_EATEN, OnFlockEaten);
		Messenger.RemoveListener<Reward>(GameEvents.LETTER_COLLECTED, OnLetterCollected);
		Messenger.RemoveListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryRush);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Update score multiplier (won't be called if we're in the first multiplier)
		if(m_scoreMultiplierTimer > 0) {
			// Update timer
			m_scoreMultiplierTimer -= Time.deltaTime;
			
			// If timer has ended, end multiplier streak
			if(m_scoreMultiplierTimer <= 0) 
			{
				if (m_currentScoreMultiplierIndex != 0)
				{
					SetScoreMultiplier(0);
					Messenger.Broadcast(GameEvents.SCORE_MULTIPLIER_LOST);
				}
			}
		}

		// Check survival bonus
		CheckSurvivalBonus();
	}

	/// <summary>
	/// Read setup from definitions.
	/// Not-static since instance can't be accessed during the Awake call.
	/// </summary>
	public void InitFromDef() {
		// Init score multipliers
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.SCORE_MULTIPLIERS);
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		m_scoreMultipliers = new ScoreMultiplier[defs.Count];
		ScoreMultiplier newMult;
		List<DefinitionNode> feedbackDefs;
		for(int i = 0; i < defs.Count; i++) {
			// Create a new score multiplier
			newMult = new ScoreMultiplier();
			newMult.multiplier = defs[i].GetAsFloat("multiplier");
			newMult.requiredKillStreak = defs[i].GetAsInt("requiredKillStreak");
			newMult.duration = defs[i].GetAsFloat("duration");

			// Feedback messages
			feedbackDefs = defs[i].GetChildNodesByTag("FeedbackMessage");
			for(int j = 0; j < feedbackDefs.Count; j++) {
				newMult.feedbackMessages.Add(feedbackDefs[j].GetAsString("tidMessage"));
			}

			// Store new multiplier
			m_scoreMultipliers[i] = newMult;
		}


	}

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Reset the temp data. To be called, for example, when starting a new game.
	/// </summary>
	public static void Reset() {
		// Score
		instance.m_score = 0;
		instance.m_coins = 0;
		instance.m_pc = 0;
		instance.m_xp = 0;

		// Multipliers
		instance.SetScoreMultiplier(0);

		instance.m_sceneController = InstanceManager.gameSceneControllerBase;
		instance.ParseSurvivalBonus( InstanceManager.player.data.tierDef.sku );

		instance.m_isHighScore = false;

		// Current dragon progress
		if(DragonManager.currentDragon != null) {
			instance.m_dragonInitialLevel = DragonManager.currentDragon.progression.level;
			instance.m_dragonInitialLevelProgress = DragonManager.currentDragon.progression.progressCurrentLevel;
		} else {
			instance.m_dragonInitialLevel = 1;
			instance.m_dragonInitialLevelProgress = 0;
		}

		// Chests
		instance.m_initialCollectedChests = ChestManager.collectedAndPendingChests;
	}

	/// <summary>
	/// Adds the current rewards to the user profile. To be called at the end of 
	/// the game, for example.
	/// </summary>
	public static void ApplyRewardsToProfile() {
		// Just do it :)
		UsersManager.currentUser.AddCoins(instance.m_coins);
		UsersManager.currentUser.AddCoins(instance.CalculateSurvivalBonus());
		UsersManager.currentUser.AddPC(instance.m_pc);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Time elapsed.
	/// </summary>
	private float GameTime()
	{
		if (m_sceneController != null)
			return m_sceneController.elapsedSeconds;
		return 0;
	}

	/// <summary>
	/// Apply the given rewards package.
	/// </summary>
	/// <param name="_reward">The rewards to be applied.</param>
	/// <param name="_entity">The entity that has triggered the reward. Can be null.</param>
	private void ApplyReward(Reward _reward, Transform _entity) {
		// Score
		// Apply multiplier
		_reward.score = (int)(_reward.score * currentScoreMultiplier);
		instance.m_score += _reward.score;

		// Coins
		instance.m_coins += _reward.coins;

		// PC
		instance.m_pc += _reward.pc;

		// XP
		InstanceManager.player.data.progression.AddXp(_reward.xp, true);
		instance.m_xp += _reward.xp;

		// Global notification (i.e. to show feedback)
		Messenger.Broadcast<Reward, Transform>(GameEvents.REWARD_APPLIED, _reward, _entity);
	}

	//------------------------------------------------------------------//
	// SCORE MULTIPLIER MANAGEMENT										//
	//------------------------------------------------------------------//
	/// <summary>
	/// Define the new score multiplier.
	/// </summary>
	/// <param name="_multiplierIdx">The index of the new multiplier in the SCORE_MULTIPLIERS array.</param>
	private void SetScoreMultiplier(int _multiplierIdx) {
		// Make sure given index is valid
		if(_multiplierIdx < 0 || _multiplierIdx >= m_scoreMultipliers.Length) return;
		
		// Reset everything when going to 0
		if(_multiplierIdx == 0) {
			m_scoreMultiplierTimer = -1;
			m_scoreMultiplierStreak = 0;
		}

		// Store new multiplier value
		ScoreMultiplier old = m_scoreMultipliers[m_currentScoreMultiplierIndex];
		m_currentScoreMultiplierIndex = _multiplierIdx;

		// Dispatch game event (only if actually changing)
		if(old != currentScoreMultiplierData) {
			Messenger.Broadcast<ScoreMultiplier, float>(GameEvents.SCORE_MULTIPLIER_CHANGED, currentScoreMultiplierData, m_currentFireRushMultiplier);
		}
	}
	
	/// <summary>
	/// Update the score multiplier with a new kill (eat/burn/destroy).
	/// </summary>
	private void UpdateScoreMultiplier() {
		// Update current streak
		m_scoreMultiplierStreak++;
		
		// Reset timer
		m_scoreMultiplierTimer = currentScoreMultiplierData.duration;
		
		// Check if we've reached next threshold
		if(m_currentScoreMultiplierIndex < m_scoreMultipliers.Length - 1 
		&& m_scoreMultiplierStreak >= m_scoreMultipliers[m_currentScoreMultiplierIndex + 1].requiredKillStreak) {
			// Yes!! Change current multiplier
			SetScoreMultiplier(m_currentScoreMultiplierIndex + 1);
		}
	}

	//------------------------------------------------------------------//
	// SURVIVAL BONUS MANAGEMENT										//
	//------------------------------------------------------------------//
	/// <summary>
	/// Checks the survival bonus.
	/// </summary>
	private void CheckSurvivalBonus()
	{
		//Check if a survival minute has passed and show the notification to the user!
		float elapsedTime = GameTime();
		int elapsedMinutes = (int)Math.Floor(elapsedTime / 60);

		if( elapsedMinutes > m_lastAwardedSurvivalBonusMinute )
		{
			foreach(Dictionary<string, int> data in m_survivalBonus)
			{
				if( elapsedMinutes == data["minMinutes"] )
				{
					m_lastAwardedSurvivalBonusMinute = elapsedMinutes;

					// Trigger event so HUD can show an event!
					Messenger.Broadcast(GameEvents.SURVIVAL_BONUS_ACHIEVED);

					break;
				}
			}
		}
	}


	/// <summary>
	/// Parses the survival bonus. It fills m_survivalBonus
	/// </summary>
	private void ParseSurvivalBonus( string tier )
	{
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable( DefinitionsCategory.SURVIVAL_BONUS , "tier", tier);

		List<int> minutes = def.GetAsList<int>("minutes");
		List<int> coins = def.GetAsList<int>("coins");

		DebugUtils.Assert( minutes.Count == coins.Count, "Minues and Coins for tier " + tier + " don't match");

		m_survivalBonus.Clear();
		for(int i = 0; i < minutes.Count; i++)
		{
			m_survivalBonus.Add(new Dictionary<string, int>
				{
					{ "minMinutes", minutes[i] },
					{ "coins", coins[i] },
				});
		}

		m_survivalBonus.Sort(delegate(Dictionary<string, int> a, Dictionary<string, int> b)
			{
				return a["minMinutes"] - b["minMinutes"];
			});
	}

	/// <summary>
	/// Calculates the survival bonus at the end of the game
	/// </summary>
	/// <returns>The survival bonus.</returns>
	public int CalculateSurvivalBonus()
	{
		//Survival bonus is awarded for every block of 10 seconds the user has survived
		float elapsedTime = GameTime();
		int elapsedMinutes = (int)Math.Floor(elapsedTime / 60);
		int elapsedBlocks = (int)Math.Floor(elapsedTime / 10);

		int coinsPerBlock = 0;

		var iterator = m_survivalBonus.GetEnumerator();
		while(iterator.MoveNext())
		{
			Dictionary<string, int> data = iterator.Current;
			if(elapsedMinutes >= data["minMinutes"])
			{
				coinsPerBlock = data["coins"];
			}
		}

		int survivalBonus = (int)Math.Floor(elapsedBlocks * coinsPerBlock * 1.0f); // TODO (miguel); m_accessorySurvivalBonusMultiplier
		return survivalBonus;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An entity has been eaten, burned or killed. If any of these events requires 
	/// a specific implementation, just create a custom callback for it.
	/// </summary>
	/// <param name="_entity">The entity that has been killed.</param>
	/// <param name="_reward">The reward linked to this event.</param>
	private void OnKill(Transform _entity, Reward _reward) {
		// Add the reward
		ApplyReward(_reward, _entity);

		// Update multiplier
		UpdateScoreMultiplier();
	}

	private void OnBurned( Transform _entity, Reward _reward){
		_reward.coins = (int)(_reward.coins * m_burnCoinsMultiplier);
		OnKill( _entity, _reward );
	}

	private void OnFlockEaten(Transform _entity, Reward _reward) {
		// Add the reward
		ApplyReward(_reward, _entity);
	}

	private void OnLetterCollected(Reward _reward){
		ApplyReward(_reward, null);
	}

	/// <summary>
	/// The player has received damage.
	/// </summary>
	/// <param name="_amount">The amount of damage received.</param>
	/// <param name="_type">The type of damage received.</param>
	/// <param name="_source">The source of the damage.</param>
	private void OnDamageReceived(float _amount, DamageType _type, Transform _source) {
		// Break current streak
		if (m_currentScoreMultiplierIndex != 0)
		{
			SetScoreMultiplier(0);
			Messenger.Broadcast(GameEvents.SCORE_MULTIPLIER_LOST);
		}
	}

	private void OnFuryRush(bool toggle, DragonBreathBehaviour.Type type )
	{
		Messenger.Broadcast<ScoreMultiplier, float>(GameEvents.SCORE_MULTIPLIER_CHANGED, currentScoreMultiplierData, m_currentFireRushMultiplier);
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded()
	{
		// Check final score and mark if its a new HighScore
		if ( m_score > UsersManager.currentUser.highScore )
		{
			m_isHighScore = true;
			UsersManager.currentUser.highScore = m_score;
			PersistenceManager.Save();
		}
		else
		{
			m_isHighScore = false;
		}

	}
}