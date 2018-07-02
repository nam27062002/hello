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
    	public bool m_isReady = false;
        private bool m_hasInitFailed = false;

        public CaletyGameStoreListener() : base()
        {
            Reset();
        }

        public void Reset()
        {
            m_isReady = false;
            m_hasInitFailed = false;
        }

		public override void onPurchaseCompleted(string sku, string strTransactionID, JSONNode kReceiptJSON, string strPlatformOrderID) 
		{
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                string msg = "onPurchaseCompleted sku = " + sku + " strTransactionID = " + strTransactionID + " strPlatformOrderID = " + strPlatformOrderID + " receipt = ";
                if (kReceiptJSON == null)
                {
                    msg += "null";
                }
                else
                {
                    msg +=  kReceiptJSON.ToString();
                }

                ControlPanel.Log(msg, ControlPanel.ELogChannel.Store);
            }

            bool needsServerConfirmation = FeatureSettingsManager.instance.NeedPendingTransactionsServerConfirm();
            System.Action onDone = delegate()
            {
                // string gameSku = PlatformSkuToGameSku( sku );
                Messenger.Broadcast<string, string, JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, sku, strTransactionID, kReceiptJSON);
            };

            System.Action<bool> onConfirmDone = delegate(bool success)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    ControlPanel.Log("Server confirmation for purchase " + sku + " received with success = " + success, ControlPanel.ELogChannel.Store);
                }

                if (needsServerConfirmation)
                {
                    onDone();
                }
            };

            Transaction transaction = new Transaction();
            transaction.SetId(strPlatformOrderID);
            transaction.SetSource("shop");
            TransactionManager.instance.Pending_ConfirmTransactionWithServer(transaction, onConfirmDone);                        

            if (!needsServerConfirmation)
            {                                                    
                onDone();
            }            
		}       

        public override void onPurchaseCancelled(string sku, string strTransactionID) 
		{
            if (FeatureSettingsManager.IsDebugEnabled)
                Debug.Log("onPurchaseCancelled");

			Messenger.Broadcast<string>(MessengerEvents.PURCHASE_CANCELLED, sku);
		}

		public override void onPurchaseFailed(string sku, string strTransactionID) 
		{
            if (FeatureSettingsManager.IsDebugEnabled)
				Debug.Log("onPurchaseFailed sku = " + sku + " transactionID = " + strTransactionID);

			Messenger.Broadcast<string>(MessengerEvents.PURCHASE_FAILED, sku);
		}

		public override void onStoreIsReady() 
		{
            if (FeatureSettingsManager.IsDebugEnabled)
                Debug.Log("onStoreIsReady");

			m_isReady = true;	
		}

		/// <summary>
		// TODO: TEST!!!!!
		/// Ons the IAP promoted received. 
		/// </summary>
		/// <param name="strSku">String sku Product bought on the store.</param>
		public override void onIAPPromotedReceived (string strSku) 
		{
			// Check if this sku is valid. Is it a one time purchase?
			// if the user cannot purchase -> show message: You already have this item
			// it the user can purchase -> GameStoreManager.SharedInstance.Buy(strSku)
		}

        /*
		private string PlatformSkuToGameSku( string platform_sku )
	    {
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable( DefinitionsCategory.SHOP_PACKS, GameStoreManagerCalety.GetPlatformAttribute(), platform_sku);
			return def.sku;
	    }
	    */

        public override void onStoreIosInitFail(int errorCode)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Debug.Log("onStoreIosInitFail errorCode = " + errorCode);

            m_hasInitFailed = true;
        }

        public bool HasInitFailed()
        {
            return m_hasInitFailed;
        }
    }
	#endregion

	const string IOS_ATTRIBUTE = "apple";
	const string GOOGLE_ATTRIBUTE = "google";
	const string AMAZON_ATTRIBUTE = "amazon";

	CaletyGameStoreListener m_storeListener;
	string[] m_storeSkus;

    private bool m_isFirstInit;
    
    public GameStoreManagerCalety () 
	{
		m_storeListener = new CaletyGameStoreListener();
        m_isFirstInit = true;
    }

    private void Reset()
    {
        m_isFirstInit = true;
        m_storeListener.Reset();
    }

	public override void Initialize()
	{
        if (m_isFirstInit)
        {
            Messenger.AddListener(MessengerEvents.CONNECTION_RECOVERED, OnConnectionRecovered);            

            StoreManager.SharedInstance.AddListener(m_storeListener);
            CacheStoreSkus();

			m_isFirstInit = false;
        }

		m_storeListener.Reset();
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

    private void OnConnectionRecovered()
    {
        TryToSolveInitializeProblems();
    }

    private void TryToSolveInitializeProblems()
    {
		if (FeatureSettingsManager.IsDebugEnabled)
			Debug.Log("TryToSolveInitializedProblems isReady = " + IsReady() + " HasInitFailed = " + m_storeListener.HasInitFailed());
		
        // Checks if there was an initialize problem
        if (!IsReady() && m_storeListener.HasInitFailed())
        {
            m_storeListener.Reset();
            StoreManager.SharedInstance.Initialise(ref m_storeSkus, false);
        }
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
