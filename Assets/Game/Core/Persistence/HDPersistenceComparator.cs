using System;
using System.Collections.Generic;
using UnityEngine;
public class HDPersistenceComparator : PersistenceComparator
{
    private PersistenceComparatorSystem m_localProgress = null;
    private PersistenceComparatorSystem m_cloudProgress = null;

    public override void Reset()
    {
        if (m_localProgress != null) {
            m_localProgress.Reset();
        }

        if (m_cloudProgress != null) {
            m_cloudProgress.Reset();
        }
    }

    public override PersistenceStates.EConflictState Compare(PersistenceData local, PersistenceData cloud)
    {
        m_localProgress = null;
        m_cloudProgress = null;

        PersistenceStates.EConflictState state = PersistenceStates.EConflictState.Equal;

        if (local != null)
        {
            PersistenceComparatorSystem localProgress = new PersistenceComparatorSystem();
            localProgress.data = local;

            try
            {
                localProgress.Load();
                localProgress.lastModified = local.Timestamp;
                localProgress.lastDevice = local.DeviceName;

                m_localProgress = localProgress;
            }
            catch (Exception) { }
        }

        if (cloud != null)
        {
            PersistenceComparatorSystem cloudProgress = new PersistenceComparatorSystem();
            cloudProgress.data = cloud;

            try
            {
                cloudProgress.Load();
                cloudProgress.lastModified = cloud.Timestamp;
                cloudProgress.lastDevice = cloud.DeviceName;

                m_cloudProgress = cloudProgress;
            }
            catch (Exception) { }
        }

        if (m_localProgress != null)
        {
            if (m_cloudProgress != null)
            {
                Debug.Log("HDPersistenceComparator (CompareSaves) :: Local Save");
                Debug.Log("dragonsOwned: " + m_localProgress.dragonsOwned);
                Debug.Log("eggsCollected: " + m_localProgress.eggsCollected);
                Debug.Log("timePlayed: " + m_localProgress.timePlayed);
                Debug.Log("iapPurchaseMade: " + m_localProgress.iapPurchaseMade);
                Debug.Log("timestamp: " + local.Timestamp);

                Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud Save");
                Debug.Log("dragonsOwned: " + m_cloudProgress.dragonsOwned);
                Debug.Log("eggsCollected: " + m_cloudProgress.eggsCollected);
                Debug.Log("timePlayed: " + m_cloudProgress.timePlayed);
                Debug.Log("iapPurchaseMade: " + m_cloudProgress.iapPurchaseMade);
                Debug.Log("timestamp: " + cloud.Timestamp);

                //Check local save progress and automatically replace with cloud if its a brand new save!
                if (m_localProgress.dragonsOwned <= 1 && m_localProgress.eggsCollected == 0 && m_cloudProgress.timePlayed == 0 && !m_localProgress.iapPurchaseMade)
                {
                    Debug.Log("HDPersistenceComparator (CompareSaves) :: Brand New Save UseLoad");
                    state = PersistenceStates.EConflictState.UseLocal;
                }
                //If dragons and the amount of eggs collected are equal
                else if (m_localProgress.dragonsOwned == m_cloudProgress.dragonsOwned && m_localProgress.eggsCollected == m_cloudProgress.eggsCollected)
                {
                    if (m_localProgress.timePlayed == m_cloudProgress.timePlayed)
                    {
                        if (local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local time stamp greater UseLocal");
                            state = PersistenceStates.EConflictState.UseLocal;
                        }
                        else if (local.Timestamp < cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud time stamp greater RecommendCloud");
                            state = PersistenceStates.EConflictState.RecommendCloud;
                        }
                    }
                    else if (m_localProgress.timePlayed > m_cloudProgress.timePlayed)
                    {
                        if (local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local time played greater UseLocal");
                            state = PersistenceStates.EConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local time played greater but cloud timestamp newer RecommendLocal");
                            state = PersistenceStates.EConflictState.RecommendLocal;
                        }
                    }
                    else if (m_localProgress.timePlayed < m_cloudProgress.timePlayed)
                    {
                        if (local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud time played greater UseCloud");
                            state = PersistenceStates.EConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local time played greater RecommendCloud");
                            state = PersistenceStates.EConflictState.RecommendCloud;
                        }
                    }
                }
                //If just the dragons are equal
                else if (m_localProgress.dragonsOwned == m_cloudProgress.dragonsOwned)
                {
                    if (m_localProgress.eggsCollected > m_cloudProgress.eggsCollected)
                    {
                        if (m_localProgress.timePlayed >= m_cloudProgress.timePlayed && local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local Eggs Collected greater UseLocal");
                            state = PersistenceStates.EConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local Eggs Collected greater but time player or timestamp less RecommendLocal");
                            state = PersistenceStates.EConflictState.RecommendLocal;
                        }
                    }
                    else
                    {
                        if (m_localProgress.timePlayed <= m_cloudProgress.timePlayed && local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud Eggs Collected greater UseCloud");
                            state = PersistenceStates.EConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud Eggs Collected greater but time player or timestamp less RecommendCloud");
                            state = PersistenceStates.EConflictState.RecommendCloud;
                        }
                    }
                }
                //If just the Eggs Collected are equal
                else if (m_localProgress.eggsCollected == m_cloudProgress.eggsCollected)
                {
                    if (m_localProgress.dragonsOwned > m_cloudProgress.dragonsOwned)
                    {
                        if (m_localProgress.timePlayed >= m_cloudProgress.timePlayed && local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local dragons greater UseLocal");
                            state = PersistenceStates.EConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local dragons greater but time player or timestamp less RecommendLocal");
                            state = PersistenceStates.EConflictState.RecommendLocal;
                        }
                    }
                    else
                    {
                        if (m_localProgress.timePlayed <= m_cloudProgress.timePlayed && local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud dragons greater UseCloud");
                            state = PersistenceStates.EConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud dragons greater but time player or timestamp less RecommendCloud");
                            state = PersistenceStates.EConflictState.RecommendCloud;
                        }
                    }
                }
                //If the local dragons are greater
                else if (m_localProgress.dragonsOwned > m_cloudProgress.dragonsOwned)
                {
                    if (m_localProgress.eggsCollected > m_cloudProgress.eggsCollected)
                    {
                        if (m_localProgress.timePlayed >= m_cloudProgress.timePlayed && local.Timestamp >= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local dragons and Eggs Collected greater UseLocal");
                            state = PersistenceStates.EConflictState.UseLocal;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local dragons and Eggs Collected greater but time player or timestamp less RecommendLocal");
                            state = PersistenceStates.EConflictState.RecommendLocal;
                        }
                    }
                    else
                    {
                        Debug.Log("HDPersistenceComparator (CompareSaves) :: Local dragons greater but Eggs Collected less UserDecision");
                        state = PersistenceStates.EConflictState.UserDecision;
                    }
                }
                //If the local dragons are less
                else if (m_localProgress.dragonsOwned < m_cloudProgress.dragonsOwned)
                {
                    if (m_localProgress.eggsCollected < m_cloudProgress.eggsCollected)
                    {
                        if (m_localProgress.timePlayed <= m_cloudProgress.timePlayed && local.Timestamp <= cloud.Timestamp)
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud dragons and Eggs Collected greater UseCloud");
                            state = PersistenceStates.EConflictState.UseCloud;
                        }
                        else
                        {
                            Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud dragons and Eggs Collected greater but time player or timestamp less RecommendCloud");
                            state = PersistenceStates.EConflictState.RecommendCloud;
                        }
                    }
                    else
                    {
                        Debug.Log("HDPersistenceComparator (CompareSaves) :: cloud dragons greater but Eggs Collected less UserDecision");
                        state = PersistenceStates.EConflictState.UserDecision;
                    }
                }
            }
            else
            {
                Debug.Log("HDPersistenceComparator (CompareSaves) :: Cloud save unavailable UseLocal");
                state = PersistenceStates.EConflictState.UseLocal;
            }
        }
        else if (m_cloudProgress != null)
        {
            Debug.Log("HDPersistenceComparator (CompareSaves) :: Local save unavailable UseCloud");
            state = PersistenceStates.EConflictState.UseCloud;
        }

