using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A price in any currency
/// </summary>
public class Price{

    private float m_amount;
    private UserProfile.Currency m_currency;

    public UserProfile.Currency Currency
    {
        get
        {
            return m_currency;
        }

        set
        {
            m_currency = value;
        }
    }

    public float Amount
    {
        get
        {
            return m_amount;
        }

        set
        {
            m_amount = value;
        }
    }

    // Constructor
    public Price(float m_amount, UserProfile.Currency m_currency)
    {
        this.m_amount = m_amount;
        this.m_currency = m_currency;
    }

}
