﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class GameStoreManagerCalety : GameStoreManager
{

	#region listener
    /// Listener /////////////////////////////////////////////////////////////
	private class CaletyGameStoreListener : StoreManager.StoreListenerBase
    {
    	public bool m_isReady = false;
		public override void onPurchaseCompleted(string sku, string strTransactionID, JSONNode kReceiptJSON, string strPlatformOrderID) 
		{
			Debug.Log("onPurchaseCompleted");
			// string gameSku = PlatformSkuToGameSku( sku );
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition( DefinitionsCategory.SHOP_PACKS, sku);
			if ( def != null )
			{
				PopupCurrencyShopPill.ApplyShopPack( def );	
			}
			Messenger.Broadcast<string, string, JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, sku, strTransactionID, kReceiptJSON);
		}

		public override void onPurchaseCancelled(string sku, string strTransactionID) 
		{
			Debug.Log("onPurchaseCancelled");
			Messenger.Broadcast<string>(MessengerEvents.PURCHASE_CANCELLED, sku);
		}

		public override void onPurchaseFailed(string sku, string strTransactionID) 
		{
			Debug.Log("onPurchaseFailed");
			Messenger.Broadcast<string>(MessengerEvents.PURCHASE_FAILED, sku);
		}

		public override void onStoreIsReady() 
		{
			Debug.Log("onStoreIsReady");
			m_isReady = true;	
		}
		/*
		private string PlatformSkuToGameSku( string platform_sku )
	    {
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable( DefinitionsCategory.SHOP_PACKS, GameStoreManagerCalety.GetPlatformAttribute(), platform_sku);
			return def.sku;
	    }
	    */
    }
	#endregion

	const string IOS_ATTRIBUTE = "apple";
	const string GOOGLE_ATTRIBUTE = "google";
	const string AMAZON_ATTRIBUTE = "amazon";

	CaletyGameStoreListener m_storeListener;
	string[] m_storeSkus;

	public GameStoreManagerCalety () 
	{
		m_storeListener = new CaletyGameStoreListener();
	}

	public override void Initialize()
	{
		StoreManager.SharedInstance.AddListener (m_storeListener);
		CacheStoreSkus();	    
		StoreManager.SharedInstance.Initialise (ref m_storeSkus, false);
		#if UNITY_ANDROID && !UNITY_EDITOR
	        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
	        if(settingsInstance != null)
	        {
	            if (settingsInstance.m_iAndroidMarketSelected == CaletyConstants.MARKET_GOOGLE_PLAY)
	            {
	                StoreManager.SharedInstance.onReceivedPublicKey(settingsInstance.m_strAndroidPublicKeysGoogle[settingsInstance.m_iBuildEnvironmentSelected]);
	            }
	            else if (settingsInstance.m_iAndroidMarketSelected == CaletyConstants.MARKET_AMAZON)
	            {
	                StoreManager.SharedInstance.onReceivedPublicKey(settingsInstance.m_strAndroidPublicKeysAmazon[settingsInstance.m_iBuildEnvironmentSelected]);
	            }
	        }
		#endif
	}

	public void CacheStoreSkus()
	{
		m_storeSkus = null;
		List<DefinitionNode> outList = new List<DefinitionNode>();
		List<string> skus = new List<string>();;
		DefinitionsManager.SharedInstance.GetDefinitions( DefinitionsCategory.SHOP_PACKS, ref outList);
		for( int i = 0; i<outList.Count; i++ )
		{  
            /*          
            // We used to use a different field for the name of the product in every platform
            string platformId = outList[i].Get( GetPlatformAttribute());
			if ( !string.IsNullOrEmpty(platformId) )
			{
				skus.Add( platformId );
			}
            */
         
            // Sku is used directly as product id
            skus.Add(outList[i].sku);
        }
		m_storeSkus = skus.ToArray();
	}


	public override bool IsReady()
	{
		return m_storeListener.m_isReady;
	}

	public override string GetLocalisedPrice( string sku )
	{
		// string item = GameSkuToPlatformSku( sku );
		StoreManager.StoreProduct product = StoreManager.SharedInstance.GetStoreProduct( sku );
		if ( product != null )
		{            
			return product.m_strLocalisedPrice;
		}
		return "";
	}

    public override StoreManager.StoreProduct GetStoreProduct( string sku )
    {
        // string item = GameSkuToPlatformSku(sku);
        return StoreManager.SharedInstance.GetStoreProduct(sku);
    }


    public override bool CanMakePayment()
	{
#if UNITY_EDITOR
		return true;
#else
		return StoreManager.SharedInstance.CanMakePayments();
#endif	
	}
    


	public override void Buy( string _sku )
	{
#if UNITY_EDITOR
		StoreManager.SharedInstance.StartCoroutine( SimulatePurchase(_sku) );
#else
    	if (StoreManager.SharedInstance.CanMakePayments()) 
    	{
    		// string item = GameSkuToPlatformSku( _sku );
			if ( !string.IsNullOrEmpty( _sku ) )
    		{
                HDTrackingManager.Instance.Notify_IAPStarted();
				StoreManager.SharedInstance.RequestProduct (_sku);
    		}
    	}
#endif
    }		   

    IEnumerator SimulatePurchase( string _sku)
    {
		yield return new WaitForSecondsRealtime( 0.25f );
		// string item = GameSkuToPlatformSku( _sku );
		m_storeListener.onPurchaseCompleted( _sku, "", null, "");
    }
    /*
    private string GameSkuToPlatformSku( string gameSku )
    {
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, gameSku);
		if ( def != null )
		{
			return def.Get( GetPlatformAttribute() );

		}
		return "";
    }
    */

    private static string GetPlatformAttribute()
    {
		#if UNITY_IOS
			return IOS_ATTRIBUTE;
		#elif UNITY_ANDROID
			CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
	        if(settingsInstance != null)
	        {
	            if (settingsInstance.m_iAndroidMarketSelected == CaletyConstants.MARKET_GOOGLE_PLAY)
	            {
					return GOOGLE_ATTRIBUTE;
	            }
	            else if (settingsInstance.m_iAndroidMarketSelected == CaletyConstants.MARKET_AMAZON)
	            {
					return AMAZON_ATTRIBUTE;
	            }
	        }
			return GOOGLE_ATTRIBUTE;
		#endif
    	return "";
    }

  
}
