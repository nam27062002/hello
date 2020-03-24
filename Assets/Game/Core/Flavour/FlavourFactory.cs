#if UNITY_IOS
#define FACTORY_IOS
#endif

using System;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for creating a Flavour object depending on a country code and the device platform
/// </summary>
public class FlavourFactory
{
    public Flavour CreateFlavour()
    {
        return new Flavour();
    }

    public void SetupFlavour(Flavour flavour, string countryCode)
    {        
        if (!Catalog_IsInitialized())
        {
            Catalog_Initialize();
        }

        string flavourSku = Catalog_GetCountryCodeToFlavourSku(countryCode);
        SetupFlavourDelegate setupFlavourDelegate = Catalog_GetSetupFlavourDelegate(flavourSku);
        setupFlavourDelegate(flavour, flavourSku);        
    }    

    private void Worldwide_SetupFlavour(Flavour flavour, string sku)
    {
         flavour.Setup(
            sku: sku,
            socialPlatform:SocialUtils.EPlatform.Facebook,
            isSIWAEnabled:IsSIWAEnabled());
    }

    private void China_SetupFlavour(Flavour flavour, string sku)
    {
        flavour.Setup(
            sku: sku,
            socialPlatform: SocialUtils.EPlatform.Weibo,
            isSIWAEnabled: IsSIWAEnabled());
    }

    private bool IsSIWAEnabled()
    {
#if FACTORY_IOS && USE_SIWA
        return true;
#else
        return false;
#endif
    }

#region catalog
    // Flavour skus: So far "ww" is used for worldwide version and the country code for every country that requires a different flavour
    public const string CATALOG_SKU_WW = "WW";
    public const string CATALOG_SKU_CHINA = "CN";
    
    delegate void SetupFlavourDelegate(Flavour flavour, string sku);

    private Dictionary<string, SetupFlavourDelegate> m_catalog;
    private List<string> m_catalogSkus;

    private void Catalog_Initialize()
    {
        if (!Catalog_IsInitialized())
        {
            m_catalog = new Dictionary<string, SetupFlavourDelegate>();
            m_catalog.Add(CATALOG_SKU_WW, Worldwide_SetupFlavour);

#if FACTORY_IOS
            m_catalog.Add(CATALOG_SKU_CHINA, China_SetupFlavour);
#endif
        }
    }

    private bool Catalog_IsInitialized()
    {
        return m_catalog != null;
    }

    private string Catalog_GetCountryCodeToFlavourSku(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode))
        {
            countryCode = CATALOG_SKU_WW;
        }
        else
        {
            countryCode = countryCode.ToUpper();
        }

        return (m_catalog.ContainsKey(countryCode)) ? countryCode : CATALOG_SKU_WW;
    }

    private SetupFlavourDelegate Catalog_GetSetupFlavourDelegate(string flavourSku)
    {        
        string sku = (m_catalog.ContainsKey(flavourSku)) ? flavourSku : CATALOG_SKU_WW;
        return m_catalog[sku];        
    }

    public bool Catalog_ContainsSku(string sku)
    {
        return !string.IsNullOrEmpty(sku) && m_catalog.ContainsKey(sku);
    }

    public List<string> Catalog_GetSkus()
    {
        if (m_catalogSkus == null)
        {
            m_catalogSkus = new List<string>(m_catalog.Keys);
        }

        return m_catalogSkus;
    }
#endregion
}
