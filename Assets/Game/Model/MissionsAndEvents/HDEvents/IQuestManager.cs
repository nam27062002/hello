using System;
using System.Collections.Generic;

public interface IQuestManager
    {
        bool EventExists();

        bool IsTeasing();

        bool IsRunning();

        bool IsActive();

        bool IsRewardPending();

        string GetGoalDescription();

        HDLiveQuestDefinition GetQuestDefinition();

        HDLiveQuestData GetQuestData();

        double GetRemainingTime();

        long GetRunScore();

        void UpdateStateFromTimers();

        void RequestRewards();
        
        List<HDLiveData.Reward>  GetMyRewards();

        void FinishEvent();

        void ClearEvent();

        string FormatScore(long _score);

        void Contribute(float _runScore, float _keysMultiplier, bool _spentHC, bool _viewAD);

         bool ShouldRequestDefinition();

         bool RequestDefinition(bool _force = false);

         bool IsWaitingForNewDefinition();

    }