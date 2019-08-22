using SimpleJSON;
using System.Collections.Generic;
public class Transaction
{
    private const string KEY_ID = "order_id";
    private const string KEY_SOURCE = "source";
    private const string KEY_ITEMS = "items";

    private const string SOURCE_SHOP = "shop";
    private const string SOURCE_CUSTOMER_SUPPORT = "crm";

    private static HDTrackingManager.EEconomyGroup GetEconomyGroupFromSource(string source)
    {
        HDTrackingManager.EEconomyGroup returnValue = HDTrackingManager.EEconomyGroup.UNKNOWN;
        switch (source)
        {
            case SOURCE_SHOP:
                returnValue = HDTrackingManager.EEconomyGroup.SHOP_PURCHASE_RESUMED;
                break;

            case SOURCE_CUSTOMER_SUPPORT:
                returnValue = HDTrackingManager.EEconomyGroup.CUSTOMER_SUPPORT;
                break;
        }

        return returnValue;
    }

    public const string KEY_SC = "sc";
    public const string KEY_HC = "hc";
    public const string KEY_GF = "gf";

    private const string KEY_ITEM_SKU = "sku";
    private const string KEY_ITEM_TYPE = "type";
    private const string KEY_ITEM_AMOUNT = "amount";

    /// <summary>
    /// Array containing the keys used by server to identify every currency type.
    /// </summary>
    private static List<string> KEYS_CURRENCIES = new List<string>
    {
        "none",
        KEY_SC,
        KEY_HC,
        KEY_GF
    };

    private static int KEYS_CURRENCIES_COUNT = KEYS_CURRENCIES.Count;

    private static UserProfile.Currency GetCurrencyFromKey(string key)
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

