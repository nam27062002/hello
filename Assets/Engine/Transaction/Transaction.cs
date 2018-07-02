using SimpleJSON;
using System.Collections.Generic;

public class Transaction
{        
    private const string KEY_SC = "sc";
    private const string KEY_HC = "hc";

    /// <summary>
    /// Array containing the keys used by server to identify every currency type.
    /// </summary>
    private static List<string> KEYS = new List<string>
    {
        "none",
        KEY_SC,
        KEY_HC
    };

    private static int KEYS_COUNT = KEYS.Count;

    /// <summary>
    /// Id given by server to this transaction.
    /// </summary>
    private string m_id;

    /// <summary>
    /// Reason why the user is getting this transaction
    /// </summary>
    private string m_source;

    private bool m_isValid;

    private bool m_hasBeenPerformed;

    /// <summary>
    /// List of involving resource transactions that compose this transaction. Each simple transaction has 
    /// </summary>
    private List<TransactionResource> m_transactionResources;    

    public enum EPerformType
    {
        // The transaction is performed directly so the user gets the resources immediately
        Direct, 

        // The transaction is added to the user's profile as a reward so the user will receive it when the reward flow is triggered, typically when entering in dragon selection screen
        AddToUserProfile
    };    

    public Transaction()
    {
        Reset();
    }

    public string GetId()
    {
        return m_id;
    }

    public void SetId(string value)
    {
        m_id = value;
    }

    public string GetSource()
    {
        return m_source;
    }

    public void SetSource(string value)
    {
        m_source = value;
    }

    private bool IsValid()
    {
        return m_isValid;
    }

    private void SetIsValid(bool value)
    {
        m_isValid = value;
    }

    private bool HasBeenPerformed()
    {
        return m_hasBeenPerformed;
    }

    private void SetHasBeenPerformed(bool value)
    {
        m_hasBeenPerformed = value;
    }

    private UserProfile.Currency GetCurrencyFromKey(string key)
    {
        UserProfile.Currency returnValue = UserProfile.Currency.NONE;
        switch (key)
        {
            case KEY_SC:
                returnValue = UserProfile.Currency.SOFT;
                break;

            case KEY_HC:
                returnValue = UserProfile.Currency.HARD;
                break;
        }

        return returnValue;
    }

    private string GetKeyFromCurrency(UserProfile.Currency currency)
    {
        string returnValue = null;
        for (int i = 0; i < KEYS_COUNT && returnValue == null; i++)
        {
            if (GetCurrencyFromKey(KEYS[i]) == currency)
            {
                returnValue = KEYS[i];
            }
        }

        return returnValue;        
    }

    private void Reset()
    {                
        SetId(null);
        SetSource(null);
        SetIsValid(false);
        SetHasBeenPerformed(false);

        if (m_transactionResources != null)
        {
            m_transactionResources.Clear();
        }
    }

    private const string KEY_ID = "order_id";
    private const string KEY_SOURCE = "source";

    /// <summary>
    /// Sets up the transaction out of a <c>JSONNode</c>
    /// </summary>
    /// <param name="json">JSON to take information from to create the transaction</param>
    /// <returns>Whether or not the transaction can be performed</returns>
    public bool FromJSON(JSONNode json)
    {
        // Example { "id":"1", "source":"crm", "hc":"1" "sc":"100"}
        Reset();

        if (json != null)
        {
            SetId(json[KEY_ID]);
            SetSource(json[KEY_SOURCE]);

            // Resources
            string key;
            UserProfile.Currency currency;
            TransactionResource transactionResource;
            bool isValid = true;
            for (int i = 0; i < KEYS_COUNT; i++)
            {
                key = KEYS[i];
                if (json.ContainsKey(key))
                {
                    currency = GetCurrencyFromKey(key);
                    if (currency == UserProfile.Currency.NONE)
                    {
                        TransactionManager.LogWarning("Resource not supported: " + currency);
                        isValid = false;
                    }
                    else
                    {
                        transactionResource = GetTransactionResource(currency, json[key].AsInt);
                        AddTransactionResource(transactionResource);
                    }                    
                }
            }

            SetIsValid(isValid);                          
        }

        return CanPerform();
    }    

