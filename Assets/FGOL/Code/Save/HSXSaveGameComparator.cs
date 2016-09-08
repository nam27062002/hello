using FGOL.Save;
using FGOL.Save.SaveStates;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HSXSaveGameComparator : SaveGameComparator
{
    private ProgressComparatorSystem m_localProgress = null;
    private ProgressComparatorSystem m_cloudProgress = null;

    public override ConflictState CompareSaves(SaveData local, SaveData cloud)
    {
        m_localProgress = null;
        m_cloudProgress = null;

        ConflictState state = ConflictState.Equal;

        if(local != null)
        {
            ProgressComparatorSystem localProgress = new ProgressComparatorSystem();
            localProgress.data = local;

            try
            {
                localProgress.Load();
                localProgress.lastModified = local.Timestamp;
                localProgress.lastDevice = local.DeviceName;

                m_localProgress = localProgress;
            }
            catch(Exception) { }
        }

        if(cloud != null)
        {
            ProgressComparatorSystem cloudProgress = new ProgressComparatorSystem();
            cloudProgress.data = cloud;

            try
            {
                cloudProgress.Load();
                cloudProgress.lastModified = cloud.Timestamp;
                cloudProgress.lastDevice = cloud.DeviceName;

                m_cloudProgress = cloudProgress;
            }
            catch(Exception) { }
        }

        if(m_localProgress != null)
        {
            if(m_cloudProgress != null)
            {
                Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local Save");
                Debug.Log("dragonsOwned: " + m_localProgress.dragonsOwned);
                Debug.Log("missionsCompleted: " + m_localProgress.missionsCompleted);
                Debug.Log("timePlayed: " + m_localProgress.timePlayed);
                Debug.Log("iapPurchaseMade: " + m_localProgress.iapPurchaseMade);
                Debug.Log("timestamp: " + local.Timestamp);

                Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud Save");
                Debug.Log("dragonsOwned: " + m_cloudProgress.dragonsOwned);
                Debug.Log("missionsCompleted: " + m_cloudProgress.missionsCompleted);
                Debug.Log("timePlayed: " + m_cloudProgress.timePlayed);
                Debug.Log("iapPurchaseMade: " + m_cloudProgress.iapPurchaseMade);
                Debug.Log("timestamp: " + cloud.Timestamp);

                //Check local save progress and automatically replace with cloud if its a brand new save!
                if (m_localProgress.dragonsOwned <= 1 && m_localProgress.missionsCompleted == 0 && m_cloudProgress.timePlayed == 0 && !m_localProgress.iapPurchaseMade)
                {
                    Debug.Log("HSXSaveGameComparator (CompareSaves) :: Brand New Save UseCloud");

                    // [DGR] SERVER: When the server sends the default persistence then we'll be able to use UseCloud again
                    //state = ConflictState.UseCloud;
                    state = ConflictState.UseLocal;
                }
                //If dragons and mission are equal
                else if (m_localProgress.dragonsOwned == m_cloudProgress.dragonsOwned && m_localProgress.missionsCompleted == m_cloudProgress.missionsCompleted)
                {
                    if (m_localProgress.timePlayed == m_cloudProgress.timePlayed)
                    {
                        if (local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local time stamp greater UseLocal");
                            state = ConflictState.UseLocal;
                        }
                        else if (local.Timestamp < cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud time stamp greater RecommendCloud");
                            state = ConflictState.RecommendCloud;
                        }
                    }
                    else if (m_localProgress.timePlayed > m_cloudProgress.timePlayed)
                    {
                        if (local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local time played greater UseLocal");
                            state = ConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local time played greater but cloud timestamp newer RecommendLocal");
                            state = ConflictState.RecommendLocal;
                        }
                    }
                    else if (m_localProgress.timePlayed < m_cloudProgress.timePlayed)
                    {
                        if (local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud time played greater UseCloud");
                            state = ConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local time played greater RecommendCloud");
                            state = ConflictState.RecommendCloud;
                        }
                    }
                }
                //If just the dragons are equal
                else if (m_localProgress.dragonsOwned == m_cloudProgress.dragonsOwned)
                {
                    if (m_localProgress.missionsCompleted > m_cloudProgress.missionsCompleted)
                    {
                        if (m_localProgress.timePlayed >= m_cloudProgress.timePlayed && local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local Missions greater UseLocal");
                            state = ConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local Missions greater but time player or timestamp less RecommendLocal");
                            state = ConflictState.RecommendLocal;
                        }
                    }
                    else
                    {
                        if (m_localProgress.timePlayed <= m_cloudProgress.timePlayed && local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud Missions greater UseCloud");
                            state = ConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud Missions greater but time player or timestamp less RecommendCloud");
                            state = ConflictState.RecommendCloud;
                        }
                    }
                }
                //If just the missions are equal
                else if (m_localProgress.missionsCompleted == m_cloudProgress.missionsCompleted)
                {
                    if (m_localProgress.dragonsOwned > m_cloudProgress.dragonsOwned)
                    {
                        if (m_localProgress.timePlayed >= m_cloudProgress.timePlayed && local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local dragons greater UseLocal");
                            state = ConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local dragons greater but time player or timestamp less RecommendLocal");
                            state = ConflictState.RecommendLocal;
                        }
                    }
                    else
                    {
                        if (m_localProgress.timePlayed <= m_cloudProgress.timePlayed && local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud dragons greater UseCloud");
                            state = ConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud dragons greater but time player or timestamp less RecommendCloud");
                            state = ConflictState.RecommendCloud;
                        }
                    }
                }
                //If the local dragons are greater
                else if (m_localProgress.dragonsOwned > m_cloudProgress.dragonsOwned)
                {
                    if (m_localProgress.missionsCompleted > m_cloudProgress.missionsCompleted)
                    {
                        if (m_localProgress.timePlayed >= m_cloudProgress.timePlayed && local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local dragons and mission greater UseLocal");
                            state = ConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local dragons and mission greater but time player or timestamp less RecommendLocal");
                            state = ConflictState.RecommendLocal;
                        }
                    }
                    else
                    {
                        Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local dragons greater but missions less UserDecision");
                        state = ConflictState.UserDecision;
                    }
                }
                //If the local dragons are less
                else if (m_localProgress.dragonsOwned < m_cloudProgress.dragonsOwned)
                {
                    if (m_localProgress.missionsCompleted < m_cloudProgress.missionsCompleted)
                    {
                        if (m_localProgress.timePlayed <= m_cloudProgress.timePlayed && local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud dragons and mission greater UseCloud");
                            state = ConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud dragons and mission greater but time player or timestamp less RecommendCloud");
                            state = ConflictState.RecommendCloud;
                        }
                    }
                    else
                    {
                        Debug.Log("HSXSaveGameComparator (CompareSaves) :: cloud dragons greater but missions less UserDecision");
                        state = ConflictState.UserDecision;
                    }
                }
            }
            else
            {
                Debug.Log("HSXSaveGameComparator (CompareSaves) :: Cloud save unavailable UseLocal");
                state = ConflictState.UseLocal;
            }
        }
        else if(m_cloudProgress != null)
        {
            Debug.Log("HSXSaveGameComparator (CompareSaves) :: Local save unavailable UseCloud");
            state = ConflictState.UseCloud;
        }

        return state;
    }

	public override void ReconcileData(SaveData local, SaveData cloud)
	{
		// Prepare local purchases dictionary
		Dictionary<string, object> localPurchasesRaw = local.Purchases as Dictionary<string, object>;
		Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> localPurchases = new Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData>();
		ParsePurchases(localPurchasesRaw, localPurchases);

		// Prepare cloud purchases dictionary
		Dictionary<string, object> cloudPurchasesRaw = cloud.Purchases as Dictionary<string, object>;
		Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> cloudPurchases = new Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData>();
		ParsePurchases(cloudPurchasesRaw, cloudPurchases);

		// Detect difference between keys in dictionaries
		CopyIAPPurchases(localPurchases, cloudPurchases, cloud);
		CopyIAPPurchases(cloudPurchases, localPurchases, local);
	}

    public override object GetLocalProgress()
    {
        return m_localProgress;
    }

    public override object GetCloudProgress()
    {
        return m_cloudProgress;
    }

	// Parse IAP data from raw dictionary and copy into parsed dictionary
	private void ParsePurchases(Dictionary<string, object> raw, Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> parsed)
	{
		if(raw != null)
		{
			var iterator = raw.GetEnumerator();

			while(iterator.MoveNext())
			{
				if(!parsed.ContainsKey(iterator.Current.Key))
				{
					parsed.Add(iterator.Current.Key, new IAPSaveSystem.ReconcilablePurchaseData(iterator.Current.Value));
				}
			}
		}
	}

	// Copy IAP purchases from one dictionary to another and save the changes in a save file
	private void CopyIAPPurchases(Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> source, Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> target, SaveData targetSave)
	{
		var iterator = source.GetEnumerator();

		while(iterator.MoveNext())
		{
			// Conditions to copy IAP are:
			// 1. An item with the same receipt ID doesn't exist 
			// 2. The item platform is the same as current platform
			if(!target.ContainsKey(iterator.Current.Key) && (iterator.Current.Value.m_platform == Globals.GetPlatform().ToString()))
			{
				string key = "";

				// Save purchased data
				// Coins
				try
				{
					key = string.Format("Bank.{0}.PurchasedCoins", Globals.GetPlatform().ToString());
					targetSave[key] = System.Convert.ToInt32(targetSave[key]) + iterator.Current.Value.m_coins;
				}
				catch(Exception e)
				{
					Debug.Log("HSXSaveGameComparator :: Exception copying purchased coins between save files :: Exception = " + e);
				}

				// Gems
				try
				{
					key = string.Format("Bank.{0}.PurchasedGems", Globals.GetPlatform().ToString());
					targetSave[key] = System.Convert.ToInt32(targetSave[key]) + iterator.Current.Value.m_gems;
				}
				catch(Exception e)
				{
					Debug.Log("HSXSaveGameComparator :: Exception copying purchased gems between save files :: Exception = " + e);
				}

				target.Add(iterator.Current.Key, iterator.Current.Value);
            }	
		}

		// Convert target dictionary into string, object
		Dictionary<string, object> purchasesDictionary = new Dictionary<string, object>();
		iterator = target.GetEnumerator();
		while(iterator.MoveNext())
		{
			purchasesDictionary.Add(iterator.Current.Key, iterator.Current.Value.Serialize());
		}

		// Save the raw purchases dictionary as purchases in the save
		targetSave.Purchases = purchasesDictionary;
	}
}