            case KEY_GF:
                returnValue = UserProfile.Currency.GOLDEN_FRAGMENTS;
                break;
        }

        return returnValue;
    }

    private static string GetKeyFromCurrency(UserProfile.Currency currency)
    {
        string returnValue = null;
        for (int i = 0; i < KEYS_CURRENCIES_COUNT && returnValue == null; i++)
        {
            if (GetCurrencyFromKey(KEYS_CURRENCIES[i]) == currency)
            {
                returnValue = KEYS_CURRENCIES[i];
            }
        }

        return returnValue;
    }

    private Dictionary<UserProfile.Currency, Metagame.Reward> m_rewardCurrencies;    

    internal class ItemData
    {
        internal Metagame.Reward.Data data { get; set; }
        internal Metagame.Reward reward { get; set; }

        internal ItemData(Metagame.Reward.Data data, Metagame.Reward reward)
        {
            this.data = data;
            this.reward = reward;
        }
    };

    private List<ItemData> m_rewardItems;

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

    private void Reset()
    {
        SetId(null);
        SetSource(null);
        SetIsValid(false);
        SetHasBeenPerformed(false);

        if (m_rewardCurrencies != null)
        {
            m_rewardCurrencies.Clear();
        }        

        if (m_rewardItems != null)
        {
            m_rewardItems.Clear();
        }
    }    

    /// <summary>
    /// Sets up the transaction out of a <c>JSONNode</c>
    /// </summary>
    /// <param name="json">JSON to take information from to create the transaction</param>
    /// <returns>Whether or not the transaction can be performed</returns>
    public bool FromJSON(JSONNode json)
    {
        // Example { "orderId":"1", "source":"crm", "hc":"1" "sc":"100", "items":[{"sku":"egg_event", "type":"egg", "amount":"1"}]}
        Reset();

        if (json != null)
        {
            SetId(json[KEY_ID]);
            SetSource(json[KEY_SOURCE]);

            // Resources
            long amount;
            string key;
            UserProfile.Currency currency;            
            Metagame.Reward currentReward;
            Metagame.Reward rewardToAdd;
            bool isValid = true;

            //
            // Currencies
            //
            for (int i = 0; i < KEYS_CURRENCIES_COUNT && isValid; i++)
            {
                key = KEYS_CURRENCIES[i];
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
                        amount = json[key].AsInt;

                        // Checks if there's already an object of this currency type
                        currentReward = GetRewardCurrency(currency);
                        if (currentReward != null)
                        {
                            amount += currentReward.amount;
                        }

                        rewardToAdd = CreateRewardCurrency(currency, amount);

                        if (rewardToAdd == null)
                        {                                                     
                            TransactionManager.LogWarning("No support to translate currency type " + currency + " into Metaga.Reward");                            
                        }
                        else
                        {
                            AddRewardCurrency(currency, rewardToAdd);
                        }                                                
                    }
                }
            }

            // 
            // Items
            //
            if (json.ContainsKey(KEY_ITEMS))
            {
                JSONNode items = json[KEY_ITEMS];
                if (items != null)
                {
                    // Process every transaction
                    JSONArray itemsArray = items.AsArray;
                    if (itemsArray != null)
                    {
                        int count = itemsArray.Count;
                        JSONNode itemNode;                        
                        for (int i = 0; i < count; i++)
                        {
                            itemNode = itemsArray[i];

                            amount = itemNode[KEY_ITEM_AMOUNT].AsInt;
                            if (amount > 0)
                            {   
								// An issue was reported (HDK-2164) caused by using multiegg reward implementation. We're fixing that issue, in the meantime we just avoid using it by
								// creating as many eggs as needed
								for(int j = 0; j < amount; j++) 
								{
									AddRewardItemData(CreateRewardItemData(itemNode[KEY_ITEM_SKU], itemNode[KEY_ITEM_TYPE], 1));
								}
                            }
                        }
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

        //
        // Currencies
        //
        if (m_rewardCurrencies != null)
        {
            string key;
            foreach (KeyValuePair<UserProfile.Currency, Metagame.Reward> entry in m_rewardCurrencies)
            {
                if (entry.Value != null)
                {
                    key = GetKeyFromCurrency(entry.Key);
                    returnValue[key] = entry.Value.amount.ToString();
                }
            }           
        }

        //
        // Items
        //
        if (m_rewardItems != null)
        {
            int count = m_rewardItems.Count;
            if (count > 0)
            {
                // Dragons
                JSONArray items = new JSONArray();
                JSONNode itemNode;
                for (int i = 0; i < count; i++)
                {
                    if (m_rewardItems[i] != null)
                    {
                        itemNode = new JSONClass();
                        itemNode[KEY_ITEM_SKU] = m_rewardItems[i].data.sku;
                        itemNode[KEY_ITEM_TYPE] = m_rewardItems[i].data.typeCode;
                        itemNode[KEY_ITEM_AMOUNT] = m_rewardItems[i].data.amount.ToString();

                        items.Add(itemNode);
                    }
                }

                returnValue.Add(KEY_ITEMS, items);
            }
        }

        return returnValue;
    }

    public bool CanPerform()
    {
        // The transaction has to be valid and not performed yet
        bool returnValue = IsValid() && !HasBeenPerformed();       
        return returnValue;
    }

    public bool Perform(EPerformType performType)
    {		
        bool canPerform = CanPerform();
		
        TransactionManager.Log("Trying to perform transaction " + ToJSON().ToString() + " canPerform = " + canPerform);

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
        else
        {
            TransactionManager.LogWarning("Transaction can't be performed");
        }

        return canPerform;
    }

    private void PerformDirect()
    {
        //
        // Currencies
        //
        if (m_rewardCurrencies != null)
        {            
            foreach (KeyValuePair<UserProfile.Currency, Metagame.Reward> entry in m_rewardCurrencies)
            {
                if (entry.Value != null)
                {
                    entry.Value.Collect();
                }
            }
        }

        //
        // Items
        //
        if (m_rewardItems != null)
        {
            int count = m_rewardItems.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_rewardItems[i] != null && m_rewardItems[i].reward != null)
                {
                    m_rewardItems[i].reward.Collect();
                }
            }
        }
    }

    private void AddToUserProfile()
    {
        //
        // Currencies
        //
        UserProfile user = UsersManager.currentUser;
        if (m_rewardCurrencies != null)
        {
            foreach (KeyValuePair<UserProfile.Currency, Metagame.Reward> entry in m_rewardCurrencies)
            {
                if (entry.Value != null)
                {
                    user.PushReward(entry.Value);
                }
            }
        }

        //
        // Items
        //
        if (m_rewardItems != null)
        {
            int count = m_rewardItems.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_rewardItems[i] != null && m_rewardItems[i].reward != null)
                {
                    user.PushReward(m_rewardItems[i].reward);
                }
            }
        }

        // Save current profile state in case the open egg flow is interrupted
        PersistenceFacade.instance.Save_Request(true);
    }

    private Metagame.Reward CreateRewardCurrency(UserProfile.Currency currency, long amount)    
    {
        Metagame.Reward returnValue = null;

        string source = GetSource();
        HDTrackingManager.EEconomyGroup economyGroup = GetEconomyGroupFromSource(source);
        switch (currency)
        {
            case UserProfile.Currency.SOFT:
                returnValue = Metagame.Reward.CreateTypeSoftCurrency(amount, economyGroup, source);
                break;

            case UserProfile.Currency.HARD:
                returnValue = Metagame.Reward.CreateTypeHardCurrency(amount, economyGroup, source);
                break;

            case UserProfile.Currency.GOLDEN_FRAGMENTS:
                // Rarity is unknown at this place so we just use COMMON. It's not important because it's used only for visuals. We can't use UNKNOWN because the visuals crash with that rarity
                returnValue = Metagame.Reward.CreateTypeGoldenFragments((int)amount, Metagame.Reward.Rarity.COMMON, economyGroup, source);
                break;
        }       

        return returnValue;
    }    

    private void AddRewardCurrency(UserProfile.Currency currency, Metagame.Reward reward)
    {		
		TransactionManager.Log("Add Currency " + currency.ToString());
		
        if (m_rewardCurrencies == null)
        {
            m_rewardCurrencies = new Dictionary<UserProfile.Currency, Metagame.Reward>();
        }

        if (m_rewardCurrencies.ContainsKey(currency))
        {
            m_rewardCurrencies[currency] = reward;
        }
        else
        {
            m_rewardCurrencies.Add(currency, reward);
        }
    }

    private Metagame.Reward GetRewardCurrency(UserProfile.Currency currency)
    {
        return (m_rewardCurrencies != null && m_rewardCurrencies.ContainsKey(currency)) ? m_rewardCurrencies[currency] : null;        
    }

    private ItemData CreateRewardItemData(string sku, string type, long amount)
    {		
	    TransactionManager.Log("CreateRewardItemData sku = " + sku + " type = " + type + " amount = " + amount);
		
        string source = GetSource();        
        Metagame.Reward.Data data = new Metagame.Reward.Data();
        data.sku = sku;
        data.typeCode = type;
        data.amount = amount;
        Metagame.Reward reward = Metagame.Reward.CreateFromData(data, GetEconomyGroupFromSource(source), source);

        return new ItemData(data, reward);
    }

    private void AddRewardItemData(ItemData itemData)
    {        
        if (m_rewardItems == null)
        {
            m_rewardItems = new List<ItemData>();
        }

        m_rewardItems.Add(itemData);        
    }
}