using UnityEngine;
using System.Collections;
using FGOL.Marketing;

public class MarketingCallbacks : MonoBehaviour
{
// [DGR] Not needed yet
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR && false

	void OnEnable()
	{
		UpsightManager.unlockedRewardEvent += OnRewardRecieved;
		UpsightManager.makePurchaseEvent += OnMakePurchaseEvent;
	}

	void OnDisable()
	{
		UpsightManager.unlockedRewardEvent -= OnRewardRecieved;
		UpsightManager.makePurchaseEvent -= OnMakePurchaseEvent;
	}

	void OnRewardRecieved(UpsightReward reward)
	{
		Debug.Log("Upsight reward recieved " + reward.name + " " + reward.quantity);

		MarketingReward marketingReward = new FGOL.Marketing.MarketingReward();
		marketingReward.Name = reward.name;
		marketingReward.Quantity = reward.quantity;

		MarketingFacade.Instance.TriggerEvent(marketingReward);
	}

	void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			//  Let Upsight knows that we have resumed our app
			Upsight.requestAppOpen();
		}
	}

	private void OnMakePurchaseEvent(UpsightPurchase purchase)
	{
		Debug.Log("Upsight Purchase Recieved JSON: with ID: " + purchase.productIdentifier + " title: " + purchase.title + " quantity: " + purchase.quantity + " placement: " + purchase.placement + " " + purchase.json);

		/*
		 * Example response:
		 * UpsightPurchaseEvent JSON: with ID: com.ubisoft.hungrysharkworld.coins1 title: Can full of gold quantity: 1 placement: shop_entry 
		 * {"placement":"shop_entry","cookie":"OWJhZjY5MDUwZDgzMTVkNzc1Mzk1ODBiMmExOTI1NTE6eyJpYXBfaWQiOiA1NjExMiwgImxhbmd1YWdlcyI6ICJlbiIsICJjb250ZW50X3R5cGVfaWQiOiA3LCAiY3JlYXRpdmVfaWQiOiAzNzU0OTIsICJjb250ZW50X2lkIjogMTcyNDI0NCwgImNhbXBhaWduX2lkIjogMjI1NTI0LCAicGxhY2VtZW50X3RhZyI6ICJzaG9wX2VudHJ5In06MjE1ODYyMjMwNw","title":"Can full of gold","price":null,"store":null,"productIdentifier":"com.ubisoft.hungrysharkworld.coins1","quantity":"1","receipt":"646309194"}
		*/

		MarketingPurchase mp = new MarketingPurchase();
		mp.Name = purchase.title;
		mp.Quantity = purchase.quantity;
		mp.ProductID = purchase.productIdentifier;

		MarketingFacade.Instance.TriggerIAPEvent(mp);
	}

#endif
}
