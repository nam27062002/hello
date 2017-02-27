using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class GameStoreManagerCalety : GameStoreManager
{

	#region listener
    /// Listener /////////////////////////////////////////////////////////////
	private class CaletyGameStoreListener : StoreManager.StoreListenerBase
    {
		public override void onPurchaseCompleted(string sku, JSONNode kReceiptJSON) 
		{
			Messenger.Broadcast<string>(EngineEvents.PURCHASE_SUCCESSFUL, PlatormSkuToGameSku( sku ) );
		}

		public override void onPurchaseCancelled(string sku, string strTransactionID) 
		{
			Messenger.Broadcast<string>(EngineEvents.PURCHASE_CANCELLED, PlatormSkuToGameSku( sku ) );
		}

		public override void onPurchaseFailed(string sku, string strTransactionID) 
		{
			Messenger.Broadcast<string>(EngineEvents.PURCHASE_FAILED, PlatormSkuToGameSku( sku ) );
		}

		public override void onStoreIsReady() 
		{
			
		}

		private string PlatormSkuToGameSku( string platform_sku )
	    {
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable( DefinitionsCategory.SHOP_PACKS, GameStoreManagerCalety.GetPlatformAttribute(), platform_sku);
			return def.sku;
	    }
    }
	#endregion

	const string IOS_ATTRIBUTE = "ios";
	const string ANDROID_ATTRIBUTE = "android";

	CaletyGameStoreListener m_storeListener;
	string[] m_storeSkus;



	public GameStoreManagerCalety () 
	{
		m_storeListener = new CaletyGameStoreListener();
		StoreManager.SharedInstance.AddListener (m_storeListener);
		CacheStoreSkus();	    
		StoreManager.SharedInstance.Initialise (ref m_storeSkus);
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
			if ( outList[i].GetAsFloat("priceDollars") > 0 )
			{
				skus.Add( outList[i].Get( GetPlatformAttribute()));
			}
		}
		m_storeSkus = skus.ToArray();
	}

	public override bool CanMakePayment()
	{
		return StoreManager.SharedInstance.CanMakePayments();
	}
    


	public override void Buy( string _sku )
	{
    	if (StoreManager.SharedInstance.CanMakePayments()) 
    	{
    		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, _sku);
    		if ( def != null )
    		{
    			string item = def.Get( GetPlatformAttribute() );
				StoreManager.SharedInstance.RequestProduct (item);
    		}
    	}
    }

    private static string GetPlatformAttribute()
    {
		#if UNITY_IOS
			return IOS_ATTRIBUTE;
		#elif UNITY_ANDROID
			return ANDROID_ATTRIBUTE;
		#endif
    	return "";
    }

  
}
