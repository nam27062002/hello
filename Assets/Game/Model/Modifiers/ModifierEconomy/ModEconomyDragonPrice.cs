﻿
public class ModEconomyDragonPrice : ModifierEconomy {
    public const string TARGET_CODE = "dragon_price";


    //------------------------------------------------------------------------//
	private string m_dragonSku;
	public string dragonSku {
		get { return m_dragonSku; }
	}

    private float m_percentage;
    private string m_currency;

    //------------------------------------------------------------------------//
    public ModEconomyDragonPrice(DefinitionNode _def) : base(TARGET_CODE, _def) {        
        m_dragonSku = _def.Get("param1");
        m_percentage = _def.GetAsFloat("param2");
        m_currency = _def.Get("param3");
        BuildTextParams(m_dragonSku, m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public ModEconomyDragonPrice(SimpleJSON.JSONNode _data) : base(TARGET_CODE, _data) {
        m_dragonSku = _data["param1"];
        m_percentage = PersistenceUtils.SafeParse<float>(_data["param2"]);
        m_currency = _data["param3"];
        BuildTextParams(m_dragonSku, m_percentage + "%", UIConstants.PET_CATEGORY_DEFAULT.ToHexString("#"));
    }

    public override void Apply() {
        IDragonData dragonData = DragonManager.GetDragonData(m_dragonSku);
        if (dragonData != null) {
            switch(m_currency) {
                case "sc": dragonData.AddPriceModifer(m_percentage, UserProfile.Currency.SOFT); break;
				case "hc": dragonData.AddPriceModifer(m_percentage, UserProfile.Currency.HARD); break;
            }
            Messenger.Broadcast<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, dragonData);
        }
    }

    public override void Remove() {
        IDragonData dragonData = DragonManager.GetDragonData(m_dragonSku);
        if (dragonData != null) {
            switch (m_currency) {
				case "sc": dragonData.AddPriceModifer(-m_percentage, UserProfile.Currency.SOFT); break;
				case "hc": dragonData.AddPriceModifer(-m_percentage, UserProfile.Currency.HARD); break;
            }
            Messenger.Broadcast<IDragonData>(MessengerEvents.MODIFIER_ECONOMY_DRAGON_PRICE_CHANGED, dragonData);
        }
    }

    protected override SimpleJSON.JSONClass __ToJson() {
        SimpleJSON.JSONClass data = base.__ToJson();

        data.Add("param1", m_dragonSku);
        data.Add("param2", PersistenceUtils.SafeToString(m_percentage));
        data.Add("param3", m_currency);

        return data;
    }
}