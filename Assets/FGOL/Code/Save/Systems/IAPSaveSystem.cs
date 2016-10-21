using UnityEngine;
using System.Collections;
using FGOL.Save;
using System.Collections.Generic;
using System;

// Saves all iap purchases
public class IAPSaveSystem
{
	public struct ReconcilablePurchaseData
	{
		public int		m_coins;
		public int		m_gems;
		public string[] m_items;
		public string	m_platform;

		public ReconcilablePurchaseData(int coins, int gems, string[] items)
		{
			m_coins = coins;
			m_gems = gems;

			// Item ID's
			m_items = items;

			m_platform = Globals.GetPlatform().ToString();
        }

		public ReconcilablePurchaseData(object rawObj)
		{
			Dictionary<string, object> dic = rawObj as Dictionary<string, object>;

			object data;
			
			// Set all data to default values as some values may not exist in different versions of save data
			m_coins = 0;
			m_gems = 0;
			m_platform = "";
			m_items = null;

			if(dic.TryGetValue("Coins", out data))
			{
				m_coins = Convert.ToInt32(data);
			}

			if(dic.TryGetValue("Gems", out data))
			{
				m_gems = Convert.ToInt32(data);
			}

			if(dic.TryGetValue("Platform", out data))
			{
				m_platform = Convert.ToString(data);
			}

			if(dic.TryGetValue("Items", out data))
			{
				List<object> list = data as List<object>;

				if(list != null)
				{
					m_items = new string[list.Count];

					for(int i = 0;i < m_items.Length;i++)
					{
						m_items[i] = Convert.ToString(list[i]);
					}
				}
			}
		}

		public object Serialize()
		{
			Dictionary<string, object> dic = new Dictionary<string, object>();

			dic.Add("Coins", m_coins);
			dic.Add("Gems", m_gems);
			dic.Add("Platform", m_platform);
			dic.Add("Items", m_items);

			return dic;
        }
	}

	public static void AddTransaction(string transactionID, int coins, int gems, string[] items)
	{
		// Read all purchases from file
		Dictionary<string, object> purchasesRaw = SaveGameManager.Instance.SaveData.Purchases as Dictionary<string, object>;

		// Create dictionary if its first purchase
		if(purchasesRaw == null)
		{
			purchasesRaw = new Dictionary<string, object>();
		}

		// Add the new transaction if it doesn't exist
		if(!string.IsNullOrEmpty(transactionID) && !purchasesRaw.ContainsKey(transactionID))
		{
			purchasesRaw.Add(transactionID, new ReconcilablePurchaseData(coins, gems, items).Serialize());
		}

		// Write transactions back to save data
		SaveGameManager.Instance.SaveData.Purchases = purchasesRaw;

		// Force save to disk/cloud
		SaveFacade.Instance.Save();
    }
}