        return state;
    }

    public override void ReconcileData(PersistenceData local, PersistenceData cloud)
    {
		// [DGR] TODO
		/*
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
        */
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
	// [DGR] TODO
   /*
	private void ParsePurchases(Dictionary<string, object> raw, Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> parsed)
    {
        if (raw != null)
        {
            var iterator = raw.GetEnumerator();

            while (iterator.MoveNext())
            {
                if (!parsed.ContainsKey(iterator.Current.Key))
                {
                    parsed.Add(iterator.Current.Key, new IAPSaveSystem.ReconcilablePurchaseData(iterator.Current.Value));
                }
            }
        }
    }
    */

    // Copy IAP purchases from one dictionary to another and save the changes in a save file
	// [DGR] TODO
    /*
	private void CopyIAPPurchases(Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> source, Dictionary<string, IAPSaveSystem.ReconcilablePurchaseData> target, PersistenceData targetSave)
    {
        var iterator = source.GetEnumerator();

        while (iterator.MoveNext())
        {
            // Conditions to copy IAP are:
            // 1. An item with the same receipt ID doesn't exist 
            // 2. The item platform is the same as current platform
            if (!target.ContainsKey(iterator.Current.Key) && (iterator.Current.Value.m_platform == Globals.GetPlatform().ToString()))
            {
                string key = "";

                // Save purchased data
                // Coins
                try
                {
                    key = string.Format("Bank.{0}.PurchasedCoins", Globals.GetPlatform().ToString());
                    targetSave[key] = System.Convert.ToInt32(targetSave[key]) + iterator.Current.Value.m_coins;
                }
                catch (Exception e)
                {
                    Debug.Log("HDPersistenceComparator :: Exception copying purchased coins between save files :: Exception = " + e);
                }

                // Gems
                try
                {
                    key = string.Format("Bank.{0}.PurchasedGems", Globals.GetPlatform().ToString());
                    targetSave[key] = System.Convert.ToInt32(targetSave[key]) + iterator.Current.Value.m_gems;
                }
                catch (Exception e)
                {
                    Debug.Log("HDPersistenceComparator :: Exception copying purchased gems between save files :: Exception = " + e);
                }

                target.Add(iterator.Current.Key, iterator.Current.Value);
            }
        }

        // Convert target dictionary into string, object
        Dictionary<string, object> purchasesDictionary = new Dictionary<string, object>();
        iterator = target.GetEnumerator();
        while (iterator.MoveNext())
        {
            purchasesDictionary.Add(iterator.Current.Key, iterator.Current.Value.Serialize());
        }

        // Save the raw purchases dictionary as purchases in the save
        targetSave.Purchases = purchasesDictionary;
    }
    */
}