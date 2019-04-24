﻿using System;
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
        private bool m_isInitialising = false;
    	public bool m_isReady = false;
        private bool m_hasInitFailed = false;
        private GameStoreManagerCalety m_manager;

        public CaletyGameStoreListener(GameStoreManagerCalety manager) : base()
        {
            Reset();
            m_manager = manager;
        }

        public void Reset()
        {
            m_isReady = false;
            m_hasInitFailed = false;
            m_isInitialising = false;
        }

        public void Initialise()
        {
            m_isInitialising = false;
        }

        public bool IsInitialising()
        {
            return m_isInitialising;
        }

        public void InitialiseStore(ref string[] storeSKUs, bool bAvoidVerification = false)
        {
            Reset();
            m_isInitialising = true;
            StoreManager.SharedInstance.Initialise(ref storeSKUs, bAvoidVerification);
        }

#if UNITY_EDITOR
        public void FakeInit(bool success)
        {
            m_isInitialising = false;
            m_hasInitFailed = !success;
            if (success)
            {
                onStoreIsReady();
            }
        }
#endif        

        public override void onPurchaseCompleted(string sku, string strTransactionID, JSONNode kReceiptJSON, string strPlatformOrderID) 
		{
            string purchaseSkuTriggered = m_manager.GetPurchaseSkuTriggeredByUser();

            if (FeatureSettingsManager.IsDebugEnabled)
            {
                string msg = "onPurchaseCompleted sku = " + sku + " purchaseSkuTriggeredByUser = " + purchaseSkuTriggered + " strTransactionID = " + strTransactionID + " strPlatformOrderID = " + strPlatformOrderID + " receipt = ";
                if (kReceiptJSON == null)
                {
                    msg += "null";
                }
                else
                {
                    msg +=  kReceiptJSON.ToString();
                }

                Log(msg);
            }

            // If the user hasn't triggered this purchase then it means that this purchase is being resumed from a purchase that was interrupted in a previous session
            if (string.IsNullOrEmpty(purchaseSkuTriggered) || purchaseSkuTriggered != sku)
            {
                // We need to urge to request for pending transactions
				Log("Pending transactions are urged because of an interrupted IAP");
                TransactionManager.instance.Pending_UrgeToRequestTransactions();
            }
            else
            {
                bool needsServerConfirmation = FeatureSettingsManager.instance.NeedPendingTransactionsServerConfirm();
                System.Action onDone = delegate ()
                {
                    if (!needsServerConfirmation)
                    {
                        TransactionManager.instance.Given_AddTransactionId(strTransactionID, true);
                    }

					// string gameSku = PlatformSkuToGameSku( sku );
					Log("PURCHASE_SUCCESSFUL Broadcast " + sku);
                    Messenger.Broadcast<string, string, JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, sku, strTransactionID, kReceiptJSON);
                };

                System.Action<bool> onConfirmDone = delegate (bool success)
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                    {
                        Log("Server confirmation for purchase " + sku + " received with success = " + success);
                    }

                    if (needsServerConfirmation)
                    {
                        onDone();
                    }
                    else
                    {
                        TransactionManager.instance.Given_RemoveTransactionId(strTransactionID);
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

            m_manager.OnPurchaseDone();
		}       

        public override void onPurchaseCancelled(string sku, string strTransactionID) 
		{
            string purchaseSkuTriggered = m_manager.GetPurchaseSkuTriggeredByUser();

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("onPurchaseCancelled sku completed " + sku + " purchaseSkuTriggeredByUser = " + purchaseSkuTriggered);

			Messenger.Broadcast<string>(MessengerEvents.PURCHASE_CANCELLED, purchaseSkuTriggered);

            m_manager.OnPurchaseDone();
		}

		public override void onPurchaseFailed(string sku, string strTransactionID) 
		{
            string purchaseSkuTriggered = m_manager.GetPurchaseSkuTriggeredByUser();

            if (FeatureSettingsManager.IsDebugEnabled)
				Log("onPurchaseFailed sku completed = " + sku + " purchaseSkuTriggeredByUser = " + purchaseSkuTriggered + " transactionID = " + strTransactionID);

			Messenger.Broadcast<string>(MessengerEvents.PURCHASE_FAILED, purchaseSkuTriggered);

            m_manager.OnPurchaseDone();
        }

		public override void onStoreIsReady() 
		{
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("onStoreIsReady");

			m_isReady = true;
            OnInitialiseStoreDone();
        }

        private void OnInitialiseStoreDone()
        {
            m_isInitialising = false;
        }

		/// <summary>
		/// Ons the IAP promoted received. 
		/// </summary>
		/// <param name="strSku">String sku Product bought on the store.</param>
		public override void onIAPPromotedReceived (string strSku) 
		{
			m_manager.RegisterPromotedIAP(strSku);
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
                Log("onStoreIosInitFail errorCode = " + errorCode);

            m_hasInitFailed = true;
            OnInitialiseStoreDone();
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

    private string m_purchaseSkuTriggeredByUser;
    
    private Queue<string> m_promotedIAPs;

    private float m_waitForInitializationExpiresAt = -1f;
    private Action m_onWaitForInitializationDone;

    public GameStoreManagerCalety () 
	{
        m_promotedIAPs = new Queue<string>();
		m_storeListener = new CaletyGameStoreListener(this);
        m_isFirstInit = true;
        m_purchaseSkuTriggeredByUser = null;        
    }

    private void Reset()
    {        
        m_isFirstInit = true;
        m_storeListener.Reset();
        ResetWaitForInitialization();
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

        ResetWaitForInitialization();

        m_storeListener.InitialiseStore(ref m_storeSkus, false);

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

    public override bool IsInitializing()
    {
        return m_storeListener.IsInitialising();
    }

    private void OnConnectionRecovered()
    {
        TryToSolveInitializeProblems();
    }

    private void TryToSolveInitializeProblems()
    {
		if (FeatureSettingsManager.IsDebugEnabled)
			Log("TryToSolveInitializedProblems isReady = " + IsReady() + " HasInitFailed = " + m_storeListener.HasInitFailed());
		
        // Checks if there was an initialize problem
        if (!IsReady() && m_storeListener.HasInitFailed())
        {
            m_storeListener.InitialiseStore(ref m_storeSkus, false);
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

	public void RegisterPromotedIAP(string _sku) {
        m_promotedIAPs.Enqueue(_sku);
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


	public override bool CanMakePayment() {
		Log("CanMakePayment?");
		if(!string.IsNullOrEmpty(m_purchaseSkuTriggeredByUser))   // if purchase in process we dont let make more payments
		{
			Log("CanMakePayment? NO! Another purchase with sku " + m_purchaseSkuTriggeredByUser + " is running!");
			return false;
		}
#if UNITY_EDITOR
        return IsReady();
#else
		Log("CanMakePayment? Asking native library...");
		bool canMakePayment = StoreManager.SharedInstance.CanMakePayments();
		Log("CanMakePayment? Native library response is " + canMakePayment);
		return canMakePayment;
#endif
    }

	public override void Buy(string _sku) {
		Log("Buy() " + _sku);
		m_purchaseSkuTriggeredByUser = _sku;

#if UNITY_EDITOR
		StoreManager.SharedInstance.StartCoroutine(SimulatePurchase(_sku));
#else
    	if (StoreManager.SharedInstance.CanMakePayments()) 
    	{
    		// string item = GameSkuToPlatformSku( _sku );
			if ( !string.IsNullOrEmpty( _sku ) )
    		{
                HDTrackingManager.Instance.Notify_IAPStarted();
				Log("Requesting product " + _sku + " to the native store");
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
        //m_storeListener.onPurchaseFailed("UNKOWN", ""); // Simulate current store behaviour
    }

    void OnPurchaseDone()
    {
        m_purchaseSkuTriggeredByUser = null;
    }

    string GetPurchaseSkuTriggeredByUser()
    {
        return m_purchaseSkuTriggeredByUser;
    }


    public override bool HavePromotedIAPs() { 
        return m_promotedIAPs.Count > 0; 
    }

    public override string GetNextPromotedIAP() {
        return m_promotedIAPs.Dequeue(); 
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
    
    public override void WaitForInitialization(Action onDone, float timeOut=20f)
    {
        m_onWaitForInitializationDone = onDone;
        m_waitForInitializationExpiresAt = Time.realtimeSinceStartup + timeOut;
    }  
    
    private void ResetWaitForInitialization()
    {
        m_waitForInitializationExpiresAt = -1f;
        m_onWaitForInitializationDone = null;
    }

#if UNITY_EDITOR
    // Time for the shope to initialize. Increase this value if you want to test what happens when trying to purchase before the shop has been initialized
    // 40 seconds is the time that is taking to initialize with the current amount of products (86 on January 2019) on the actual device 
    private float m_fakeInitTimer = 5f;
#endif

    public override void Update()
    {
#if UNITY_EDITOR
        if (m_fakeInitTimer > 0f)
        {
            m_fakeInitTimer -= Time.deltaTime;
            if (m_fakeInitTimer <= 0f)
            {
                m_fakeInitTimer = -1f;

                // Pass true if you want the shop to be initialized successfully
                m_storeListener.FakeInit(true);
            }
        }
#endif
        if (m_onWaitForInitializationDone != null)
        {            
            if (Time.realtimeSinceStartup >= m_waitForInitializationExpiresAt || !IsInitializing())
            {                
                m_onWaitForInitializationDone();
                ResetWaitForInitialization();
            }            
        }
    }

    #region log
    private static void Log(string msg)
    {
		if(!FeatureSettingsManager.IsDebugEnabled) return;
        msg = "[GameStoreManagerCalety]" + msg;
        ControlPanel.Log(msg, ControlPanel.ELogChannel.Store);
    }

    private static void LogError(string msg)
    {
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		msg = "[GameStoreManagerCalety]" + msg;
        ControlPanel.LogError(msg, ControlPanel.ELogChannel.Store);
    }

    private static void LogWarning(string msg)
    {
		if(!FeatureSettingsManager.IsDebugEnabled) return;
		msg = "[GameStoreManagerCalety]" + msg;
        ControlPanel.LogWarning(msg, ControlPanel.ELogChannel.Store);
    }
    #endregion
}
