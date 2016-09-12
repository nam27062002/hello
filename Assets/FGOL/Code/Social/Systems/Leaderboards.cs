using FGOL.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboards
{
	public const float LeaderboardRequestTimeout = 300f; // 5 minutes.

	public struct LeaderboardEntry
    {
        public string fgolID;
        public string socialID;
        public SocialFacade.Network network;
        public string name;
        public int position;
        public float percentile;
        public int score;
        public int timestamp;
        public int playTime;
    }

    public struct EquippableRecipe
    {
        public string key;
        public int level;
    }

    public struct SharkRecipe
    {
        public string key;
        public float growth;
        public List<EquippableRecipe> items;
        public List<string> pets;
        public int lastUpdate;
    }

    public struct SharkEntry
    {
        public string fgolID;
        public string socialID;
        public SocialFacade.Network network;
        public string name;
        public int lastLogin;
        public int playTime;
        public int score;
        public int numSharksOwned;
        public SharkRecipe? shark;
    }

	private class CacheItem<T>
	{
		public T cache;
		public SocialManagerUtilities.ConnectionState state;
		public float lastUpdateTime;

		public CacheItem(T cache, SocialManagerUtilities.ConnectionState state, float lastUpdateTime = float.MinValue)
		{
			this.lastUpdateTime = lastUpdateTime;
			this.cache = cache;
			this.state = state;
		}
	}

    private bool m_inited = false;

	private Dictionary<string, CacheItem<LeaderboardEntry[]>> m_leaderboardsCache = new Dictionary<string, CacheItem<LeaderboardEntry[]>>();
	private CacheItem<SharkEntry[]> m_sharksCache;

	public void Init()
    {
        if (!m_inited)
        {
            m_inited = true;

            GameServicesInterface.LeaderboardDef global = new GameServicesInterface.LeaderboardDef();
            global.GameCenterID = "com.hungrysharkworld.global";
            global.GooglePlayServicesID = GooglePlayServicesConstants.leaderboard_hungry_shark_global_leaderboard;
            global.GameCircleID = "CgkInJiYwPwaEAIQAQ";

            GameServicesInterface.LeaderboardDef hawaii = new GameServicesInterface.LeaderboardDef();
            hawaii.GameCenterID = "com.hungrysharkworld.hawaii";
            hawaii.GooglePlayServicesID = GooglePlayServicesConstants.leaderboard_hungry_shark_pacific_islands_leaderboard;
            hawaii.GameCircleID = "CgkInJiYwPwaEAIQAg";

            GameServicesInterface.LeaderboardDef middleEast = new GameServicesInterface.LeaderboardDef();
            middleEast.GameCenterID = "com.hungrysharkworld.middleeast";
            middleEast.GooglePlayServicesID = GooglePlayServicesConstants.leaderboard_hungry_shark_arabian_sea_leaderboard;
            middleEast.GameCircleID = "CgkInJiYwPwaEAIQAw";

            GameServicesInterface.LeaderboardDef arctic = new GameServicesInterface.LeaderboardDef();
            arctic.GameCenterID = "com.hungrysharkworld.arctic";
            arctic.GooglePlayServicesID = GooglePlayServicesConstants.leaderboard_hungry_shark_arctic_ocean_leaderboard;
            arctic.GameCircleID = "CgkInJiYwPwaEAIQBA";

            GameServicesFacade.Instance.RegisterLeaderboard("global", global);
            GameServicesFacade.Instance.RegisterLeaderboard("Hawaii", hawaii);
            GameServicesFacade.Instance.RegisterLeaderboard("MiddleEast", middleEast);
            GameServicesFacade.Instance.RegisterLeaderboard("Arctic", arctic);

			EventManager.Instance.RegisterEvent(Events.GoInGameButtonPressed, OnGoInGame);
        }
    }

    public void RecordUserScore(int score, int playTime)
    {
        Debug.Log("Leaderboards (RecordUserScore) :: Recording users highscore");

        //[DGR] LEADERBOARD No support added yet
        /*
        string level = App.Instance.currentLevel;

        if (GameServicesFacade.Instance.IsLoggedIn())
        {
            GameServicesFacade.Instance.PostScore(level, score);
            GameServicesFacade.Instance.PostScore("global", score);
        }

        SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state) {
            if (state == SocialManagerUtilities.ConnectionState.OK)
            {
                string activeSharkKey = App.Instance.PlayerProgress.GetActiveSharkKey();

                SharkProgress sharkProgress = App.Instance.PlayerProgress.GetSharkProgress(activeSharkKey);
                List<Equippable> equippables = sharkProgress.GetAllAttachedEquippables();
                Dictionary<string, int> pets = sharkProgress.GetAllAttachedPets();

                if (sharkProgress != null)
                {
                    List<object> equippableInfo = new List<object>();

                    foreach (Equippable equipabble in equippables)
                    {
                        equippableInfo.Add(new Dictionary<string, object>{
                            { "key", equipabble.Key },
                            { "level", equipabble.Level }
                        });
                    }

                    List<object> petInfo = new List<object>();

                    foreach (KeyValuePair<string,int> kvp in pets)
                    {
                        petInfo.Add(new Dictionary<string, object>{
                            { "key", kvp.Key },
                        });
                    }

                    Dictionary<string, object> sharkInfo = new Dictionary<string, object>
                    {
                        { "key",  activeSharkKey },
                        { "growth", sharkProgress.growthValue },
                        { "items", equippableInfo },
                        { "pets", petInfo }
                    };

					Dictionary<string, int> eventData = DailyEventsManager.Instance.GetEventScores();
					// let's wipe out the score if the user has been banned.
					if(SaveFacade.Instance.userSaveSystem.bannedFromLiveEvents || SaveFacade.Instance.userSaveSystem.isHacker || SaveFacade.Instance.userSaveSystem.isPirate || SaveFacade.Instance.userSaveSystem.isCheater)
					{
						eventData.Clear();
					}

					SocialFacade.Instance.GetProfileInfo(SocialManager.GetSelectedSocialNetwork(), (info) =>
					{
						string name = "";
						if(info != null)
						{
							info.TryGetValue("name", out name);
						}

						HSXServer.Instance.RecordUserScore(level, score, playTime, sharkInfo, eventData, name, delegate (bool success)
						{
							if(success)
							{
								Debug.Log("Leaderboards (RecordUserScore) :: Complete");
								// Report analytics for daily events
								if(eventData != null && eventData.Count > 0 && DailyEventsManager.Instance.activeEvent != null)
								{
									HSXAnalyticsManager.Instance.GameEventEntered(DailyEventsManager.Instance.activeEventTracker.category.ToString(), DailyEventsManager.Instance.activeEvent.eventKey);
								}
							}
							// notify the daily event tracker.
							DailyEventsManager.Instance.ScoreSubmitted(success);
						});
					});
                }
                else
                {
                    Debug.LogError("Leaderboards (RecordUserScore) :: Unable to get shark progress for key: " + activeSharkKey);
                }
            }
        });
        */
    }

    public void GetLeaderboard(string level, Action<SocialManagerUtilities.ConnectionState, LeaderboardEntry[]> onGetLeaderboard)
    {
        //[DGR] No support added yet
        /*
		//Delete the cache if we are not logged in and we have an existing cache already
		if (!CheckIfLoggedIn () && m_leaderboardsCache.Keys.Count > 0 )
		{
			m_leaderboardsCache.Clear ();
		}

		// download the leaderboard if there is no time stored for the selected level or the timer has expired.
		bool download = !m_leaderboardsCache.ContainsKey(level) || Time.realtimeSinceStartup - m_leaderboardsCache[level].lastUpdateTime > LeaderboardRequestTimeout;
		if(download)
		{
			// reset the cache.
			m_leaderboardsCache[level] = null;
			// ask the server.
			SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state)
			{
				if(state == SocialManagerUtilities.ConnectionState.OK)
				{
					SocialFacade.Network network = SocialManager.GetSelectedSocialNetwork();

					SocialFacade.Instance.GetFriends(network, delegate (Dictionary<string, string> friends)
					{
						string friendIDs = null;

						if(friends != null)
						{
							Dictionary<string, LeaderboardEntry> entries = new Dictionary<string, LeaderboardEntry>();

							foreach(KeyValuePair<string, string> pair in friends)
							{
								if(friendIDs != null)
								{
									friendIDs += ",";
								}

								string socialID = SocialManagerUtilities.GetPrefixedSocialID(pair.Key);

								friendIDs += socialID;

								LeaderboardEntry entry = new LeaderboardEntry();
								entry.socialID = pair.Key;
								entry.network = SocialManager.GetSelectedSocialNetwork();
								entry.name = pair.Value;
								entry.score = -1;

								entries.Add(socialID, entry);
							}

							SocialFacade.Instance.GetProfileInfo(network, delegate (Dictionary<string, string> profileInfo)
							{
								if(profileInfo != null && profileInfo.ContainsKey("name") && profileInfo.ContainsKey("id"))
								{
									string socialID = SocialManagerUtilities.GetPrefixedSocialID(profileInfo["id"]);

									LeaderboardEntry entry = new LeaderboardEntry();
									entry.socialID = profileInfo["id"];
									entry.network = SocialManager.GetSelectedSocialNetwork();
									entry.name = profileInfo["name"];

									if(level == "global")
									{
										entry.score = (int)App.Instance.PlayerProgress.highestScore;
									}
									else
									{
										entry.score = (int)App.Instance.PlayerProgress.GetLevelHighScore(level);
									}

									entries.Add(socialID, entry);

									if(friendIDs != null)
									{
										friendIDs += ",";
									}

									friendIDs += socialID;
								}

								HSXServer.Instance.GetLeaderboard(level, friendIDs, delegate (List<object> leaderboard)
								{
									if(leaderboard != null)
									{
										foreach(object rawEntry in leaderboard)
										{
											try
											{
												Dictionary<string, object> entryDic = rawEntry as Dictionary<string, object>;

												if(entryDic != null)
												{
													if(entryDic.ContainsKey("socialID"))
													{
														string socialID = entryDic["socialID"] as string;

														if(entries.ContainsKey(socialID) && entryDic.ContainsKey("fgolID") && entryDic.ContainsKey("timestamp") && entryDic.ContainsKey("playTime") && entryDic.ContainsKey("score"))
														{
															LeaderboardEntry entry = entries[socialID];

															entry.fgolID = entryDic["fgolID"] as string;
															entry.timestamp = Convert.ToInt32(entryDic["timestamp"]);
															entry.playTime = Convert.ToInt32(entryDic["playTime"]);

															int score = Convert.ToInt32(entryDic["score"]);

															if(score > entry.score)
															{
																entry.score = score;
															}

															entries[socialID] = entry;
														}
													}
												}
											}
											catch(Exception e)
											{
												Debug.LogWarning("Leaderboards (GetLeaderboard) :: Error parsing entry - " + e);
											}
										}

										List<LeaderboardEntry> validEntries = new List<LeaderboardEntry>();

										foreach(var entry in entries)
										{
											if(entry.Value.score >= 0)
											{
												validEntries.Add(entry.Value);
											}
										}

										LeaderboardEntry[] sortedEntries = validEntries.ToArray();
										Array.Sort<LeaderboardEntry>(sortedEntries, delegate (LeaderboardEntry a, LeaderboardEntry b)
										{
											return b.score - a.score;
										});
										m_leaderboardsCache[level] = new CacheItem<LeaderboardEntry[]>(sortedEntries, state, Time.realtimeSinceStartup);
										onGetLeaderboard(state, sortedEntries);
									}
									else
									{
										m_leaderboardsCache[level] = new CacheItem<LeaderboardEntry[]>(new LeaderboardEntry[0], state, Time.realtimeSinceStartup);
										onGetLeaderboard(state, m_leaderboardsCache[level].cache);
									}
								});
							});
						}
						else
						{
							m_leaderboardsCache[level] = new CacheItem<LeaderboardEntry[]>(new LeaderboardEntry[0], state, Time.realtimeSinceStartup);
							onGetLeaderboard(state, m_leaderboardsCache[level].cache);
						}
					});
				}
				else
				{
					m_leaderboardsCache[level] = new CacheItem<LeaderboardEntry[]>(null, state);
					onGetLeaderboard(state, null);
				}
			});
		}
		else
		{
			Debug.Log("Leaderboards::Using Leaderboard Cache");
			// use the cache !!
			onGetLeaderboard(m_leaderboardsCache[level].state, m_leaderboardsCache[level].cache);
		}*/
    }

	private bool CheckIfLoggedIn( )
	{
		return SocialManager.Instance.IsLoggedIn(SocialManager.GetSelectedSocialNetwork () );
	}

    public void GetFriendSharks(Action<SocialManagerUtilities.ConnectionState, SharkEntry[]> onGetFriendsSharks)
    {
        // [DGR] Not supported yet        
		//Delete the cache if we are not logged in and we have an existing cache already
		if (!CheckIfLoggedIn () && m_sharksCache != null)
		{
			m_sharksCache = null;
		}

        // [DGR] Not supported yet
        /*
        // download the sharks if there is no cache or the timer has expired.
        bool download = m_sharksCache == null || Time.realtimeSinceStartup - m_sharksCache.lastUpdateTime > LeaderboardRequestTimeout;        
		if(download)
		{
			// reset the cache.
			m_sharksCache = null;
			SocialManagerUtilities.CheckConnectionAuth(delegate (SocialManagerUtilities.ConnectionState state)
			{
				if(state == SocialManagerUtilities.ConnectionState.OK)
				{
					SocialFacade.Network network = SocialManager.GetSelectedSocialNetwork();

					SocialFacade.Instance.GetFriends(network, delegate (Dictionary<string, string> friends)
					{
						string friendIDs = null;
						Debug.LogWarning("You have " + friends.Count + " friends");
						if(friends != null)
						{
							Dictionary<string, SharkEntry> entries = new Dictionary<string, SharkEntry>();

							foreach(KeyValuePair<string, string> pair in friends)
							{
								if(friendIDs != null)
								{
									friendIDs += ",";
								}

								string socialID = SocialManagerUtilities.GetPrefixedSocialID(pair.Key);

								friendIDs += socialID;

								SharkEntry entry = new SharkEntry();
								entry.socialID = socialID;
								entry.network = network;
								entry.name = pair.Value;
								entry.shark = null;

								entries.Add(socialID, entry);
							}

							HSXServer.Instance.GetFriendSharks(friendIDs, delegate (List<object> friendsSharks)
							{
								if(friendsSharks != null)
								{
									foreach(object friendSharkObj in friendsSharks)
									{
										Dictionary<string, object> friendShark = friendSharkObj as Dictionary<string, object>;

										if(friendShark != null)
										{
											if(friendShark.ContainsKey("socialID"))
											{
												string socialID = friendShark["socialID"] as string;

												if(entries.ContainsKey(socialID) && friendShark.ContainsKey("fgolID") && friendShark.ContainsKey("lastLogin") && friendShark.ContainsKey("highScore") && friendShark.ContainsKey("highScorePlayTime") && friendShark.ContainsKey("numSharksOwned") && friendShark.ContainsKey("shark"))
												{
													try
													{
														SharkEntry entry = entries[socialID];

														Dictionary<string, object> sharkRecipe = friendShark["shark"] as Dictionary<string, object>;

														if(sharkRecipe != null)
														{
															if(sharkRecipe.ContainsKey("key") && sharkRecipe.ContainsKey("growth"))
															{
																SharkRecipe recipe = new SharkRecipe();
																recipe.key = sharkRecipe["key"] as string;
																recipe.growth = Convert.ToSingle(sharkRecipe["growth"]);
																recipe.lastUpdate = -1; //Default to negative time if we don't get anything from the server

																List<EquippableRecipe> items = new List<EquippableRecipe>();
																if(sharkRecipe.ContainsKey("items"))
																{

																	List<object> itemObjs = sharkRecipe["items"] as List<object>;

																	if(itemObjs != null)
																	{
																		foreach(object itemObj in itemObjs)
																		{
																			Dictionary<string, object> item = itemObj as Dictionary<string, object>;

																			EquippableRecipe itemRecipe = new EquippableRecipe();
																			itemRecipe.key = item["key"] as string;
																			itemRecipe.level = Convert.ToInt32(item["level"]);

																			items.Add(itemRecipe);
																		}
																	}
																}

																recipe.items = items;

																List<string> pets = new List<string>();
																if(sharkRecipe.ContainsKey("pets"))
																{

																	List<object> petsObjs = sharkRecipe["pets"] as List<object>;

																	if(petsObjs != null)
																	{
																		foreach(object petObj in petsObjs)
																		{
																			Dictionary<string, object> pet = petObj as Dictionary<string, object>;
																			string petKey = pet["key"] as string;
																			pets.Add(petKey);
																		}
																	}


																}

																if(friendShark.ContainsKey("sharkInfoLastUpdate"))
																{
																	try
																	{
																		recipe.lastUpdate = Convert.ToInt32(friendShark["sharkInfoLastUpdate"]);
																	}
																	catch(Exception) { }
																}

																recipe.pets = pets;
																entry.shark = recipe;
																entry.lastLogin = Convert.ToInt32(friendShark["lastLogin"]);
																entry.score = Convert.ToInt32(friendShark["highScore"]);
																entry.playTime = Convert.ToInt32(friendShark["highScorePlayTime"]);
																entry.numSharksOwned = Convert.ToInt32(friendShark["numSharksOwned"]);

																entries[socialID] = entry;
															}
														}
													}
													catch(Exception e)
													{
														Debug.LogWarning("Leaderboards (GetFriendSharks) :: Error parsing entry - " + e);
													}
												}
											}
										}
									}

									List<SharkEntry> validEntries = new List<SharkEntry>();

									foreach(var entry in entries)
									{
										if(entry.Value.shark != null)
										{
											validEntries.Add(entry.Value);
										}
									}

									SharkEntry[] sortedEntries = validEntries.ToArray();
									Array.Sort<SharkEntry>(sortedEntries, delegate (SharkEntry a, SharkEntry b)
									{
										return string.Compare(a.name, b.name);
									});
									m_sharksCache = new CacheItem<SharkEntry[]>(sortedEntries, state, Time.realtimeSinceStartup);
									onGetFriendsSharks(state, sortedEntries);
								}
								else
								{
									m_sharksCache = new CacheItem<SharkEntry[]>(new SharkEntry[0], state, Time.realtimeSinceStartup);
									onGetFriendsSharks(state, m_sharksCache.cache);
								}
							});
						}
						else
						{
							m_sharksCache = new CacheItem<SharkEntry[]>(new SharkEntry[0], state, Time.realtimeSinceStartup);
							onGetFriendsSharks(state, m_sharksCache.cache);
						}
					});
				}
				else
				{
					m_sharksCache = new CacheItem<SharkEntry[]>(null, state);
					onGetFriendsSharks(state, null);
				}
			});
		}
		else*/
        {
			Debug.Log("Leaderboards::Using Leaderboard Cache");
			// use the cache !!
			onGetFriendsSharks(m_sharksCache.state, m_sharksCache.cache);
		}
	}

	private void OnGoInGame(Enum m_event, object[] args)
	{
		ClearCache();
		GC.Collect();
	}

	public void ClearCache()
	{
		Debug.Log("Leaderboards::Clearing leaderboard Cache");
		m_leaderboardsCache.Clear();
		m_sharksCache = null;
	}
}