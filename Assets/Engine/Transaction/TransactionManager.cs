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

    private List<Transaction> m_pendingTransactions;

    private const float PENDING_TIME_BETWEEN_REQUESTS = 10 * 60f;

    private enum EState
    {
        None,
        WaitingToRequest,
        WaitingForResponse,
        WaitingForConfirmation       
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
                                        Pending_AddTransaction(transaction);                                        
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
            Pending_SetState(EState.WaitingForConfirmation);
            GameServerManager.SharedInstance.ConfirmPendingTransactions(m_pendingTransactions, Pending_OnConfirmPendingTransactions);

            if (!FeatureSettingsManager.instance.NeedPendingTransactionsServerConfirm())
            {
                int count = m_pendingTransactions.Count;
                for (int i = 0; i < count; i++)                                
                {
                    m_pendingTransactions[i].Perform(Transaction.EPerformType.AddToUserProfile);
                }

                m_pendingTransactions.Clear();
            }            
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
            m_pendingTransactions = new List<Transaction>();
        }

        m_pendingTransactions.Add(value);        
    }   

    private bool Pending_IsEmpty()
    {
        return m_pendingTransactions == null || m_pendingTransactions.Count == 0;
    }

    private void Pending_OnConfirmPendingTransactions(FGOL.Server.Error error, GameServerManager.ServerResponse response)
    {
        if (FeatureSettingsManager.instance.NeedPendingTransactionsServerConfirm())
        {
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
                                // Process every transaction
                                JSONArray transactions = txs.AsArray;
                                Transaction transaction = Factory_GetTransaction();
                                if (transactions != null)
                                {
                                    int count = transactions.Count;
                                    for (int i = 0; i < count; i++)
                                    {
                                        transaction.FromJSON(transactions[i]);
                                        if (transaction.CanPerform())
                                        {
                                            transaction.Perform(Transaction.EPerformType.AddToUserProfile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        Pending_SetState(EState.WaitingToRequest);

        if (m_pendingTransactions != null)
        {
            m_pendingTransactions.Clear();
        }
    }

    public void Pending_ConfirmTransactionWithServer(Transaction transaction, System.Action<bool> onResult)
    {        
        if (onResult != null)
        {
            if (transaction == null)
            {
                onResult(false);
            }
            else
            {
                GameServerManager.SharedInstance.ConfirmPendingTransaction(transaction,
                       delegate (FGOL.Server.Error error, GameServerManager.ServerResponse response)
                       {
                           bool result = false;
                           if (error == null && response != null && response.ContainsKey("response"))
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
                                           // Process every transaction
                                           JSONArray transactions = txs.AsArray;
                                           if (transactions != null)
                                           {
                                               string transactionId = transaction.GetId();
                                               Transaction confirmedTransaction = Factory_GetTransaction();
                                               int count = transactions.Count;
                                               int i = 0;
                                               while (i < count && !result)
                                               {
                                                   confirmedTransaction.FromJSON(transactions[i]);
                                                   result = (confirmedTransaction.GetId() == transactionId);
                                                   i++;
                                               }
                                           }
                                       }
                                   }
                               }
                           }

                           onResult(result);
                       }
                    );
               }
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
        transaction.Perform(Transaction.EPerformType.Direct);

        Log("Transaction again " + json.ToString() + " isValid = " + isValid);
        transaction.Perform(Transaction.EPerformType.Direct);        
    }   
    #endregion
}