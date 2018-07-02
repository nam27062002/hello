public class TransactionResource
{
    private UserProfile.Currency m_currency;

    private int m_amount;

    public int GetAmount()
    {
        return m_amount;
    }

    private void SetAmount(int value)
    {
        m_amount = value;
    }

    public UserProfile.Currency GetCurrency()
    {
        return m_currency;
    }

    private void SetCurrency(UserProfile.Currency value)
    {
        m_currency = value;
    }

    public void Reset()
    {
        SetCurrency(UserProfile.Currency.NONE);
        SetAmount(0);
    }

    public void Setup(UserProfile.Currency currency, int amount)
    {
        Reset();
        SetCurrency(currency);
        SetAmount(amount);
    }

    public bool CanPerform()
    {
        // Only positive transactions are supported for now
        return GetCurrency() != UserProfile.Currency.NONE && GetAmount() > 0;
    }
    
    /// <summary>
    /// Perform the transaction.
    /// </summary>
    /// <returns><c>true</c> if the transaction was performed successfully. Otherwise <c>false</c></returns>
    public bool Perform()
    {
        bool canPerform = CanPerform();        
        if (canPerform)
        {
            // Only positive transactions are supported for now
            UsersManager.currentUser.EarnCurrency(GetCurrency(), (ulong)GetAmount(), false, HDTrackingManager.EEconomyGroup.CUSTOMER_SUPPORT);
            PersistenceFacade.instance.Save_Request(true);                   
        }

        return canPerform;
    }

    public override bool Equals(object obj)
    {
        bool returnValue = false;
        TransactionResource item = obj as TransactionResource;
        if (item != null)
        {
            returnValue = item.GetCurrency() == GetCurrency() && item.GetAmount() == GetAmount();            
        }

        return returnValue;
    }

    public Metagame.Reward ToReward(string source)
    {
        Metagame.Reward returnValue = null;

        switch (GetCurrency())
        {
            case UserProfile.Currency.SOFT:
                returnValue = Metagame.Reward.CreateTypeSoftCurrency(GetAmount(), HDTrackingManager.EEconomyGroup.CUSTOMER_SUPPORT, source);
                break;

            case UserProfile.Currency.HARD:
                returnValue = Metagame.Reward.CreateTypeHardCurrency(GetAmount(), HDTrackingManager.EEconomyGroup.CUSTOMER_SUPPORT, source);
                break;
        }

        if (FeatureSettingsManager.IsDebugEnabled && returnValue == null)
        {
            TransactionManager.LogWarning("No support to translate currency type " + GetCurrency() + " into Metaga.Reward");
        }

        return returnValue;      
    }
}