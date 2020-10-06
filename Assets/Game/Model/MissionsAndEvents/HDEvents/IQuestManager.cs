using System;
using System.Collections.Generic;

public interface IQuestManager
    {
        /// <summary>
        /// Returns true if there is a valid quest definition
        /// </summary>
        /// <returns></returns>
        bool EventExists();

        /// <summary>
        /// True if the quest is in the teasing state
        /// </summary>
        /// <returns></returns>
        bool IsTeasing();

        /// <summary>
        /// True if this quest is active (not teasing, nor reward pending)
        /// </summary>
        /// <returns></returns>
        bool IsRunning();

        /// <summary>
        /// True if this quest has been activated
        /// </summary>
        /// <returns></returns>
        bool IsActive();

        /// <summary>
        /// True if the quest is in reward available state
        /// </summary>
        /// <returns></returns>
        bool IsRewardPending();

        
        /// <summary>
        /// Returns a human readable description of the quest
        /// </summary>
        /// <returns></returns>
        string GetGoalDescription();

        /// <summary>
        /// Returns the quest definition object
        /// </summary>
        /// <returns></returns>
        HDLiveQuestDefinition GetQuestDefinition();

        /// <summary>
        /// Returns the quest data
        /// </summary>
        /// <returns></returns>
        HDLiveQuestData GetQuestData();

        /// <summary>
        /// Returns the remaining time in seconds for a started quest, or the
        /// time left to start for a teasing quest
        /// </summary>
        /// <returns></returns>
        double GetRemainingTime();

        
        /// <summary>
        /// Returns the scored achieved in the current run
        /// </summary>
        /// <returns></returns>
        long GetRunScore();

        /// <summary>
        /// Set the proper state based on the definition timers
        /// </summary>
        void UpdateStateFromTimers();

        /// <summary>
        /// Make a call to request the rewards
        /// </summary>
        void RequestRewards();
        
        /// <summary>
        /// Returns all the rewards achieved in this quest (based on the rewardLevel value)
        /// </summary>
        /// <returns></returns>
        List<HDLiveData.Reward>  GetMyRewards();

        /// <summary>
        /// Mark event as collected
        /// </summary>
        void FinishEvent();

        /// <summary>
        /// Clear all the pending events, and save them in the cache
        /// </summary>
        void ClearEvent();

        
        /// <summary>
        /// Given a score, format it based on quest type
        /// </summary>
        string FormatScore(long _score);

        /// <summary>
        /// Notify the quest of the contribution made by the player
        /// </summary>
        /// <param name="_runScore"></param>
        /// <param name="_keysMultiplier"></param>
        /// <param name="_spentHC"></param>
        /// <param name="_viewAD"></param>
        void Contribute(float _runScore, float _keysMultiplier, bool _spentHC, bool _viewAD);

        /// <summary>
        /// Whether we need to request the definition again. In cases like event_id changed.
        /// </summary>
        /// <returns></returns>
         bool ShouldRequestDefinition();

        /// <summary>
        /// Makes a call to update the quest definitionT
        /// </summary>
        /// <param name="_force"></param>
        /// <returns>Returns true if the def was requested successfully</returns>
         bool RequestDefinition(bool _force = false);

        /// <summary>
        /// True if the definition was requested, but its still waiting for the response
        /// </summary>
        /// <returns></returns>
         bool IsWaitingForNewDefinition();

    }