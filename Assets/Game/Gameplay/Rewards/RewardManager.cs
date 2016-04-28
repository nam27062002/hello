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
public class RewardManager : SingletonMonoBehaviour<RewardManager> {
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

	// Score multiplier
	[SerializeField] private int m_currentScoreMultiplier = 0;
	public static ScoreMultiplier currentScoreMultiplier {
		get { return instance.m_scoreMultipliers[instance.m_currentScoreMultiplier]; }
	}
	public static ScoreMultiplier defaultScoreMultiplier {
		get { return instance.m_scoreMultipliers[0]; }
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
		Messenger.AddListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
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
		Messenger.RemoveListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
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
			if(m_scoreMultiplierTimer <= 0) {
				SetScoreMultiplier(0);
			}
		}

		// Check survival bonus
		// CheckSurvivalBonus();
	}

	/// <summary>
	/// Read setup from definitions.
	/// Not-static since instance can't be accessed during the Awake call.
	/// </summary>
	public void InitFromDef() {
		// Init score multipliers
		List<DefinitionNode> defs = DefinitionsManager.GetDefinitions(DefinitionsCategory.SCORE_MULTIPLIERS);
		DefinitionsManager.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		m_scoreMultipliers = new ScoreMultiplier[defs.Count];
		ScoreMultiplier newMult;
		List<DefinitionNode> feedbackDefs;
		for(int i = 0; i < defs.Count; i++) {
			// Create a new score multiplier
			newMult = new ScoreMultiplier();
			newMult.multiplier = defs[i].Get<float>("multiplier");
			newMult.requiredKillStreak = defs[i].Get<int>("requiredKillStreak");
			newMult.duration = defs[i].Get<float>("duration");

			// Feedback messages
			feedbackDefs = defs[i].GetChildNodesByTag("FeedbackMessage");
			for(int j = 0; j < feedbackDefs.Count; j++) {
				newMult.feedbackMessages.Add(feedbackDefs[j].Get<string>("tidMessage"));
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

		// Multipliers
		instance.SetScoreMultiplier(0);

		instance.m_sceneController = InstanceManager.GetSceneController<GameSceneControllerBase>();
		// Survival Bonus
		instance.ParseSurvivalBonus( InstanceManager.player.data.tierDef.sku );

		instance.m_isHighScore = false;
	}

	/// <summary>
	/// Adds the current rewards to the user profile. To be called at the end of 
	/// the game, for example.
	/// </summary>
	public static void ApplyRewardsToProfile() {
		// Just do it :)
		UserProfile.AddCoins(instance.m_coins);
		UserProfile.AddCoins(instance.CalculateSurvivalBonus());
		UserProfile.AddPC(instance.m_pc);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply the given rewards package.
	/// </summary>
	/// <param name="_reward">The rewards to be applied.</param>
	/// <param name="_entity">The entity that has triggered the reward. Can be null.</param>
	private void ApplyReward(Reward _reward, Transform _entity) {
		// Score
		// Apply multiplier
		_reward.score = (int)(_reward.score * currentScoreMultiplier.multiplier);
		instance.m_score += _reward.score;

		// Coins
		instance.m_coins += _reward.coins;

		// PC
		instance.m_pc += _reward.pc;

		// XP
		InstanceManager.player.data.progression.AddXp(_reward.xp, true);

		// Global notification (i.e. to show feedback)
		Messenger.Broadcast<Reward, Transform>(GameEvents.REWARD_APPLIED, _reward, _entity);
	}

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
		ScoreMultiplier old = m_scoreMultipliers[m_currentScoreMultiplier];
		m_currentScoreMultiplier = _multiplierIdx;

		// Dispatch game event (only if actually changing)
		if(old != currentScoreMultiplier) {
			Messenger.Broadcast<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, old, currentScoreMultiplier);
		}
	}
	
	/// <summary>
	/// Update the score multiplier with a new kill (eat/burn/destroy).
	/// </summary>
	private void UpdateScoreMultiplier() {
		// Update current streak
		m_scoreMultiplierStreak++;
		
		// Reset timer
		m_scoreMultiplierTimer = currentScoreMultiplier.duration;
		
		// Check if we've reached next threshold
		if(m_currentScoreMultiplier < m_scoreMultipliers.Length - 1 
		&& m_scoreMultiplierStreak >= m_scoreMultipliers[m_currentScoreMultiplier + 1].requiredKillStreak) {
			// Yes!! Change current multiplier
			SetScoreMultiplier(m_currentScoreMultiplier + 1);
		}
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

	/// <summary>
	/// The player has received damage.
	/// </summary>
	/// <param name="_amount">The amount of damage received.</param>
	/// <param name="_source">The source of the damage.</param>
	private void OnDamageReceived(float _amount, Transform _source) {
		// Break current streak
		SetScoreMultiplier(0);
	}

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

					// play SFX.
					// AudioManager.PlaySfx(AudioManager.Ui.SurvivalBonus);
                    // if(TextSystem.Instance != null)
                    // {
                      //  TextSystem.Instance.ShowSituationalText(SituationalTextSystem.Type.SurvivalBonus);
                    //}
					// EventManager.Instance.TriggerEvent(Events.SurvivalBonusAchieved);

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
		DefinitionNode def = DefinitionsManager.GetDefinitionByVariable( DefinitionsCategory.SURVIVAL_BONUS , "tier", tier);

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


    private float GameTime()
    {
		if (m_sceneController != null)
			return m_sceneController.elapsedSeconds;
		return 0;
    }


    // 
	void OnGameEnded()
	{
		// Check final score and mark if its a new HighScore
		if ( m_score > UserProfile.highScore )
		{
			m_isHighScore = true;
			UserProfile.highScore = m_score;
			UserProfile.Save();
		}
		else
		{
			m_isHighScore = false;
		}

	}
}