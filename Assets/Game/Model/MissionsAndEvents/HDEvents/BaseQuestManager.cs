using System;
using System.Collections.Generic;

//------------------------------------------------------------------------//
// CLASSES										                                                      //
//------------------------------------------------------------------------//
/// <summary>
/// Abstract class to gather all the common stuff for the live quests and the solo quests
/// </summary>
public abstract class BaseQuestManager: HDLiveEventManager, IBroadcastListener
    {
     
     //------------------------------------------------------------------------//
     // MEMBERS AND PROPERTIES												                                     //
     //------------------------------------------------------------------------//
     protected TrackerBase m_tracker = new TrackerBase();

     protected new HDLiveQuestData m_data;
     protected new HDLiveQuestDefinition m_def;
     
     public override HDLiveEventData data
     {	
         get { return m_data; }
     }


     //------------------------------------------------------------------------//
     // GENERIC METHODS               												                             //
     //------------------------------------------------------------------------//
     
     protected BaseQuestManager()
     {
      
        m_data = new HDLiveQuestData();
        m_def = m_data.definition as HDLiveQuestDefinition;
        
        // Subscribe to external events
        Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
     }

     /// <summary>
     /// Destructor
     /// </summary>
     ~BaseQuestManager()
     {
        m_data = null;
        m_def = null;
     }
     
     //------------------------------------------------------------------------//
     // METHODS               												                                     //
     //------------------------------------------------------------------------//
     
     
     /// <summary>
     /// Returns a human readable description of the quest
     /// </summary>
     /// <returns></returns>
     public string GetGoalDescription()
     {
         return m_tracker.FormatDescription(m_def.m_goal.m_desc, m_def.m_goal.m_amount);
     }

     /// <summary>
     /// Returns the quest definition object
     /// </summary>
     /// <returns></returns>
     public HDLiveQuestDefinition GetQuestDefinition()
     {
          return m_def;
     }


     /// <summary>
     /// Returns the quest data
     /// </summary>
     /// <returns></returns>
     public HDLiveQuestData GetQuestData()
     {
          return m_data;
     }


     /// <summary>
     /// Returns the scored achieved in the current run
     /// </summary>
     /// <returns></returns>
     public long GetRunScore()
     {
          return m_tracker.currentValue;
     }

     //------------------------------------------------------------------------//
     // ABSTRACT METHODS                                                       //
     //------------------------------------------------------------------------//
     
     /// <summary>
     /// Notify the quest of the contribution made by the player
     /// </summary>
     /// <param name="_runScore"></param>
     /// <param name="_keysMultiplier"></param>
     /// <param name="_spentHC"></param>
     /// <param name="_viewAD"></param>
     public abstract void Contribute(float _runScore, float _keysMultiplier, bool _spentHC, bool _viewAD);

     public abstract bool IsWaitingForNewDefinition();
     
    

     //------------------------------------------------------------------------//
     // PARENT OVERRIDING                           //
     //------------------------------------------------------------------------//
     
     /// <summary>
     /// Returns all the rewards achieved in this quest (based on the rewardLevel value)
     /// </summary>
     /// <returns></returns>
     public override List<HDLiveData.Reward> GetMyRewards()
     {
         // Create new list
         List<HDLiveData.Reward> rewards = new List<HDLiveData.Reward>();

         // We must have a valid data and definition
         if(m_data != null && m_data.definition != null) {
             // Check reward level
             // In a quest, the reward level tells us in which reward tier have been reached
             // All rewards below it are also given
             HDLiveQuestDefinition def = m_data.definition as HDLiveQuestDefinition;
             for(int i = 0; i < m_rewardLevel; ++i) {	// Reward level is 1-N
                 rewards.Add(def.m_rewards[i]);	// Assuming rewards are properly sorted :)
             }
         }

         // Done!
         return rewards;
     }
     
     //------------------------------------------------------------------------//
     // EVENTS                          //
     //------------------------------------------------------------------------//
     
         
     public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
     {
         switch(eventType)
         {
             case BroadcastEventType.GAME_ENDED:
             {
                 OnGameEnded();
             }
                 break;
         }
     }
     
     public void OnGameStarted(){
          if ( m_tracker != null)
          {
               m_tracker.enabled = m_active;
               m_tracker.InitValue(0);
          }
     }
     
     public void OnGameEnded(){
        // Save tracker value?
     }
     
     
     
     
     //------------------------------------------------------------------------//
     // UI HELPER METHODS													  //
     //------------------------------------------------------------------------//
     /// <summary>
     /// Given a score, format it based on quest type
     /// </summary>
     public string FormatScore(long _score) {
          // Tracker will do it for us
          if(m_tracker != null) {
                return m_tracker.FormatValue(_score);
          }
          return StringUtils.FormatNumber(_score); 
     }
     
}