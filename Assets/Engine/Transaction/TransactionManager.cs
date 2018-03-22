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

    private void Reset()
    {
        Pending_Reset();
        Flow_Reset();
    }

    public void Update()
    {
        // Check if it's a good moment to request for pending transactions (if the user is playing a run then we don't want to mess up with performance by requesting pending transactions because we don't consider this stuff is so urgent)
        if (m_pendingNeedsToRequest && !FlowManager.IsInGameScene())
        {
            Pending_RequestTransactions();
        }
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

    /// <summary>
    /// Whether or not the pending transactions request has already been sent to the server
    /// </summary>
    private bool m_pendingNeedsToRequest;

    private Queue<Transaction> m_pendingTransactions;

    private void Pending_Reset()
    {
        Pending_SetNeedsToRequest(true);
        if (m_pendingTransactions != null)
        {
            m_pendingTransactions.Clear();
        }
    }

    /// <summary>
    /// It should be used only for debug purposes. By calling this event the pending transactions request is forced to be sent so the flow can be easily
    /// </summary>
    public void Pending_ForceRequestTransactions()
    {
        Pending_SetNeedsToRequest(true);
    }

    private void Pending_RequestTransactions()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Requesting pending transactions...");

        GameServerManager.SharedInstance.GetPendingTransactions(Pending_OnTransactionsResponse);
        Pending_SetNeedsToRequest(false);
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

    private void Pending_SetNeedsToRequest(bool value)
    {
        m_pendingNeedsToRequest = value;
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

    private GameObject m_flowUICanvas;

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

        m_flowUICanvas = null;                  
    }

    private bool Flow_ArePendingTransactionsUnlocked()
    {
        // Pending transactions are unlocked after second run to prevent this flow from messing up with tutorials
        return UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN);
    }

    /// <summary>
    /// Checks whether or not the flow that gives the pending transactions to the user should be triggered
    /// </summary>
    /// <returns><c>true</c> if the flow that gives the pending transactions to the user should be triggerd. Otherwise <c>false</c></returns>
    public bool Flow_Check(GameObject uiCanvas)
    {
        m_flowUICanvas = uiCanvas;
        
        bool returnValue = Flow_ArePendingTransactionsUnlocked() && !Pending_IsEmpty();
        if (returnValue)
        {
            // m_flowCoroutine should be null at this point. If that's not the case, just in case, it's stopped
            if (m_flowCoroutine != null)
            {
                StopCoroutine(m_flowCoroutine);
            }

            m_flowCoroutine = StartCoroutine(Flow_PerformPendingTransactions());
        }

        return returnValue;
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
        m_flowCoroutine = null;
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
        GameServerManager.SharedInstance.ConfirmPendingTransaction(transaction, Flow_OnConfirmPendingTransaction);
    }
    
    private void Flow_OnClosePendingTransactionPopup()
    {
        // A popup notifying the user that she will have the chance to collect her pending transactions later is opened
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "Warning";
        config.MessageTid = "You'll have the chance to collect these transactions later";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;

        // All pending transactions are cancelled because we assume that the user wants to skip this flow
        config.OnConfirm = Flow_OnCancelPendingTransactions;                        
        config.CancelButtonTid = "Ok";
        config.IsButtonCloseVisible = false;
        config.BackButtonStrategy = PopupMessage.Config.EBackButtonStratety.Close;
        PopupManager.PopupMessage_Open(config);
    }   

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
                        if (txs != null)
                        {
                            // TODO: Check that transactions received from server and the ones that the client is about to perform are the same
                        }

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
                                internalErrorCode = 99;
                            }
                        }
                    }
                }
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
                    if (m_flowUICanvas != null)
                    {
                        UINotificationShop.CreateAndLaunch(currency, transaction.GetCurrencyAmount(currency), Vector3.down * 150f, m_flowUICanvas.transform as RectTransform);
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
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_CONNECTION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_CONNECTION_DESC";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;                      
        config.IsButtonCloseVisible = false;        
        PopupManager.PopupMessage_Open(config);
    }

    /// <summary>
    /// Opens a popup that lets the user know that there was a problem on server side (typically the server hasn't confirmed the pending transactions that the user tried to perform)
    ///  so he should try later again
    /// </summary>
    private void Flow_OpenInternalError()
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "Internal error";
        config.MessageTid = "Something went wrong when confirming the transaction. Don't worry you'll have the chance to collect it again later";        
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = Flow_OnCancelPendingTransactions;                        
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

        msg = "<color=yellow>" + msg + "</color>";        
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
    public void Debug_TestTransaction()
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

    public void Debug_TestPendingTransactionsFlow()
    {
        bool value = Flow_Check(m_flowUICanvas);
        Log("Checking pending transactions flow... " + value);
    }
    #endregion
}