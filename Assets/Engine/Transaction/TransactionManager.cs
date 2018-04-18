using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for handling transactions. A transaction is the way to add something to the user's profile, typically either sc or pc
/// </summary>
public class TransactionManager : UbiBCN.SingletonMonoBehaviour<TransactionManager>
{    
    public void Initialise()
    {
        Reset();
    }

    public void Reset()
    {
        Pending_Reset();
        Flow_Reset();
    }

    public void Update()
    {
        Pending_Update();

#if UNITY_EDITOR
        if (DEBUG_ENABLED)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Requests pending transactions
                Pending_ForceRequestTransactions();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                // Test pending transactions flow (chain of popups giving resources)
                Debug_TestPendingTransactionsFlow();
            }
        }
#endif    
    }

    #region factory
    private Transaction Factory_GetTransaction()
    {
        return new Transaction();        
    }
    #endregion

    #region pending
    /// This region is responsible for handling pending transactions, which are transactions received from server that are pending to be applied to the user's profile. 
    /// These pending transactions could have different sources but for now they all come from customer support.    
    
    private float m_pendingTimeToRequest;

    private Queue<Transaction> m_pendingTransactions;

    private const float PENDING_TIME_BETWEEN_REQUESTS = 10 * 60f;

    private enum EState
    {
        None,
        WaitingToRequest,
        WaitingForResponse,
        WaitingToPresent,
        Presenting
    };

    private EState m_pendingState;

    private void Pending_Reset()
    {        
        Pending_SetState(EState.WaitingToRequest);

        // We want the pending transactions to be requested immediately when the game is launched so the user can get the rewards as soon as she's told to
        Pending_SetTimeToRequest(0);

        if (m_pendingTransactions != null)
        {
            m_pendingTransactions.Clear();
        }
    }

    private EState Pending_GetState()
    {
        return m_pendingState;
    }

    private void Pending_SetState(EState value)
    {
        switch (m_pendingState)
        {
            case EState.Presenting:
                m_flowCoroutine = null;
                break;
        }

        m_pendingState = value;

        switch (m_pendingState)
        {
            case EState.WaitingToRequest:
                Pending_SetTimeToRequest(PENDING_TIME_BETWEEN_REQUESTS);
                break;            
        }
    }

    private void Pending_Update()
    {
        switch (Pending_GetState())
        {
            case EState.WaitingToRequest:
                float time = Pending_GetTimeToRequest();
                if (time > 0f)
                {
                    time -= Time.deltaTime;
                    Pending_SetTimeToRequest(time);
                }

                // Check if it's a good moment to request the pending transactions
                if (time <= 0f && !FlowManager.IsInGameScene())
                {
                    Pending_RequestTransactions();
                }
                break;
        }
    }

    private float Pending_GetTimeToRequest()
    {
        return m_pendingTimeToRequest;
    }

    private void Pending_SetTimeToRequest(float value)
    {
        m_pendingTimeToRequest = value;
    }

    /// <summary>
    /// It should be used only for debug purposes. By calling this event the pending transactions request is forced to be sent so the flow can be easily
    /// </summary>
    private void Pending_ForceRequestTransactions()
    {
        Pending_SetState(EState.WaitingToRequest);        
        Pending_SetTimeToRequest(0f);
    }

    private void Pending_RequestTransactions()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Requesting pending transactions...");

        GameServerManager.SharedInstance.GetPendingTransactions(Pending_OnTransactionsResponse);        
        Pending_SetState(EState.WaitingForResponse);
    }

    private void Pending_OnTransactionsResponse(FGOL.Server.Error error, GameServerManager.ServerResponse response)
    {        
        // Previous pending transactions are reseted because the server is responsible for providing with all pending transactions for this user
        if (m_pendingTransactions != null)
        {
            m_pendingTransactions.Clear();
        }

        if (FeatureSettingsManager.IsDebugEnabled)
            Log("OnPendingTransactionResponse: " + ((response == null) ? " no response" : response.ToString()));

        if (error == null && response != null)
        {            
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("OnPendingTransactionResponse: " + response.ToString());

            if (response.ContainsKey("response"))
            {
                string responseAsString = response["response"] as string;
                if (!string.IsNullOrEmpty(responseAsString))
                {
                    JSONNode json = JSON.Parse(responseAsString);
                    if (json != null)
                    {                    
                        JSONNode txs = json["txs"];
                        if (txs != null)
                        {
                            bool isValid;

                            // Process every transaction
                            JSONArray transactions = txs.AsArray;
                            if (transactions != null)
                            {
                                int count = transactions.Count;
                                JSONNode transactionNode;
                                Transaction transaction;
                                for (int i = 0; i < count; i++)
                                {
                                    transactionNode = transactions[i];
                                    transaction = Factory_GetTransaction();
                                    isValid = transaction.FromJSON(transactionNode);
                                    if (isValid)
                                    {
                                        // UI is prepared to give only a resource type (either sc or pc), so if the transaction contains more than one resource type an error is shown and the 
                                        // transaction is ignored
                                        if (transaction.GetResourceTypesAmount() == 1)
                                        {
                                            Pending_AddTransaction(transaction);
                                        }
                                        else if (FeatureSettingsManager.IsDebugEnabled)
                                        {
                                            LogError("Transaction " + transactionNode.ToString() + " received from the server contains more than one resource type and the client only support one resource type transactions so it's ignored");
                                        }
                                    }
                                    else if (FeatureSettingsManager.IsDebugEnabled)
                                    {
                                        LogError("Transaction " + transactionNode.ToString() + " received from the server is not supported by the client so it's ignored");
                                    }
                                }
                            }
                        }
                    }
                }                
            }
        }
        else
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                LogWarning("Error when requesting pending transactions " + error.ToString());
        }
        
        if (Pending_IsEmpty())
        {
            Pending_SetState(EState.WaitingToRequest);
        }
        else
        {
            Pending_SetState(EState.WaitingToPresent);
        }
        /*
        // Uncomment this piece of code to test a transaction that wasn't received from server
        if (m_pendingTransactions == null || m_pendingTransactions.Count == 0)
        {
            JSONNode transactionJSON = new JSONClass();
            transactionJSON["id"] = "1";
            transactionJSON["source"] = "support";
            transactionJSON["hc"] = 1;

            Transaction transaction = Factory_GetTransaction();
            bool isValid = transaction.FromJSON(transactionJSON);
            if (isValid)
            {
                Pending_AddTransaction(transaction);
            }            
        }*/
    }    

    private void Pending_AddTransaction(Transaction value)
    {
        if (m_pendingTransactions == null)
        {
            m_pendingTransactions = new Queue<Transaction>();
        }

        m_pendingTransactions.Enqueue(value);
    }

    private bool Pending_IsEmpty()
    {
        return m_pendingTransactions == null || m_pendingTransactions.Count == 0;
    }
    #endregion

    #region flow    
    // This region is responsible for handing the flow that gives the pending transactions to the user
    
    /// <summary>
    /// Coroutine used to give all pending transactions to the user
    /// </summary>
    private Coroutine m_flowCoroutine = null;
    
    /// <summary>
    /// Popup currently open by the flow
    /// </summary>
    private PopupController m_flowCurrentPopup;

    /// <summary>
    /// <c>RectTransform</c> of the canvas where the feedback for currencies will be displayed
    /// </summary>
    private RectTransform m_flowUICanvasRectTransform;

    private bool Flow_IsProcessingPopup()
    {
        return m_flowCurrentPopup != null;
    }    

    private void Flow_Reset()
    {
        if (m_flowCoroutine != null)
        {
            StopCoroutine(m_flowCoroutine);
            m_flowCoroutine = null;
        }

        Flow_CloseCurrentPendingTransactionPopup();

        m_flowUICanvasRectTransform = null;                  
    }

    private bool Flow_ArePendingTransactionsUnlocked()
    {
        // Pending transactions are unlocked after egg info tutorial has being shown to prevent this flow from messing up with tutorials
        return UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INFO);
    }

    public bool Flow_NeedsToShowPendingTransactions()
    {
        // Pending transactions are unlocked and there are some to show
        return Flow_ArePendingTransactionsUnlocked() && !Pending_IsEmpty();
    }

    /// <summary>
    /// Triggers the flow that gives the pending transactions to the user should be triggered
    /// </summary>
    /// <param name="uiCanvas"><c>GameObject</c> of the UI canvas where the feedback for gaining currencies needs to be displayed</param>    
    public void Flow_PerformPendingTransactions(GameObject uiCanvas)
    {
        m_flowUICanvasRectTransform = (uiCanvas) == null ? null : uiCanvas.transform as RectTransform;                
        if (Flow_NeedsToShowPendingTransactions())
        {
            // m_flowCoroutine should be null at this point. If that's not the case, just in case, it's stopped
            if (m_flowCoroutine != null)
            {
                StopCoroutine(m_flowCoroutine);
            }

            m_flowCoroutine = StartCoroutine(Flow_PerformPendingTransactions());
            Pending_SetState(EState.Presenting);
        }        
    }

    private IEnumerator Flow_PerformPendingTransactions()
    {
        Transaction transaction;
        while (!Pending_IsEmpty())
        {
            transaction = m_pendingTransactions.Peek();
            Flow_OpenPendingTransactionPopup(transaction);

            while (Flow_IsProcessingPopup())
            {
                yield return null;
            }            
        }

        Pending_SetState(EState.WaitingToRequest);        
    }

    private void Flow_OpenPendingTransactionPopup(Transaction transaction)
    {
        // This popup only supports hc and sc for now        
        UserProfile.Currency currency = Flow_GetCurrency(transaction);        
        m_flowCurrentPopup = PopupManager.LoadPopup(PopupTransaction.PATH);
        m_flowCurrentPopup.GetComponent<PopupTransaction>().Init(transaction.GetCurrencyAmount(currency), currency, Flow_OnAcceptPendingTransaction, Flow_OnClosePendingTransactionPopup);
        m_flowCurrentPopup.Open();                        
    }

    /// <summary>
    /// Callback called when the user accepts the transaction by clicking on "Accept" button in the popup
    /// </summary>
    private void Flow_OnAcceptPendingTransaction()
    {
        PersistenceFacade.Popups_OpenLoadingPopup();

        // Sends a request to the server to confirm that the user is allowed to collect this transaction and in order to let the server delete this transaction from the list of pending transactions
        Transaction transaction = m_pendingTransactions.Peek();
        
        // Uncomment to test that the server is able to confirm a set of transactions
        //List<Transaction> list = new List<Transaction>(m_pendingTransactions);
        //GameServerManager.SharedInstance.ConfirmPendingTransactions(list, Flow_OnConfirmPendingTransaction);

        GameServerManager.SharedInstance.ConfirmPendingTransaction(transaction, Flow_OnConfirmPendingTransaction);
    }
    
    private void Flow_OnClosePendingTransactionPopup()
    {
        // A popup notifying the user that she will have the chance to collect her pending transactions later is opened
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_TRANSACTION_CLOSE_TITLE";
        config.MessageTid = "TID_TRANSACTION_CLOSE_DESC";        
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;

        // All pending transactions are cancelled because we assume that the user wants to skip this flow
        config.OnConfirm = Flow_OnCancelPendingTransactions;
        config.ConfirmButtonTid = "TID_GEN_OK";
        config.IsButtonCloseVisible = false;
        config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.Close;
        PopupManager.PopupMessage_Open(config);
    }

    private const int FLOW_INTERNAL_ERROR_TRANSACTIONS_MISMATCH = 77;
    private const int FLOW_INTERNAL_ERROR_GENERIC = 78;
    private const int FLOW_INTERNAL_ERROR_NO_RESPONSE = 79;

    private void Flow_OnConfirmPendingTransaction(FGOL.Server.Error error, GameServerManager.ServerResponse response)
    {        
        PersistenceFacade.Popups_CloseLoadingPopup();
        bool needsToClosePopup = true;

        bool success = error == null;
        int internalErrorCode = -1;        
        if (error == null)
        {
            if (response != null && response.ContainsKey("response"))
            {
                string responseAsString = response["response"] as string;
                if (!string.IsNullOrEmpty(responseAsString))
                {
                    JSONNode json = JSON.Parse(responseAsString);
                    if (json != null)
                    {
                        JSONNode txs = json["txs"];
                        if (txs == null)
                        {
                            internalErrorCode = FLOW_INTERNAL_ERROR_TRANSACTIONS_MISMATCH;
                        }
                        else
                        {                            
                            // Process every transaction
                            JSONArray transactions = txs.AsArray;
                            if (transactions != null)
                            {                                
                                if (transactions.Count != 1)
                                {
                                    internalErrorCode = FLOW_INTERNAL_ERROR_TRANSACTIONS_MISMATCH;
                                }
                                else
                                {
                                    Transaction transactionToPerform = m_pendingTransactions.Peek();
                                    Transaction transaction = Factory_GetTransaction();
                                    transaction.FromJSON(transactions[0]);                                    
                                    // Check if this transaction matches with the one that the client is processing
                                    if (!transactionToPerform.Equals(transaction))
                                    {
                                        internalErrorCode = FLOW_INTERNAL_ERROR_TRANSACTIONS_MISMATCH;
                                    }                                    
                                }
                            }
                        }

                        if (internalErrorCode != -1)
                        {
                            success = false;
                        }
                        else
                        {
                            string key = "result";
                            if (json.ContainsKey(key))
                            {
                                success = json[key].AsBool;
                            }

                            if (!success)
                            {
                                key = "code";
                                if (json.ContainsKey(key))
                                {
                                    internalErrorCode = json[key].AsInt;
                                }
                                else
                                {
                                    internalErrorCode = FLOW_INTERNAL_ERROR_GENERIC;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                internalErrorCode = FLOW_INTERNAL_ERROR_NO_RESPONSE;
            }
        }
        else if (error.code == FGOL.Server.ErrorCodes.LogicError)
        {
            internalErrorCode = error.m_errorCode;
        }

        if (success)
        { 
            // Removes the pending transaction being processed
            Transaction transaction = m_pendingTransactions.Dequeue();            

            // Apply the pending transaction
            bool transactionSuccess = transaction.Perform();            

            // Some visual feedback is launched
            if (transactionSuccess)
            {
                UserProfile.Currency currency = Flow_GetCurrency(transaction);
                if (currency == UserProfile.Currency.SOFT || currency == UserProfile.Currency.HARD)
                {
                    if (m_flowUICanvasRectTransform != null)
                    {
                        UINotificationShop.CreateAndLaunch(currency, transaction.GetCurrencyAmount(currency), Vector3.down * 150f, m_flowUICanvasRectTransform);
                    }
                }
            }
        }
        else
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("Error when confirming transaction code: " + internalErrorCode);
            }

            needsToClosePopup = false;

            if (internalErrorCode > -1)
            {
                Flow_OpenInternalError();
            }
            else
            {
                Flow_OpenNoConnectionError();
            }
        }        

        if (needsToClosePopup)
        {
            Flow_CloseCurrentPendingTransactionPopup();
        }
    }

    private UserProfile.Currency Flow_GetCurrency(Transaction transaction)
    {
        // So far only two currencies are supported
        UserProfile.Currency currency = UserProfile.Currency.NONE;
        if (transaction.GetCurrencyAmount(UserProfile.Currency.HARD) > 0)
        {
            currency = UserProfile.Currency.HARD;
        }
        else if (transaction.GetCurrencyAmount(UserProfile.Currency.SOFT) > 0)
        {
            currency = UserProfile.Currency.SOFT;
        }

        return currency;
    }

    /// <summary>
    /// Opens a popup that lets the user know that there's a problem with the network to encourage him to fix it and try again
    /// </summary>
    private void Flow_OpenNoConnectionError()
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_TRANSACTION_CONN_LOST_TITLE";
        config.MessageTid = "TID_TRANSACTION_CONN_LOST_DESC";        
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.ConfirmButtonTid = "TID_GEN_OK";
        config.IsButtonCloseVisible = false;        
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// Opens a popup that lets the user know that there was a problem on server side (typically the server hasn't confirmed the pending transactions that the user tried to perform)
    ///  so he should try later again
    /// </summary>
    private void Flow_OpenInternalError()
    {
        IPopupMessage.Config config = IPopupMessage.GetConfig();
        config.TitleTid = "TID_TRANSACTION_INTERNAL_ERR_TITLE";
        config.MessageTid = "TID_TRANSACTION_INTERNAL_ERR_DESC";
        config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = Flow_OnCancelPendingTransactions;
        config.ConfirmButtonTid = "TID_GEN_OK";
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }

    private void Flow_OnCancelCurrentPendingTransaction()
    {
        if (m_pendingTransactions != null && m_pendingTransactions.Count > 0)
        {
            m_pendingTransactions.Dequeue();
        }

        Flow_CloseCurrentPendingTransactionPopup();        
    }

    private void Flow_OnCancelPendingTransactions()
    {
        if (m_pendingTransactions != null)
        {
            m_pendingTransactions.Clear();
        }

        // If there's a popup open then it's closed
        Flow_CloseCurrentPendingTransactionPopup();
    }

    private void Flow_CloseCurrentPendingTransactionPopup()
    {
        // If there's a popup open then it's closed
        if (m_flowCurrentPopup != null)
        {
            m_flowCurrentPopup.Close(true);
            m_flowCurrentPopup = null;
        }
    }
    #endregion

    #region log
    private const string LOG_CHANNEL = "[TransactionsManager] ";
    public static void Log(string msg)
    {
        msg = LOG_CHANNEL + msg;

#if UNITY_EDITOR
        if (DEBUG_ENABLED)
        {
            msg = "<color=yellow>" + msg + "</color>";
        }
#endif

        Debug.Log(msg);
    }

    public static void LogWarning(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogWarning(msg);
    }

    public static void LogError(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogError(msg);
    }
    #endregion

    #region debug
    private const bool DEBUG_ENABLED = false;
    private void Debug_TestTransaction()
    {
        JSONNode json = new JSONClass();
        json["id"] = "1";
        json["source"] = "crm";
        json["pc"] = 1;
        json["sc"] = 10;

        Transaction transaction = Factory_GetTransaction();
        bool isValid = transaction.FromJSON(json);
        Log("Transaction " + json.ToString() + " isValid = " + isValid);
        transaction.Perform();

        Log("Transaction again " + json.ToString() + " isValid = " + isValid);
        transaction.Perform();        
    }

    private void Debug_TestPendingTransactionsFlow()
    {
        bool needsToPerform = Flow_NeedsToShowPendingTransactions();
        Log("Checking pending transactions flow... " + needsToPerform);
        if (needsToPerform)
        {
            GameObject uiCanvas = (m_flowUICanvasRectTransform == null) ? null : m_flowUICanvasRectTransform.gameObject;
            Flow_PerformPendingTransactions(uiCanvas);
        }        
    }
    #endregion
}