    public JSONNode ToJSON()
    {
        JSONNode returnValue = new JSONClass();
        string value = GetId();
        returnValue[KEY_ID] = (value == null) ? "" : value;

        value = GetSource();
        returnValue[KEY_SOURCE] = (value == null) ? "" : value;

        if (m_transactionResources != null)
        {
            string key;
            int count = m_transactionResources.Count;
            UserProfile.Currency currency;
            for (int i = 0; i < count; i++)
            {
                currency = m_transactionResources[i].GetCurrency();
                key = GetKeyFromCurrency(currency);
                if (string.IsNullOrEmpty(key))
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                    {
                        TransactionManager.LogWarning("Not supported currency: " + currency);
                    }
                }
                else
                {
                    returnValue[key] = m_transactionResources[i].GetAmount().ToString();
                }
            }
        }        

        return returnValue;
    }

    public bool CanPerform()
    {
        // The transaction has to be valid and not performed yet
        bool returnValue = IsValid() && !HasBeenPerformed();        
        if (returnValue && m_transactionResources != null)
        {
            int count = m_transactionResources.Count;
            for (int i = 0; i < count && returnValue; i++)
            {
                returnValue = m_transactionResources[i].CanPerform();
            }
        }

        return returnValue;
    }

    public bool Perform(EPerformType performType)
    {
        bool canPerform = CanPerform();
        if (canPerform)
        {
            switch (performType)
            {
                case EPerformType.Direct:
                    PerformDirect();
                    break;

                case EPerformType.AddToUserProfile:
                    AddToUserProfile();
                    break;
            }            

            SetHasBeenPerformed(true);
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            TransactionManager.LogWarning("Transaction can't be performed");
        }

        return canPerform;
    }    

    private TransactionResource GetTransactionResource(UserProfile.Currency currency, int amount)
    {
        TransactionResource returnValue = new TransactionResource();
        returnValue.Setup(currency, amount);

        return returnValue;
    }

    private void AddTransactionResource(TransactionResource value)
    {
        if (m_transactionResources == null)
        {
            m_transactionResources = new List<TransactionResource>();
        }

        m_transactionResources.Add(value);
    }

    public int GetResourceTypesAmount()
    {
        return (m_transactionResources == null) ? 0 : m_transactionResources.Count;        
    }  

    public int GetCurrencyAmount(UserProfile.Currency currency)
    {
        int returnValue = 0;

        if (m_transactionResources != null)
        {
            int count = m_transactionResources.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_transactionResources[i].GetCurrency() == currency)
                {
                    returnValue += m_transactionResources[i].GetAmount();
                }
            }
        }

        return returnValue;
    }

    public override bool Equals(object obj)
    {
        bool returnValue = false;
        Transaction item = obj as Transaction;
        if (item != null)
        {
            returnValue = item.GetId() == GetId() && item.GetSource() == GetSource() && item.GetResourceTypesAmount() == GetResourceTypesAmount();

            // Checks the currencies
            if (returnValue)
            {
                int count = GetResourceTypesAmount();
                for (int i = 0; i < count && returnValue; i++)
                {
                    returnValue = (m_transactionResources[i].Equals(item.m_transactionResources[i]));
                }
            }
        }

        return returnValue;
    }

    public List<Metagame.Reward> ToRewards()
    {
        List<Metagame.Reward> returnValue = new List<Metagame.Reward>();
        if (m_transactionResources != null)
        {
            int count = m_transactionResources.Count;
            Metagame.Reward reward;
            for (int i = 0; i < count; i++)
            {
                reward = m_transactionResources[i].ToReward(m_source);
                if (reward != null)
                {
                    returnValue.Add(reward);
                }
            }
        }

        return returnValue;
    }

    private void PerformDirect()
    {
        if (m_transactionResources != null)
        {
            int count = m_transactionResources.Count;
            for (int i = 0; i < count; i++)
            {
                m_transactionResources[i].Perform();
            }
        }
    }

    private void AddToUserProfile()
    {
        List<Metagame.Reward> rewards = ToRewards();
        if (rewards != null)
        {
            int count = rewards.Count;
            for (int i = 0; i < count; i++)
            {
                UsersManager.currentUser.PushReward(rewards[i]);
            }
        }
    }
}
 